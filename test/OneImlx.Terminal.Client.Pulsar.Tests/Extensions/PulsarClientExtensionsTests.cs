//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Client.Pulsar.Extensions;
using OneImlx.Terminal.Shared;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Client.Pulsar.Tests.Extensions
{
    public class PulsarClientExtensionsTests
    {
        public PulsarClientExtensionsTests()
        {
            producerMock = new Mock<IProducer<byte[]>>();
            sendCallCount = 0;

            producerMock.Setup(p => p.Send(It.IsAny<MessageMetadata>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Callback<MessageMetadata, byte[], CancellationToken>((metadata, data, token) =>
                {
                    capturedData = data; // Capture the data for validation
                    sendCallCount++;  // Track the call count
                })
                .Returns(() => ValueTask.FromResult(new MessageId(1, 1, 0, 0))); // Return a fresh ValueTask each time
        }

        [Fact]
        public async Task SendToTerminalAsync_Sends_Input_As_Batch_Correctly()
        {
            // Arrange
            var cmdIds = new[] { "id1", "id2", "id3" };
            var commands = new[] { "command1", "command2", "command3" };

            // Act
            TerminalInputOutput input = TerminalInputOutput.Batch("batch1", cmdIds, commands);
            var response = await producerMock.Object.SendToTerminalAsync(input, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured data with FluentAssertions
            capturedData.Should().NotBeNull();
            var json = Encoding.UTF8.GetString(capturedData!);
            json.Should().Be("{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"command1\",\"result\":null},{\"id\":\"id2\",\"is_error\":false,\"raw\":\"command2\",\"result\":null},{\"id\":\"id3\",\"is_error\":false,\"raw\":\"command3\",\"result\":null}],\"sender_endpoint\":null,\"sender_id\":null}");

            // Ensure that Send was called exactly once
            sendCallCount.Should().Be(1);
        }

        [Fact]
        public async Task SendToTerminalAsync_Sends_Input_As_Single_Correctly()
        {
            // Act
            TerminalInputOutput input = TerminalInputOutput.Single("id1", raw: "test-command");
            var response = await producerMock.Object.SendToTerminalAsync(input, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured data with FluentAssertions
            capturedData.Should().NotBeNull();
            var json = Encoding.UTF8.GetString(capturedData!);
            json.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"test-command\",\"result\":null}],\"sender_endpoint\":null,\"sender_id\":null}");

            // Ensure that Send was called exactly once
            sendCallCount.Should().Be(1);
        }

        [Fact]
        public async Task SendToTerminalAsync_WithNullProducer_ThrowsArgumentNullException()
        {
            // Arrange
            IProducer<byte[]>? producer = null;
            var input = TerminalInputOutput.Single("cmd1", "test command");

            // Act & Assert
            await FluentActions.Awaiting(() => producer!.SendToTerminalAsync(input, CancellationToken.None))
                .Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("producer");
        }

        [Fact]
        public async Task SendToTerminalAsync_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            TerminalInputOutput? input = null;

            // Act & Assert
            await FluentActions.Awaiting(() => producerMock.Object.SendToTerminalAsync(input!, CancellationToken.None))
                .Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("input");
        }

        private readonly Mock<IProducer<byte[]>> producerMock;
        private byte[]? capturedData;
        private int sendCallCount;
    }
}