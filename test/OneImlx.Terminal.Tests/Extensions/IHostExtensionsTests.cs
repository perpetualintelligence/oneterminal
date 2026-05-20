//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands.Routers.Mocks;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// <see cref="IHost"/> extension tests.
    /// </summary>
    public class IHostExtensionsTests : IAsyncLifetime
    {
        public ValueTask DisposeAsync()
        {
            host?.Dispose();

            return ValueTask.CompletedTask;
        }

        public ValueTask InitializeAsync()
        {
            // Use Mock<IHost> instead of building a real host
            mockTerminalRouter = new Mock<ITerminalRouter<TerminalRouterContext>>();
            licenseExtractor = new MockLicenseExtractorInner();
            logger = new LoggerFactory().CreateLogger<ITerminalRouter<TerminalRouterContext>>();

            // Mock IHostApplicationLifetime
            applicationLifetime = new Mock<IHostApplicationLifetime>();
            applicationLifetime.Setup(x => x.ApplicationStopping).Returns(applicationTokenSource.Token);

            // Setup service provider
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(ITerminalRouter<TerminalRouterContext>))).Returns(mockTerminalRouter.Object);
            serviceProvider.Setup(x => x.GetService(typeof(ILicenseExtractor))).Returns(licenseExtractor);
            serviceProvider.Setup(x => x.GetService(typeof(ILogger<ITerminalRouter<TerminalRouterContext>>))).Returns(logger);
            serviceProvider.Setup(x => x.GetService(typeof(IHostApplicationLifetime))).Returns(applicationLifetime.Object);

            // Setup mock host
            mockHost = new Mock<IHost>();
            mockHost.Setup(x => x.Services).Returns(serviceProvider.Object);

            host = mockHost.Object;

            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task Failed_License_Blocks_Blocking_Router()
        {
            licenseExtractor.TestLicense.SetFailed(new OneImlx.Shared.Infrastructure.Error("test_lic_error", "test_lic_error_desc"));

            TerminalRouterContext context = CreateContext();

            Func<Task> func = async () => await host.RunTerminalRouterBlockingAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);

            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("test_lic_error")
                .WithErrorDescription("test_lic_error_desc");
        }

        [Fact]
        public void Failed_License_Blocks_Background_Router()
        {
            licenseExtractor.TestLicense.SetFailed(new OneImlx.Shared.Infrastructure.Error("test_lic_error", "test_lic_error_desc"));

            TerminalRouterContext context = CreateContext();

            Action action = () => host.RunTerminalRouterBackground<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);

            action.Should().Throw<TerminalException>()
                .WithErrorCode("test_lic_error")
                .WithErrorDescription("test_lic_error_desc");
        }

        [Fact]
        public async Task RunTerminalRouterBackground_Runs_Blocking_Router()
        {
            licenseExtractor.NoLicense = false;
            licenseExtractor.GetCalled.Should().BeFalse();

            // Intentionally set the terminal cancellation token to custom so we can verify it gets reset to application stopping
            TerminalRouterContext context = CreateContext();
            CancellationTokenSource terminalCts = new();
            context.TerminalCancellationToken = terminalCts.Token;

            bool routerCompleted = false;
            mockTerminalRouter.Setup(x => x.RunAsync(It.IsAny<TerminalRouterContext>()))
                .Returns(async () =>
                {
                    // Cancel after 1000ms to simulate application stopping and allow the router to exit
                    applicationTokenSource.CancelAfter(1000);

                    // The RunTerminalRouterBlockingAsync sets the context.TerminalCancellationToken to the application token,
                    // so we wait until that token is cancelled
                    while (!context.TerminalCancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(100);
                    }

                    routerCompleted = true;
                });

            await host.RunTerminalRouterBlockingAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);

            mockTerminalRouter.Verify(x => x.RunAsync(context), Times.Once);
            context.TerminalCancellationToken.IsCancellationRequested.Should().BeTrue();
            context.TerminalCancellationToken.Should().Be(applicationTokenSource.Token);
            licenseExtractor.GetCalled.Should().BeTrue();
            routerCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task RunTerminalRouterBackground_Runs_Non_Blocking_Router()
        {
            licenseExtractor.NoLicense = false;
            licenseExtractor.GetCalled.Should().BeFalse();

            TerminalRouterContext context = CreateContext();
            CancellationTokenSource terminalCts = new();
            context.TerminalCancellationToken = terminalCts.Token;

            TaskCompletionSource<bool> routerStarted = new();
            TaskCompletionSource<bool> routerContinue = new();

            mockTerminalRouter.Setup(x => x.RunAsync(It.IsAny<TerminalRouterContext>()))
                .Returns(async () =>
                {
                    routerStarted.SetResult(true);

                    // Block router indefinitely until test releases it
                    await routerContinue.Task;
                });

            // Ensure router actually started
            host.RunTerminalRouterBackground<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);
            await routerStarted.Task;

            // Token should now reflect host application token state
            mockTerminalRouter.Verify(x => x.RunAsync(context), Times.Once);
            licenseExtractor.GetCalled.Should().BeTrue();
            context.TerminalCancellationToken.IsCancellationRequested.Should().BeFalse();

            // Method already returned while router still running = non-blocking
            routerContinue.SetResult(true);
        }

        [Fact]
        public async Task RunTerminalRouterBlockingAsync_Should_Throw_When_No_License()
        {
            licenseExtractor.NoLicense = true;

            TerminalRouterContext context = CreateContext();

            Func<Task> func = async () => await host.RunTerminalRouterBlockingAsync<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);

            await func.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_license")
                .WithErrorDescription("Failed to extract a valid license. Please configure the hosted service correctly.");
        }

        [Fact]
        public void RunTerminalRouterBackground_Should_Throw_When_No_License()
        {
            licenseExtractor.NoLicense = true;

            TerminalRouterContext context = CreateContext();

            Action action = () => host.RunTerminalRouterBackground<ITerminalRouter<TerminalRouterContext>, TerminalRouterContext>(context);

            action.Should().Throw<TerminalException>()
                .WithErrorCode("invalid_license")
                .WithErrorDescription("Failed to extract a valid license. Please configure the hosted service correctly.");
        }

        /// <summary>
        /// Creates a terminal router context.
        /// </summary>
        /// <returns>The terminal router context.</returns>
        private TerminalRouterContext CreateContext()
        {
            TerminalRouterContext context = new TerminalConsoleRouterContext(TerminalStartMode.Custom, null);

            CancellationTokenSource terminalCts = new();
            terminalCts.Cancel();

            context.TerminalCancellationToken = terminalCts.Token;

            return context;
        }

        private Mock<IHost> mockHost = null!;
        private Mock<IHostApplicationLifetime> applicationLifetime = null!;
        private readonly CancellationTokenSource applicationTokenSource = new();
        private IHost host = null!;
        private IHostBuilder hostBuilder = null!;
        private MockLicenseExtractorInner licenseExtractor = null!;
        private ILogger<ITerminalRouter<TerminalRouterContext>> logger = null!;
        private Mock<ITerminalRouter<TerminalRouterContext>> mockTerminalRouter = null!;
    }
}