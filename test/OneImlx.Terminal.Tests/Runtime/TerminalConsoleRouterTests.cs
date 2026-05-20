//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalConsoleRouterTests
    {
        public TerminalConsoleRouterTests()
        {
            tcs = new CancellationTokenSource();
            terminalConsoleMock = new Mock<ITerminalConsole>();
            terminalProcessorMock = new Mock<ITerminalProcessor>();
            exceptionHandlerMock = new Mock<ITerminalExceptionHandler>();
            loggerMock = new Mock<ILogger<TerminalConsoleRouter>>();
            options = new TerminalOptions
            {
                Router = new RouterOptions
                {
                    Caret = ">",
                    Timeout = 25000
                }
            };
            router = new TerminalConsoleRouter(
                terminalConsoleMock.Object,
                terminalProcessorMock.Object,
                exceptionHandlerMock.Object,
                options,
                loggerMock.Object);
        }

        [Fact]
        public void IsRunning_ShouldBeFalseInitially()
        {
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Does_Not_Runs_Indefinitely_If_Route_Once()
        {
            tcs.CancelAfter(2000);

            options.Driver.Enabled = true;
            options.Driver.RootId = "test_root";
            TerminalConsoleRouterContext context = new(TerminalStartMode.Console, routeOnce: true)
            {
                TerminalCancellationToken = tcs.Token
            };
            await router.RunAsync(context);

            // Verify command is routed. Normally in 2 secs the RouteCommandAsync will be invoked multiple times due but
            // here it will call once.
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.Is<TerminalInputOutput>(ctx => ctx.Requests[0].Raw == "test_root")), Times.Once);

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Handles_Exception()
        {
            tcs.CancelAfter(300);

            terminalConsoleMock.Setup(t => t.ReadLineAsync()).ThrowsAsync(new NotSupportedException("Test exception"));

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };
            await router.RunAsync(context);

            // Verify NotSupportedException is handled. This may be invoked multiple times due to the cancellation token.
            exceptionHandlerMock.Verify(e => e.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(context =>
                context.Exception is NotSupportedException && context.Exception.Message == "Test exception")), Times.AtLeastOnce());

            // Verify Canceled exception is handled
            exceptionHandlerMock.Verify(e => e.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(context =>
                context.Exception is OperationCanceledException)), Times.Once);

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Ignore_Commands_Does_Not_Route()
        {
            terminalConsoleMock.Setup(t => t.Ignore(It.Is<string>(s => s == "xyz"))).Returns(true);
            terminalConsoleMock.Setup(t => t.ReadLineAsync()).ReturnsAsync("xyz");

            tcs.CancelAfter(200);
            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };
            await router.RunAsync(context);
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.IsAny<TerminalInputOutput>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_Routes_Request()
        {
            tcs.CancelAfter(300);

            terminalConsoleMock.Setup(t => t.ReadLineAsync()).ReturnsAsync("test_command");

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };
            await router.RunAsync(context);

            // Verify command is routed. This may be invoked multiple times due to the cancellation token.
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.Is<TerminalInputOutput>(ctx => ctx.Requests[0].Raw == "test_command")), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RunAsync_Runs_And_Stops()
        {
            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };

            var runTask = router.RunAsync(context);
            await Task.Delay(100, TestContext.Current.CancellationToken);
            router.IsRunning.Should().BeTrue();

            tcs.CancelAfter(100);
            await Task.Delay(300, TestContext.Current.CancellationToken);
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Runs_First_Arg_As_Command_When_Args_Are_Passed()
        {
            tcs.CancelAfter(2000);

            options.Driver.Enabled = true;
            options.Driver.RootId = "test_root";

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console, routeOnce: true, customProperties: null, arguments: ["test_arg1 blah", "test_arg2", "test_arg3"])
            {
                TerminalCancellationToken = tcs.Token
            };
            options.Parser.Separator = '+';
            await router.RunAsync(context);

            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.Is<TerminalInputOutput>(ctx => ctx.Requests[0].Raw == "test_root+test_arg1 blah")), Times.Once);

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Runs_Indefinitely_Until_Canceled()
        {
            tcs.CancelAfter(2000);

            terminalConsoleMock.Setup(t => t.ReadLineAsync()).ReturnsAsync("test_command");

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };
            await router.RunAsync(context);

            // Verify command is routed. This may be invoked multiple times due to the cancellation token. We are
            // verifying at least 5 times to ensure the router is running for a while.
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.Is<TerminalInputOutput>(ctx => ctx.Requests[0].Raw == "test_command")), Times.AtLeast(5));

            // Verify Canceled exception is handled
            exceptionHandlerMock.Verify(e => e.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(context =>
                context.Exception is OperationCanceledException)), Times.Once);

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Runs_Root_When_No_Args_Are_Passed()
        {
            tcs.CancelAfter(2000);

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console, routeOnce: true)
            {
                TerminalCancellationToken = tcs.Token
            };
            options.Driver.Enabled = true;
            options.Driver.RootId = "test_root";
            await router.RunAsync(context);

            // Verify command is routed. Normally in 2 secs the RouteCommandAsync will be invoked multiple times due but
            // here it will call once.
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.Is<TerminalInputOutput>(ctx => ctx.Requests[0].Raw == "test_root")), Times.Once);

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Throws_If_StartMode_IsNot_Console()
        {
            var context = new TerminalConsoleRouterContext(TerminalStartMode.Grpc);
            Func<Task> act = async () => await router.RunAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The requested start mode is not valid for console routing. start_mode=Grpc");
        }

        [Fact]
        public async Task RunAsync_Throws_When_Route_Once_But_Driver_Disabled()
        {
            tcs.CancelAfter(2000);

            TerminalConsoleRouterContext context = new(TerminalStartMode.Console, routeOnce: true)
            {
                TerminalCancellationToken = tcs.Token
            };
            options.Driver.Enabled = false;
            options.Driver.RootId = "test_root";
            await router.RunAsync(context);

            // Driver disabled for never called
            terminalProcessorMock.Verify(c => c.ExecuteAsync(It.IsAny<TerminalInputOutput>()), Times.Never);

            exceptionHandlerMock.Verify(e => e.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(context =>
                context.Exception is TerminalException && context.Exception.Message == "The route once is only valid for driver programs.")), Times.AtLeastOnce());

            // Verify IsRunning is set to false
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Writes_Caret_To_Console()
        {
            tcs.CancelAfter(200);
            TerminalConsoleRouterContext context = new(TerminalStartMode.Console)
            {
                TerminalCancellationToken = tcs.Token
            };

            terminalConsoleMock.Setup(t => t.ReadLineAsync()).ReturnsAsync("test_command");
            await router.RunAsync(context);
            terminalConsoleMock.Verify(t => t.WriteAsync(It.Is<string>(s => s == options.Router.Caret)), Times.AtLeastOnce);
        }

        private readonly Mock<ITerminalExceptionHandler> exceptionHandlerMock;
        private readonly Mock<ILogger<TerminalConsoleRouter>> loggerMock;
        private readonly TerminalOptions options;
        private readonly TerminalConsoleRouter router;
        private readonly CancellationTokenSource tcs;
        private readonly Mock<ITerminalConsole> terminalConsoleMock;
        private readonly Mock<ITerminalProcessor> terminalProcessorMock;
    }
}