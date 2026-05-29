//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IdentityModel.Client;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Authentication.Duende
{
    /// <summary>
    /// The default <see cref="IDuendeTokenAcquisition"/> implementation that communicates with a Duende IdentityServer
    /// token endpoint using the <c>IdentityModel</c> client library.
    /// </summary>
    /// <remarks>
    /// Supports client credentials (machine-to-machine), device code (interactive terminal), resource owner password,
    /// and refresh token flows — mirroring the MSAL silent/interactive pattern.
    /// </remarks>
    public class DuendeTokenAcquisition : IDuendeTokenAcquisition
    {
        private readonly HttpClient httpClient;
        private readonly string authority;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly ILogger<DuendeTokenAcquisition> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuendeTokenAcquisition"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> used to contact the identity server.</param>
        /// <param name="authority">
        /// The Duende IdentityServer authority URL (e.g. <c>https://demo.duendesoftware.com</c>).
        /// Discovery is performed automatically to resolve the token and device authorization endpoints.
        /// </param>
        /// <param name="clientId">The client identifier registered with the identity server.</param>
        /// <param name="clientSecret">The client secret registered with the identity server.</param>
        /// <param name="logger">The logger.</param>
        public DuendeTokenAcquisition(
            HttpClient httpClient,
            string authority,
            string clientId,
            string clientSecret,
            ILogger<DuendeTokenAcquisition> logger)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
            this.clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            this.clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> AcquireTokenClientCredentialsAsync(IEnumerable<string> scopes, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Acquiring Duende token via client_credentials. authority={0}", authority);

            var disco = await GetDiscoveryDocumentAsync(cancellationToken);

            var response = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = string.Join(" ", scopes)
            }, cancellationToken);

            return ExtractAccessToken(response);
        }

        /// <inheritdoc />
        public async Task<string> AcquireTokenDeviceCodeAsync(IEnumerable<string> scopes, Action<string, string> deviceCodeCallback, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Acquiring Duende token via device_code flow. authority={0}", authority);

            var disco = await GetDiscoveryDocumentAsync(cancellationToken);

            // Request device authorization — server returns user_code + verification_uri
            var deviceAuthResponse = await httpClient.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
            {
                Address = disco.DeviceAuthorizationEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = string.Join(" ", scopes)
            }, cancellationToken);

            if (deviceAuthResponse.IsError)
            {
                logger.LogError("Device authorization error. error={0} description={1}", deviceAuthResponse.Error, deviceAuthResponse.ErrorDescription);
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The Duende device authorization returned an error. error={0}", deviceAuthResponse.Error);
            }

            // Invoke callback so terminal can display user_code + verification_uri to the user
            logger.LogInformation("Device code acquired. user_code={0} verification_uri={1}", deviceAuthResponse.UserCode, deviceAuthResponse.VerificationUri);
            deviceCodeCallback(deviceAuthResponse.UserCode!, deviceAuthResponse.VerificationUri!);

            // Poll token endpoint until user completes authentication or timeout
            int interval = deviceAuthResponse.Interval > 0 ? deviceAuthResponse.Interval : 5;
            var expiry = DateTimeOffset.UtcNow.AddSeconds(deviceAuthResponse.ExpiresIn > 0 ? (double)deviceAuthResponse.ExpiresIn : 300.0);

            while (DateTimeOffset.UtcNow < expiry)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);

                var tokenResponse = await httpClient.RequestDeviceTokenAsync(new DeviceTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    DeviceCode = deviceAuthResponse.DeviceCode
                }, cancellationToken);

                if (tokenResponse.IsError)
                {
                    if (tokenResponse.Error == "authorization_pending" || tokenResponse.Error == "slow_down")
                    {
                        if (tokenResponse.Error == "slow_down")
                        {
                            interval += 5;
                        }
                        logger.LogDebug("Device code authorization pending. error={0}", tokenResponse.Error);
                        continue;
                    }

                    logger.LogError("Device code token error. error={0} description={1}", tokenResponse.Error, tokenResponse.ErrorDescription);
                    throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The Duende device code token request failed. error={0}", tokenResponse.Error);
                }

                logger.LogInformation("Acquired Duende token via device_code. authority={0}", authority);
                return ExtractAccessToken(tokenResponse);
            }

            throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The Duende device code flow timed out. The user did not complete authentication in time.");
        }

        /// <inheritdoc />
        public async Task<string> AcquireTokenPasswordAsync(IEnumerable<string> scopes, string username, string password, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Acquiring Duende token via password grant. authority={0}", authority);

            var disco = await GetDiscoveryDocumentAsync(cancellationToken);

            var response = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = string.Join(" ", scopes),
                UserName = username,
                Password = password
            }, cancellationToken);

            return ExtractAccessToken(response);
        }

        /// <inheritdoc />
        public async Task<string> AcquireTokenRefreshAsync(IEnumerable<string> scopes, string refreshToken, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Acquiring Duende token via refresh_token. authority={0}", authority);

            var disco = await GetDiscoveryDocumentAsync(cancellationToken);

            var response = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = string.Join(" ", scopes),
                RefreshToken = refreshToken
            }, cancellationToken);

            return ExtractAccessToken(response);
        }

        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync(CancellationToken cancellationToken)
        {
            var disco = await httpClient.GetDiscoveryDocumentAsync(authority, cancellationToken);
            if (disco.IsError)
            {
                logger.LogError("Duende discovery error. error={0}", disco.Error);
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Failed to retrieve Duende IdentityServer discovery document. error={0}", disco.Error);
            }

            return disco;
        }

        private string ExtractAccessToken(TokenResponse response)
        {
            if (response.IsError)
            {
                logger.LogError("Duende token endpoint error. error={0} description={1}", response.Error, response.ErrorDescription);
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The Duende token endpoint returned an error. error={0}", response.Error);
            }

            if (string.IsNullOrWhiteSpace(response.AccessToken))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The Duende access token is null or empty.");
            }

            logger.LogInformation("Acquired Duende access token. authority={0}", authority);
            return response.AccessToken!;
        }
    }
}