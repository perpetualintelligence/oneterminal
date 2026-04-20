//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class IHostExtensionsTests
    {
        [Fact]
        public async Task RunTerminalRouterBlockingAsync_Should_Run_TerminalRouter()
        {
            var mockHost = new Mock<IHost>();
            var mockLogger = new Mock<ILogger<ITerminalRouter<TerminalRouterContext>>>();
            var mockTerminalRouter = new Mock<ITerminalRouter<TerminalRouterContext>>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var terminalRouterContext = new TerminalConsoleRouterContext(TerminalStartMode.Custom, null);

            var appStoppingCts = new CancellationTokenSource();
            var mockApplicationLifetime = new Mock<IHostApplicationLifetime>();
            mockApplicationLifetime.Setup(x => x.ApplicationStopping).Returns(appStoppingCts.Token);

            // Intentionally set the terminal cancellation token to custom so we can verify it gets reset to application stopping
            CancellationTokenSource terminalCts = new();
            terminalCts.Cancel();
            terminalRouterContext.TerminalCancellationToken = terminalCts.Token;

            var routerCompleted = false;
            mockTerminalRouter.Setup(x => x.RunAsync(It.IsAny<TerminalRouterContext>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    routerCompleted = true;
                });

            // Set up the service provider to return the logger and terminal router when requested
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<ITerminalRouter<TerminalRouterContext>>)))
                .Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ITerminalRouter<TerminalRouterContext>)))
                .Returns(mockTerminalRouter.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IHostApplicationLifetime)))
                .Returns(mockApplicationLifetime.Object);

            // Set up the host to return the mock service provider
            mockHost.Setup(x => x.Services).Returns(mockServiceProvider.Object);
            await mockHost.Object.RunTerminalRouterBlockingAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(terminalRouterContext);

            // Validates blocking: router completed before method returned
            mockTerminalRouter.Verify(x => x.RunAsync(terminalRouterContext), Times.Once);
            terminalRouterContext.TerminalCancellationToken.Should().Be(mockApplicationLifetime.Object.ApplicationStopping);
            terminalCts.Token.Should().NotBe(appStoppingCts.Token);
            routerCompleted.Should().BeTrue(); 
        }

        [Fact]
        public async Task RunTerminalRouterBackgroundAsync_Should_Start_Router_Without_Blocking()
        {
            var mockHost = new Mock<IHost>();
            var mockLogger = new Mock<ILogger<ITerminalRouter<TerminalRouterContext>>>();
            var mockTerminalRouter = new Mock<ITerminalRouter<TerminalRouterContext>>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var terminalRouterContext = new TerminalConsoleRouterContext(TerminalStartMode.Custom, null);

            var appStoppingCts = new CancellationTokenSource();
            var mockApplicationLifetime = new Mock<IHostApplicationLifetime>();
            mockApplicationLifetime.Setup(x => x.ApplicationStopping).Returns(appStoppingCts.Token);

            // Intentionally set the terminal cancellation token to custom so we can verify it gets reset to application stopping
            CancellationTokenSource terminalCts = new();
            terminalCts.Cancel();
            terminalRouterContext.TerminalCancellationToken = terminalCts.Token;

            var routerCompleted = false;
            mockTerminalRouter.Setup(x => x.RunAsync(It.IsAny<TerminalRouterContext>()))
                .Returns(async () =>
                {
                    await Task.Delay(1000);
                    routerCompleted = true;
                });

            // Set up the service provider to return the logger and terminal router when requested
            mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<ITerminalRouter<TerminalRouterContext>>)))
                .Returns(mockLogger.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ITerminalRouter<TerminalRouterContext>)))
                .Returns(mockTerminalRouter.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IHostApplicationLifetime)))
                .Returns(mockApplicationLifetime.Object);

            // Set up the host to return the mock service provider
            mockHost.Setup(x => x.Services).Returns(mockServiceProvider.Object);
            Task backgroundTask = mockHost.Object.RunTerminalRouterBackgroundAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(terminalRouterContext);

            // Validates non-blocking: method returned before router completed
            await Task.Delay(100, TestContext.Current.CancellationToken);
            mockTerminalRouter.Verify(x => x.RunAsync(terminalRouterContext), Times.Once);
            terminalRouterContext.TerminalCancellationToken.Should().Be(mockApplicationLifetime.Object.ApplicationStopping);
            terminalCts.Token.Should().NotBe(appStoppingCts.Token);
            routerCompleted.Should().BeFalse(); 
        }
    }
}