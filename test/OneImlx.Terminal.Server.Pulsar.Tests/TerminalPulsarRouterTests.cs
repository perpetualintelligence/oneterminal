//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar;
using DotPulsar.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Server.Pulsar
{
    public partial class TerminalPulsarRouterTests
    {
        public TerminalPulsarRouterTests()
        {
            processorMock = new Mock<ITerminalProcessor>();
            exceptionHandlerMock = new Mock<ITerminalExceptionHandler>();
            accessorMock = new Mock<ITerminalPulsarAccessor>();
            producerMock = new Mock<IProducer<byte[]>>();
            logger = new Mock<ILogger<TerminalPulsarRouter>>().Object;

            accessorMock.Setup(x => x.GetProducer()).Returns(producerMock.Object);
            processorMock.Setup(x => x.StopProcessingAsync(It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public void IsRunning_Initially_False()
        {
            var router = CreateRouter();

            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void Name_Returns_Pulsar()
        {
            var router = CreateRouter();

            router.Name.Should().Be("pulsar");
        }

        [Fact]
        public async Task RunAsync_Invalid_StartMode_Throws()
        {
            var context = new TerminalPulsarRouterContext(TerminalStartMode.Grpc);
            var router = CreateRouter();

            await FluentActions.Awaiting(() => router.RunAsync(context))
                .Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("Invalid start mode for Pulsar.");
        }

        [Fact]
        public async Task RunAsync_Starts_And_Stops_Processor()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Single("id1", "test");

            SetupConsumerWithMessage(input);
            SetupProducer(cts);

            var router = CreateRouter();
            await router.RunAsync(context);

            processorMock.Verify(x => x.StartProcessing(context, false), Times.Once);
            processorMock.Verify(x => x.StopProcessingAsync(5000), Times.Once);
            router.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task RunAsync_Calls_ExecuteAsync_With_Input()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Single("id1", "test command");

            SetupConsumerWithMessage(input);
            SetupProducer(cts);

            var router = CreateRouter();
            await router.RunAsync(context);

            processorMock.Verify(x => x.ExecuteAsync(It.Is<TerminalInputOutput>(
                i => i.Count == 1 && i.Requests[0].Id == "id1")), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Sends_Response_Via_Producer()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Single("id1", "test");

            SetupConsumerWithMessage(input);
            SetupProducer(cts);

            var router = CreateRouter();
            await router.RunAsync(context);

            producerMock.Verify(x => x.Send(It.IsAny<MessageMetadata>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Acknowledges_Message()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(300);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Single("id1", "test");

            var consumer = SetupConsumerWithMessage(input);
            SetupProducer(cts);

            var router = CreateRouter();
            await router.RunAsync(context);

            consumer.IsAcknowledged.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Processes_Batch_Input()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Batch("batch1", ["id1", "id2"], ["cmd1", "cmd2"]);

            SetupConsumerWithMessage(input);
            SetupProducer(cts);

            var router = CreateRouter();
            await router.RunAsync(context);

            processorMock.Verify(x => x.ExecuteAsync(It.Is<TerminalInputOutput>(
                i => i.Count == 2 && i.BatchId == "batch1")), Times.Once);
        }

        [Fact]
        public async Task RunAsync_Null_Input_Calls_ExceptionHandler()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);

            SetupConsumerWithNullMessage();
            exceptionHandlerMock.Setup(x => x.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()))
                .Callback(() => cts.Cancel())
                .Returns(Task.CompletedTask);

            var router = CreateRouter();
            await router.RunAsync(context);

            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(
                It.Is<TerminalExceptionHandlerContext>(ctx => ctx.Exception is TerminalException)), Times.Once);
            processorMock.Verify(x => x.ExecuteAsync(It.IsAny<TerminalInputOutput>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_Empty_Requests_Calls_ExceptionHandler()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            var context = CreateContext(cts);
            var emptyInput = TerminalInputOutput.Batch("batch1", [], []);

            SetupConsumerWithMessage(emptyInput);
            exceptionHandlerMock.Setup(x => x.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()))
                .Callback(() => cts.Cancel())
                .Returns(Task.CompletedTask);

            var router = CreateRouter();
            await router.RunAsync(context);

            exceptionHandlerMock.Verify(x => x.HandleExceptionAsync(It.Is<TerminalExceptionHandlerContext>(ctx =>
                ctx.Exception is TerminalException)), Times.Once);
        }

        private TerminalPulsarRouterContext CreateContext(CancellationTokenSource cts)
        {
            var context = new TerminalPulsarRouterContext(TerminalStartMode.Pulsar);
            typeof(TerminalRouterContext).GetProperty("TerminalCancellationToken")!
                .SetValue(context, cts.Token);
            return context;
        }

        private MockConsumer SetupConsumerWithMessage(TerminalInputOutput input)
        {
            var data = JsonSerializer.SerializeToUtf8Bytes(input);
            var msgMock = new Mock<IMessage<byte[]>>();
            msgMock.Setup(m => m.Data).Returns(new ReadOnlySequence<byte>(data));
            msgMock.Setup(m => m.MessageId).Returns(new MessageId(1, 1, 0, 0));

            var consumer = new MockConsumer(msgMock.Object);
            accessorMock.Setup(x => x.GetConsumer()).Returns(consumer);
            return consumer;
        }

        private MockConsumer SetupConsumerWithNullMessage()
        {
            var data = Encoding.UTF8.GetBytes("null");
            var msgMock = new Mock<IMessage<byte[]>>();
            msgMock.Setup(m => m.Data).Returns(new ReadOnlySequence<byte>(data));
            msgMock.Setup(m => m.MessageId).Returns(new MessageId(1, 1, 0, 0));

            var consumer = new MockConsumer(msgMock.Object);
            accessorMock.Setup(x => x.GetConsumer()).Returns(consumer);
            return consumer;
        }

        [Fact]
        public async Task RunAsync_Processes_Until_Cancel()
        {
            // Router runs indefinitely until cancellation, so we cancel after a delay to ensure it processes messages until cancellation is requested.
            var cts = new CancellationTokenSource();
            cts.CancelAfter(500);

            var context = CreateContext(cts);
            var input = TerminalInputOutput.Single("id1", "test");
            var data = JsonSerializer.SerializeToUtf8Bytes(input);

            var msg = new Mock<IMessage<byte[]>>();
            msg.Setup(x => x.Data).Returns(new ReadOnlySequence<byte>(data));
            msg.Setup(x => x.MessageId).Returns(new MessageId(1, 1, 0, 0));

            // Return 100 messages using MockConsumer
            var consumer = new MockConsumer(msg.Object, messageCount: 100);
            accessorMock.Setup(x => x.GetConsumer()).Returns(consumer);

            // no cancel from producer
            producerMock.Setup(x => x.Send(It.IsAny<MessageMetadata>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns(() => ValueTask.FromResult(new MessageId(1, 1, 0, 0)));

            // Now the router will run indefinitely until canceled after 2000ms
            // During this time we expect the router has executed 100 messages.
            var router = CreateRouter();
            await router.RunAsync(context);

            // Verify we have processed all 100
            processorMock.Verify(x => x.ExecuteAsync(It.IsAny<TerminalInputOutput>()), Times.AtLeast(100));

            // Ensure loop re-entered (THIS proves while loop)
            // DotPulsar's Messages() extension calls Receive() repeatedly, so we check that
            consumer.ReceiveCallCount.Should().BeGreaterThan(1);

            // Ensure we are not running
            router.IsRunning.Should().BeFalse();
        }

        private void SetupProducer(CancellationTokenSource cts)
        {
            producerMock.Setup(x => x.Send(It.IsAny<MessageMetadata>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Callback(() => cts.Cancel())
                .Returns(ValueTask.FromResult(new MessageId(1, 1, 0, 0)));
        }

        private TerminalPulsarRouter CreateRouter()
        {
            var options = new TerminalOptions
            {
                Router = new RouterOptions
                {
                    Timeout = 5000
                }
            };

            return new TerminalPulsarRouter(
                processorMock.Object,
                Options.Create(options),
                logger,
                accessorMock.Object,
                exceptionHandlerMock.Object);
        }

        private readonly Mock<ITerminalPulsarAccessor> accessorMock;
        private readonly Mock<ITerminalExceptionHandler> exceptionHandlerMock;
        private readonly ILogger<TerminalPulsarRouter> logger;
        private readonly Mock<ITerminalProcessor> processorMock;
        private readonly Mock<IProducer<byte[]>> producerMock;
    }
}