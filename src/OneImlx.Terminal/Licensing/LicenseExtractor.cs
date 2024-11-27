﻿/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OneImlx.Shared.Authorization;
using OneImlx.Shared.Extensions;
using OneImlx.Shared.Infrastructure;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseExtractor"/>
    /// </summary>
    public sealed class LicenseExtractor : ILicenseExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="licenseDebugger">The license debugger.</param>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The optional HTTP client factory</param>
        public LicenseExtractor(ILicenseDebugger licenseDebugger, TerminalOptions terminalOptions, ILogger<LicenseExtractor> logger, IHttpClientFactory? httpClientFactory = null)
        {
            this.licenseDebugger = licenseDebugger;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Extracts the <see cref="License"/> from the license keys.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<LicenseExtractorResult> ExtractLicenseAsync(LicenseExtractorContext context)
        {
            logger.LogDebug("Extract license.");

            // For singleton DI service we don't extract license keys once extracted.
            licenseExtractorResult ??= await ExtractFromJsonAsync();

            return licenseExtractorResult;
        }

        /// <inheritdoc/>
        public Task<License?> GetLicenseAsync()
        {
            logger.LogDebug("Get license.");
            return Task.FromResult(licenseExtractorResult?.License);
        }

        private static async Task<LicenseClaims> CheckOfflineLicenseAsync(LicenseCheck checkModel)
        {
            // Make sure validation key is not null
            if (checkModel.ValidationKey == null)
            {
                throw new TerminalException(TerminalErrors.MissingClaim, "The validation key is missing in the request.");
            }

            // https://stackoverflow.com/questions/58102904/how-to-verify-a-jwt-token-signed-with-x509-with-only-a-public-key-in-aspnetcore
            _ = LicenseX509Service.FromJwtValidationKey(checkModel.ValidationKey, out X509Certificate2 x509Certificate);
            X509SecurityKey validationKey = new(x509Certificate);

            // Init token validation params
            TokenValidationParameters validationParameters = new()
            {
                ValidateAudience = true,
                ValidAudience = checkModel.Audience,

                ValidateIssuer = true,
                ValidIssuer = checkModel.Issuer,

                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = [validationKey],

                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            // Validate the license key
            JsonWebTokenHandler jwtHandler = new();
            TokenValidationResult result = await jwtHandler.ValidateTokenAsync(checkModel.LicenseKey, validationParameters);
            if (!result.IsValid)
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "License key validation failed. info={0}", result.Exception.Message);
            }

            // Check Standard claims
            EnsureClaim(result, "tid", checkModel.TenantId, new Error(TerminalErrors.UnauthorizedAccess, "The tenant is not authorized. tid={0}", checkModel.TenantId));
            EnsureClaim(result, "apps", checkModel.Application, new Error(TerminalErrors.UnauthorizedAccess, "The application is not authorized. app={0}", checkModel.Application));

            return LicenseClaims.Create(result.Claims);
        }

        private async Task<HttpResponseMessage> CheckOnlineLicenseAsync(LicenseCheck checkModel)
        {
            // Setup the HTTP client
            HttpClient httpClient = EnsureHttpClient();

            // Primary and Secondary endpoints E.g. during certificate renewal the primary endpoints may fail so we fall
            // back to secondary endpoints.
            HttpResponseMessage httpResponseMessage;
            var checkContent = new StringContent(JsonSerializer.Serialize(checkModel), Encoding.UTF8, "application/json");
            try
            {
                httpResponseMessage = await httpClient.PostAsync(checkLicUrl, checkContent);
            }
            catch (HttpRequestException)
            {
                logger.LogWarning("The primary endpoint is not healthy. We are falling back to the secondary endpoint. Please contact the support team if you continue to see this warning after 24 hours.");
                httpResponseMessage = await httpClient.PostAsync(fallbackCheckLicUrl, checkContent);
            }
            return httpResponseMessage;
        }

        private static void EnsureClaim(TokenValidationResult result, string claim, object? expectedValue, Error error)
        {
            result.Claims.TryGetValue(claim, out object? jwtValue);

            // Both JSON claim and expected claim do not exist. E.g. oid claim is optional, so the JWT token may not
            // have it and if check still passes that then it is an error.
            if (jwtValue == null && expectedValue == null)
            {
                return;
            }

            if (jwtValue == null || !jwtValue.Equals(expectedValue))
            {
                throw new TerminalException(error);
            }
        }

        /// <summary>
        /// Should be called only for online check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TerminalException"></exception>
        private HttpClient EnsureHttpClient()
        {
            if (httpClientFactory == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The IHttpClientFactory is not configured.");
            }

            // Setup the HTTP client
            return httpClientFactory.CreateClient();
        }

        private async Task<LicenseExtractorResult> EnsureOfflineLicenseAsync(LicenseFile licenseFile)
        {
            logger.LogDebug("Extract offline license. id={0} tenant={1}", licenseFile.Id, licenseFile.TenantId);

            // If debugger is not attached and on-premise deployment is enabled then skip license check and grant claims
            // based on license plan. If debugger is attached we always do a license check.
            if (!licenseDebugger.IsDebuggerAttached() && IsOnPremiseDeployment(terminalOptions.Licensing.Deployment))
            {
                logger.LogDebug("Extract on-premise license. id={0} tenant={1}", licenseFile.Id, licenseFile.TenantId);

                if (terminalOptions.Licensing.LicensePlan == TerminalLicensePlans.OnPremise ||
                    terminalOptions.Licensing.LicensePlan == TerminalLicensePlans.Unlimited)
                {
                    logger.LogDebug("On-premise deployment is enabled. Skipping license check. plan={0} id={1} tenant={2}", terminalOptions.Licensing.LicensePlan, licenseFile.Id, licenseFile.TenantId);
                    return new LicenseExtractorResult(OnPremiseDeploymentLicense(), null);
                }
                else
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized for on-premise deployment. plan={0}", terminalOptions.Licensing.LicensePlan);
                }
            }
            else
            {
                // Check JWS signed assertion (JWS key)
                LicenseCheck checkModel = new()
                {
                    Issuer = Shared.Constants.Issuer,
                    Audience = AuthEndpoints.PiB2CIssuer(licenseFile.TenantId),
                    Application = terminalOptions.Id,
                    AuthorizedParty = Shared.Constants.TerminalUrn,
                    TenantId = licenseFile.TenantId,
                    LicenseKey = licenseFile.LicenseKey,
                    Id = licenseFile.Id,
                    ValidationKey = licenseFile.ValidationKey,
                    Mode = TerminalIdentifiers.OfflineLicenseMode
                };

                // Make sure we use the full base address
                LicenseClaims claims = await CheckOfflineLicenseAsync(checkModel);

                // Make sure the acr contains the
                string[] acrValues = claims.AcrValues.SplitBySpace();
                if (acrValues.Length < 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
                }

                string plan = acrValues[0];
                string usage = acrValues[1];
                string brokerTenantId = acrValues[2];

                // If the on-prem-deployment plan is set then check
                if (plan != terminalOptions.Licensing.LicensePlan)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized. expected={0} actual={1}", plan, terminalOptions.Licensing.LicensePlan);
                }

                // We just check the broker tenant to be present in offline mode
                if (string.IsNullOrWhiteSpace(brokerTenantId))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The broker tenant is missing.");
                }

                LicenseLimits licenseLimits = LicenseLimits.Create(plan, claims.Custom);
                LicensePrice licensePrice = LicensePrice.Create(plan, claims.Custom);
                return new LicenseExtractorResult
                (
                    new License(plan, usage, terminalOptions.Licensing.LicenseFile!, claims, licenseLimits, licensePrice),
                    TerminalIdentifiers.OfflineLicenseMode
                );
            }
        }

        private async Task<LicenseExtractorResult> EnsureOnlineLicenseAsync(LicenseFile licenseFile)
        {
            // Online license is obsolete
            logger.LogDebug("Extract online license. id={0} tenant={1}", licenseFile.Id, licenseFile.TenantId);

            // On_premise deployment is not supported for online license
            if (terminalOptions.Licensing.Deployment == TerminalIdentifiers.OnPremiseDeployment)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The on-premise deployment is not supported for online license.");
            }

            // Check JWS signed assertion (JWS key)
            LicenseCheck checkModel = new()
            {
                Issuer = Shared.Constants.Issuer,
                Audience = AuthEndpoints.PiB2CIssuer(licenseFile.TenantId),
                Application = terminalOptions.Id,
                AuthorizedParty = Shared.Constants.TerminalUrn,
                TenantId = licenseFile.TenantId,
                LicenseKey = licenseFile.LicenseKey,
                Id = licenseFile.Id,
                Mode = TerminalIdentifiers.OnlineLicenseMode
            };

            // Make sure we use the full base address
            using (HttpResponseMessage response = await CheckOnlineLicenseAsync(checkModel))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Error? error = await JsonSerializer.DeserializeAsync<Error>(await response.Content.ReadAsStreamAsync());
                    throw new TerminalException(error!);
                }

                LicenseClaims? claims = await JsonSerializer.DeserializeAsync<LicenseClaims>(await response.Content.ReadAsStreamAsync()) ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The license claims are invalid.");

                // Make sure the acr contains the
                string[] acrValues = claims.AcrValues.SplitBySpace();
                if (acrValues.Length < 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidLicense, "The acr values are not valid. acr={0}", claims.AcrValues);
                }

                string plan = acrValues[0];
                string usage = acrValues[1];
                string brokerTenantId = acrValues[2];

                // If the on-prem-deployment plan is set then check
                if (plan != terminalOptions.Licensing.LicensePlan)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not authorized. expected={0} actual={1}", plan, terminalOptions.Licensing.LicensePlan);
                }

                // We just check the broker tenant to be present in offline mode
                if (string.IsNullOrWhiteSpace(brokerTenantId))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The broker tenant is missing.");
                }

                // Online license is only valid for demo license
                if (plan != TerminalLicensePlans.Demo)
                {
                    logger.LogWarning("Online license check is obsolete and will be removed in future release. Please use offline license. id={0} tenant={1}", licenseFile.Id, licenseFile.TenantId);
                }

                LicenseLimits licenseLimits = LicenseLimits.Create(plan, claims.Custom);
                LicensePrice licensePrice = LicensePrice.Create(plan, claims.Custom);
                return new LicenseExtractorResult
                (
                    new License(plan, usage, terminalOptions.Licensing.LicenseFile!, claims, licenseLimits, licensePrice),
                    TerminalIdentifiers.OnlineLicenseMode
                );
            }
        }

        private async Task<LicenseExtractorResult> ExtractFromJsonAsync()
        {
            logger.LogDebug("Extract from JSON license key.");

            // Missing app id
            if (string.IsNullOrWhiteSpace(terminalOptions.Id))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The authorized application is not configured as a terminal identifier.");
            }

            // Missing key
            if (string.IsNullOrWhiteSpace(terminalOptions.Licensing.LicenseFile))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file is not configured.");
            }

            // License file needs to be a valid path if license contents is not set.
            if (terminalOptions.Licensing.LicenseContents == null && !File.Exists(terminalOptions.Licensing.LicenseFile))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file path is not valid. file={0}", terminalOptions.Licensing.LicenseFile);
            }

            // Missing or invalid license plan.
            if (!TerminalLicensePlans.IsValidPlan(terminalOptions.Licensing.LicensePlan))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license plan is not valid. plan={0}", terminalOptions.Licensing.LicensePlan);
            }

            // Avoid file locking for multiple threads.
            LicenseFile? licenseFile;
            await semaphoreSlim.WaitAsync();
            try
            {
                // Decode the license contents if set.
                if (terminalOptions.Licensing.LicenseContents != null)
                {
                    licenseFile = JsonSerializer.Deserialize<LicenseFile>(TerminalServices.DecodeLicenseContents(terminalOptions.Licensing.LicenseContents));
                }
                else
                {
                    // Make sure the lic stream is disposed to avoid locking.
                    using (Stream licStream = File.OpenRead(terminalOptions.Licensing.LicenseFile))
                    {
                        licenseFile = await JsonSerializer.DeserializeAsync<LicenseFile>(licStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file or contents are not valid. file={0} info={1}", terminalOptions.Licensing.LicenseFile, ex.Message);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            // Make sure the model is valid.
            if (licenseFile == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license file cannot be read. file={0}", terminalOptions.Licensing.LicenseFile);
            }

            // License check based on configured license handler.
            LicenseExtractorResult licenseExtractorResult;
            if (licenseFile.Mode == TerminalIdentifiers.OnlineLicenseMode)
            {
                licenseExtractorResult = await EnsureOnlineLicenseAsync(licenseFile);
            }
            else if (licenseFile.Mode == TerminalIdentifiers.OfflineLicenseMode)
            {
                licenseExtractorResult = await EnsureOfflineLicenseAsync(licenseFile);
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The license mode is not valid. mode={0}", licenseFile.Mode);
            }

            logger.LogDebug("Extracted license. plan={0} usage={1} subject={2} tenant={3}", licenseExtractorResult.License.Plan, licenseExtractorResult.License.Usage, licenseExtractorResult.License.Claims.Subject, licenseExtractorResult.License.Claims.TenantId);
            return licenseExtractorResult;
        }

        private static bool IsOnPremiseDeployment(string? deployment)
        {
            return deployment == TerminalIdentifiers.OnPremiseDeployment;
        }

        /// <summary>
        /// Creates a license for on-premise production deployment.
        /// </summary>
        /// <seealso cref="LicensingOptions.Deployment"/>
        private License OnPremiseDeploymentLicense()
        {
            return new License(
                terminalOptions.Licensing.LicensePlan,
                "on-premise-deployment",
                "on-premise-deployment",
                new LicenseClaims(),
                LicenseLimits.Create(terminalOptions.Licensing.LicensePlan),
                LicensePrice.Create(terminalOptions.Licensing.LicensePlan));
        }

        private readonly string checkLicUrl = "https://api.perpetualintelligence.com/public/checklicense";
        private readonly string fallbackCheckLicUrl = "https://piapim.azure-api.net/public/checklicense";
        private readonly IHttpClientFactory? httpClientFactory;
        private readonly ILicenseDebugger licenseDebugger;
        private readonly ILogger<LicenseExtractor> logger;
        private readonly TerminalOptions terminalOptions;
        private LicenseExtractorResult? licenseExtractorResult;
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    }
}
