//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Moq;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Shared;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalConsoleExceptionHandlerTests
    {
        public TerminalConsoleExceptionHandlerTests()
        {
            _mockConsole = new Mock<ITerminalConsole>();
            _handler = new TerminalConsoleExceptionHandler(_mockConsole.Object);
        }

        [Fact]
        public async Task HandleExceptionAsync_TerminalException_WithArgs_WritesErrorDescription()
        {
            var ex = new TerminalException(new Error("test_error", "Something went wrong with {0}", "details"));

            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "Something went wrong with {0}", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_TerminalException_WithNullArgs_WritesErrorDescription()
        {
            var ex = new TerminalException(new Error("test_error", "No args error"));

            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "No args error", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_OperationCanceledException_WithRequest_WritesRequestInfo()
        {
            var request = new CommandRequest("req1", "test command");

            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(new OperationCanceledException(), request));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "The terminal request was canceled. request={0} command={1}", "req1", "test command"), Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_OperationCanceledException_WithoutRequest_WritesGenericMessage()
        {
            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(new OperationCanceledException()));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "The terminal request was canceled."), Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_GenericException_WithRequest_WritesRequestInfo()
        {
            var request = new CommandRequest("req2", "run cmd");

            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(new InvalidOperationException("something failed"), request));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "The terminal request failed. request={0} command={1} info={2}", "req2", "run cmd", "something failed"), Times.Once);
        }

        [Fact]
        public async Task HandleExceptionAsync_GenericException_WithoutRequest_WritesGenericMessage()
        {
            await _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(new InvalidOperationException("generic failure")));

            _mockConsole.Verify(c => c.WriteLineColorAsync(ConsoleColor.Red, "The terminal request failed. info={0}", "generic failure"), Times.Once);
        }

        private readonly Mock<ITerminalConsole> _mockConsole;
        private readonly TerminalConsoleExceptionHandler _handler;
    }
}