/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Authentication.Duende
{
    /// <summary>
    /// The <c>OneImlx</c> terminal <c>Kiota</c> authentication and authorization provider for <c>Duende</c> IdentityServer.
    /// </summary>
    /// <remarks>
    /// Mirrors the MSAL silent/interactive pattern: attempts client credentials first, then falls back to the device
    /// code flow (RFC 8628) when user interaction is required. Pass a <c>device_code_callback</c> key in
    /// <c>additionalAuthenticationContext</c> with an <see cref="Action{T1,T2}"/> of (userCode, verificationUri) to
    /// display the login prompt to the terminal user. Pass a <c>refresh_token</c> key to use the refresh token flow.
    /// </remarks>
    public sealed class DuendeKiotaAuthProvider : IAuthenticationProvider, IAccessTokenProvider
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalOptions">The terminal options.</param>
        /// <param name="duendeTokenAcquisition">The Duende token acquisition.</param>
        /// <param name="logger">The logger.</param>
        public DuendeKiotaAuthProvider(
            TerminalOptions terminalOptions,
            IDuendeTokenAcquisition duendeTokenAcquisition,
            ILogger<DuendeKiotaAuthProvider> logger)
        {
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.duendeTokenAcquisition = duendeTokenAcquisition ?? throw new ArgumentNullException(nameof(duendeTokenAcquisition));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the validator that ensures the authorization request is for an allowed host.
        /// </summary>
        public AllowedHostsValidator AllowedHostsValidator
        {
            get
            {
                return new AllowedHostsValidator(terminalOptions.Authentication.ValidHosts);
            }
        }

        /// <summary>
        /// Authenticates a Kiota HTTP request by acquiring an access token and setting it in the Authorization header.
        /// </summary>
        /// <param name="request">The Kiota request to authenticate.</param>
        /// <param name="additionalAuthenticationContext">
        /// Optional. Supported keys:
        /// <list type="bullet">
        /// <item><c>scopes</c> — <see cref="IEnumerable{String}"/> or space-separated string of additional scopes.</item>
        /// <item><c>refresh_token</c> — string refresh token; uses the refresh token flow when present.</item>
        /// <item><c>device_code_callback</c> — <see cref="Action{T1,T2}"/> of (userCode, verificationUri) invoked
        /// during the device code flow to display the user prompt.</item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            string accessToken = await GetAuthorizationTokenAsync(request.URI, additionalAuthenticationContext, cancellationToken);
            request.Headers["Authorization"] = [$"Bearer {accessToken}"];
            logger.LogInformation("Bearer token set. request_uri={0}", request.URI);
        }

        /// <summary>
        /// Asynchronously gets an authorization token for the specified URI.
        /// </summary>
        /// <param name="uri">The URI for which the authorization token is required.</param>
        /// <param name="additionalAuthenticationContext">
        /// Optional. Supported keys:
        /// <list type="bullet">
        /// <item><c>scopes</c> — <see cref="IEnumerable{String}"/> or space-separated string of additional scopes.</item>
        /// <item><c>refresh_token</c> — string refresh token; uses the refresh token flow when present.</item>
        /// <item><c>device_code_callback</c> — <see cref="Action{T1,T2}"/> of (userCode, verificationUri) invoked
        /// during the device code flow to display the user prompt.</item>
        /// </list>
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The access token string.</returns>
        /// <exception cref="TerminalException">Thrown for invalid provider, unauthorized host, or token failure.</exception>
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure provider is configured for Duende
            if (!terminalOptions.Authentication.Provider.Equals("duende", StringComparison.OrdinalIgnoreCase))
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The terminal Duende authentication is not enabled.");
            }

            // Verify host
            logger.LogDebug("Acquire authorization token. host={0}", uri);
            if (!AllowedHostsValidator.IsUrlHostValid(uri))
            {
                throw new TerminalException(TerminalErrors.UnauthorizedAccess, "The host is not authorized. uri={0}", uri);
            }

            List<string> scopes = ExtractScopesFromContext(additionalAuthenticationContext);

            // Refresh token flow — explicit, highest priority when caller holds a refresh token
            if (additionalAuthenticationContext != null
                && additionalAuthenticationContext.TryGetValue("refresh_token", out var refreshTokenObj)
                && refreshTokenObj is string refreshToken
                && !string.IsNullOrWhiteSpace(refreshToken))
            {
                logger.LogDebug("Acquiring Duende token via refresh_token.");
                var token = await duendeTokenAcquisition.AcquireTokenRefreshAsync(scopes, refreshToken, cancellationToken);
                logger.LogInformation("Acquired Duende token via refresh_token. scopes={0}", scopes.JoinBySpace());
                return token;
            }

            // Client credentials — silent path (machine-to-machine), mirrors MSAL AcquireTokenSilentAsync
            try
            {
                var token = await duendeTokenAcquisition.AcquireTokenClientCredentialsAsync(scopes, cancellationToken);
                logger.LogInformation("Acquired Duende token via client_credentials. scopes={0}", scopes.JoinBySpace());
                return token;
            }
            catch (TerminalException ex) when (ex.Error.ErrorCode == TerminalErrors.UnauthorizedAccess)
            {
                // Client credentials failed — fall through to interactive device code flow,
                // mirrors MSAL catch(MsalUiRequiredException) → AcquireTokenInteractiveAsync
                logger.LogDebug("Client credentials failed, falling back to device_code flow. info={0}", ex.Message);
            }

            // Device code flow — interactive path (RFC 8628), mirrors MSAL AcquireTokenInteractiveAsync
            // Caller supplies a callback via context key "device_code_callback" to display user_code + uri
            Action<string, string> deviceCodeCallback = DefaultDeviceCodeCallback;
            if (additionalAuthenticationContext != null
                && additionalAuthenticationContext.TryGetValue("device_code_callback", out var callbackObj)
                && callbackObj is Action<string, string> customCallback)
            {
                deviceCodeCallback = customCallback;
            }

            var interactiveToken = await duendeTokenAcquisition.AcquireTokenDeviceCodeAsync(scopes, deviceCodeCallback, cancellationToken);
            logger.LogInformation("Acquired Duende token via device_code. scopes={0}", scopes.JoinBySpace());
            return interactiveToken;
        }

        private static void DefaultDeviceCodeCallback(string userCode, string verificationUri)
        {
            // Default: write device code prompt to console so terminal user sees it
            Console.WriteLine($"Open {verificationUri} and enter code: {userCode}");
        }

        private List<string> ExtractScopesFromContext(Dictionary<string, object>? additionalAuthenticationContext)
        {
            List<string> scopes = [.. terminalOptions.Authentication.DefaultScopes ?? Enumerable.Empty<string>()];
            if (additionalAuthenticationContext != null && additionalAuthenticationContext.TryGetValue("scopes", out var additionalScopesObj))
            {
                if (additionalScopesObj is IEnumerable<string> additionalScopesEnumerable)
                {
                    scopes.AddRange(additionalScopesEnumerable);
                }
                else if (additionalScopesObj is string additionalScopesString)
                {
                    scopes.AddRange(additionalScopesString.SplitBySpace());
                }
                else
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "Additional scopes are not in an expected format. They should be either `IEnumerable<string>` or a space-separated string.");
                }
            }

            logger.LogDebug("Acquired authentication scopes. scopes={0}", scopes.JoinBySpace());
            return scopes;
        }

        private readonly IDuendeTokenAcquisition duendeTokenAcquisition;
        private readonly ILogger<DuendeKiotaAuthProvider> logger;
        private readonly TerminalOptions terminalOptions;
    }
}
