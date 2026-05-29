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
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Authentication.Duende
{
    public class DuendeKiotaAccessTokenProviderTests
    {
        public DuendeKiotaAccessTokenProviderTests()
        {
            _duendeTokenAcquisitionMock = new Mock<IDuendeTokenAcquisition>();
            _loggerMock = new Mock<ILogger<DuendeKiotaAuthProvider>>();
            _terminalOptions = new TerminalOptions
            {
                Authentication = new AuthenticationOptions
                {
                    Provider = "duende",
                    DefaultScopes = ["api1"],
                    ValidHosts = ["api.example.com"]
                }
            };
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_ReturnsToken_ViaClientCredentials()
        {
            string expectedToken = "cc_access_token";
            SetupMockTokenAcquisition(expectedToken);
            var provider = CreateProvider();

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"));

            token.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_ReturnsToken_ViaRefreshToken()
        {
            string expectedToken = "refreshed_access_token";
            _duendeTokenAcquisitionMock
                .Setup(x => x.AcquireTokenRefreshAsync(It.IsAny<IEnumerable<string>>(), "rt_value", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var provider = CreateProvider();
            var context = new Dictionary<string, object> { { "refresh_token", "rt_value" } };

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"), context);

            token.Should().Be(expectedToken);
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_Throws_ForInvalidHost()
        {
            var provider = CreateProvider();

            Func<Task> act = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://untrusted.com/resource"));

            await act.Should().ThrowAsync<TerminalException>().WithMessage("The host is not authorized. uri=https://untrusted.com/resource");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_Throws_If_Authentication_Is_Not_Enabled()
        {
            _terminalOptions.Authentication.Provider = "none";
            var provider = CreateProvider();

            Func<Task> func = async () => await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"));

            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidConfiguration)
                .WithErrorDescription("The terminal Duende authentication is not enabled.");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_ReturnsToken_ViaDeviceCode_WhenClientCredentialsFail()
        {
            string expectedToken = "device_access_token";
            string capturedUserCode = string.Empty;
            string capturedVerificationUri = string.Empty;

            // Client credentials throws UnauthorizedAccess — triggers interactive device-code fallback
            _duendeTokenAcquisitionMock
                .Setup(x => x.AcquireTokenClientCredentialsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TerminalException(TerminalErrors.UnauthorizedAccess, "Client credentials failed."));

            _duendeTokenAcquisitionMock
                .Setup(x => x.AcquireTokenDeviceCodeAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Action<string, string>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<string>, Action<string, string>, CancellationToken>((_, cb, __) => cb("ABCDEF", "https://login.example.com/device"))
                .ReturnsAsync(expectedToken);

            var provider = CreateProvider();
            var context = new Dictionary<string, object>
            {
                { "device_code_callback", (Action<string, string>)((code, uri) => { capturedUserCode = code; capturedVerificationUri = uri; }) }
            };

            string token = await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"), context);

            token.Should().Be(expectedToken);
            capturedUserCode.Should().Be("ABCDEF");
            capturedVerificationUri.Should().Be("https://login.example.com/device");
        }

        [Fact]
        public async Task GetAuthorizationTokenAsync_UsesCorrectScopes_ForTokenAcquisition()
        {
            IEnumerable<string>? usedScopes = null;
            _duendeTokenAcquisitionMock
                .Setup(x => x.AcquireTokenClientCredentialsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<string>, CancellationToken>((s, _) => usedScopes = s)
                .ReturnsAsync("token");

            var provider = CreateProvider();
            var additionalScopes = new[] { "api2", "api3" };

            await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"), new Dictionary<string, object> { { "scopes", additionalScopes } });
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes!.Concat(additionalScopes));

            await provider.GetAuthorizationTokenAsync(new Uri("https://api.example.com/resource"));
            usedScopes.Should().BeEquivalentTo(_terminalOptions.Authentication.DefaultScopes);
        }

        private DuendeKiotaAuthProvider CreateProvider()
        {
            return new DuendeKiotaAuthProvider(_terminalOptions, _duendeTokenAcquisitionMock.Object, _loggerMock.Object);
        }

        private void SetupMockTokenAcquisition(string token)
        {
            _duendeTokenAcquisitionMock
                .Setup(x => x.AcquireTokenClientCredentialsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
        }

        private readonly Mock<IDuendeTokenAcquisition> _duendeTokenAcquisitionMock;
        private readonly Mock<ILogger<DuendeKiotaAuthProvider>> _loggerMock;
        private TerminalOptions _terminalOptions;
    }
}
