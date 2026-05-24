//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Testing.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalLoggerExceptionHandlerTests
    {
        public TerminalLoggerExceptionHandlerTests()
        {
            _loggerFactory = new MockListLoggerFactory();
            _handler = new TerminalLoggerExceptionHandler(((ILoggerFactory)_loggerFactory).CreateLogger<TerminalLoggerExceptionHandler>());
        }

        [Fact]
        public async Task HandleExceptionAsync_TerminalException_NoRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(
                new TerminalException("err", "Something went wrong."));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=<null>");
            _loggerFactory.AllLogMessages[1].Should().Be("Something went wrong.");
        }

        [Fact]
        public async Task HandleExceptionAsync_TerminalException_WithRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(
                new TerminalException("err", "Something went wrong."),
                new CommandRequest("req-1", "run cmd"));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=req-1");
            _loggerFactory.AllLogMessages[1].Should().Be("Something went wrong.");
        }

        [Fact]
        public async Task HandleExceptionAsync_TerminalException_WithArgs_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(
                new TerminalException("err", "Value {0} is invalid for {1}.", "foo", "bar"));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=<null>");
            _loggerFactory.AllLogMessages[1].Should().Be("Value foo is invalid for bar.");
        }

        [Fact]
        public async Task HandleExceptionAsync_OperationCanceled_NoRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(new OperationCanceledException());

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=<null>");
            _loggerFactory.AllLogMessages[1].Should().Be("The request was canceled.");
        }

        [Fact]
        public async Task HandleExceptionAsync_OperationCanceled_WithRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(
                new OperationCanceledException(),
                new CommandRequest("req-2", "run cmd"));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=req-2");
            _loggerFactory.AllLogMessages[1].Should().Be("The request was canceled. request=req-2 command=run cmd");
        }

        [Fact]
        public async Task HandleExceptionAsync_GenericException_NoRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(new InvalidOperationException("Something bad."));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=<null>");
            _loggerFactory.AllLogMessages[1].Should().Be("The request failed.");
        }

        [Fact]
        public async Task HandleExceptionAsync_GenericException_WithRequest_WritesExactMessages()
        {
            var context = new TerminalExceptionHandlerContext(
                new InvalidOperationException("Something bad."),
                new CommandRequest("req-4", "run cmd"));

            await _handler.HandleExceptionAsync(context);

            _loggerFactory.AllLogMessages.Should().HaveCount(2);
            _loggerFactory.AllLogMessages[0].Should().Be("Handle exception. request=req-4");
            _loggerFactory.AllLogMessages[1].Should().Be("The request failed. request=req-4 command=run cmd info=Something bad.");
        }

        [Fact]
        public async Task HandleExceptionAsync_NullException_Throws()
        {
            Func<Task> act = () => _handler.HandleExceptionAsync(new TerminalExceptionHandlerContext(null!));
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HandleExceptionAsync_Always_ReturnsCompletedTask()
        {
            var context = new TerminalExceptionHandlerContext(new Exception("test"));

            Task result = _handler.HandleExceptionAsync(context);

            result.IsCompleted.Should().BeTrue();
            await result;
        }

        private readonly MockListLoggerFactory _loggerFactory;
        private readonly TerminalLoggerExceptionHandler _handler;
    }
}