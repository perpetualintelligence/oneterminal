/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Authentication.Duende
{
    /// <summary>
    /// An abstraction to acquire tokens using Duende IdentityServer.
    /// </summary>
    public interface IDuendeTokenAcquisition
    {
        /// <summary>
        /// Acquires a token using the client credentials flow (machine-to-machine).
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the access token string.</returns>
        Task<string> AcquireTokenClientCredentialsAsync(IEnumerable<string> scopes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a token using the device code flow (RFC 8628) — the interactive flow for terminal applications.
        /// Displays the user code and verification URI via <paramref name="deviceCodeCallback"/>, then polls until the
        /// user completes authentication in a browser.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="deviceCodeCallback">
        /// A callback invoked with the user code and verification URI so the terminal can display them to the user.
        /// Receives: (userCode, verificationUri).
        /// </param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the access token string.</returns>
        Task<string> AcquireTokenDeviceCodeAsync(IEnumerable<string> scopes, Action<string, string> deviceCodeCallback, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a token using the resource owner password credentials flow.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="username">The resource owner username.</param>
        /// <param name="password">The resource owner password.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the access token string.</returns>
        Task<string> AcquireTokenPasswordAsync(IEnumerable<string> scopes, string username, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a token using a refresh token.
        /// </summary>
        /// <param name="scopes">The scopes for which the token is requested.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the access token string.</returns>
        Task<string> AcquireTokenRefreshAsync(IEnumerable<string> scopes, string refreshToken, CancellationToken cancellationToken = default);
    }
}
