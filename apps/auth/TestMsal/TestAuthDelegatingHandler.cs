using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using OneImlx.Terminal.Authentication.Msal;

namespace OneImlx.Terminal.Apps.TestAuth
{
    public class TestAuthDelegatingHandler : MsalAccessTokenProviderDelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="accessTokenProvider">The access token provider.</param>
        /// <param name="logger">The logger.</param>
        public TestAuthDelegatingHandler(IAccessTokenProvider accessTokenProvider, ILogger<TestAuthDelegatingHandler> logger)
        : base(accessTokenProvider, logger)
        {
        }

        protected override Task PreflightAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.PreflightAsync(request, cancellationToken);

            // Add custom logic here, e.g.: adding preflight header
        }
    }
}
