//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar;
using DotPulsar.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Server.Pulsar
{
    /// <summary>
    /// Test implementation of IConsumer that allows us to control the message stream.
    /// This is necessary because we cannot mock extension methods like Messages().
    /// </summary>
    public class MockConsumer : IConsumer<byte[]>
    {
        private readonly IMessage<byte[]> _message;
        private readonly CancellationTokenSource _cts;
        private readonly int _messageCount;
        private bool _acknowledged;

        public MockConsumer(IMessage<byte[]> message, CancellationTokenSource cts, int messageCount = 1)
        {
            _message = message;
            _cts = cts;
            _messageCount = messageCount;
        }

        public async IAsyncEnumerable<IMessage<byte[]>> Messages([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < _messageCount && !cancellationToken.IsCancellationRequested && !_cts.Token.IsCancellationRequested; i++)
            {
                yield return _message;
            }

            //// After yielding all messages, wait for cancellation
            //while (!cancellationToken.IsCancellationRequested && !_cts.Token.IsCancellationRequested)
            //{
            //    await Task.Delay(10, cancellationToken);
            //}
        }

        public ValueTask<IMessage<byte[]>> Receive(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_message);
        }

        public ValueTask Acknowledge(MessageId messageId, CancellationToken cancellationToken = default)
        {
            _acknowledged = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask Acknowledge(IMessage<byte[]> message, CancellationToken cancellationToken = default)
        {
            _acknowledged = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask Acknowledge(IEnumerable<MessageId> messageIds, CancellationToken cancellationToken = default)
        {
            _acknowledged = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask AcknowledgeCumulative(MessageId messageId, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask AcknowledgeCumulative(IMessage<byte[]> message, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask NegativeAcknowledge(MessageId messageId, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask NegativeAcknowledge(IMessage<byte[]> message, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask RedeliverUnacknowledgedMessages(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask RedeliverUnacknowledgedMessages(IEnumerable<MessageId> messageIds, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Seek(MessageId messageId, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Seek(ulong publishTime, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Unsubscribe(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<IEnumerable<MessageId>> GetLastMessageIds(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IEnumerable<MessageId>>(new List<MessageId>());
        }

        public bool IsFinalState() => false;

        public bool IsFinalState(ConsumerState state) => state == ConsumerState.Closed || state == ConsumerState.Faulted;

        public ValueTask<ConsumerState> OnStateChangeTo(ConsumerState state, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(state);
        }

        public ValueTask<ConsumerState> OnStateChangeFrom(ConsumerState state, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(state);
        }

        public Uri ServiceUrl => new Uri("pulsar://localhost:6650");

        public string SubscriptionName => "test-subscription";

        public SubscriptionType SubscriptionType => SubscriptionType.Exclusive;

        public string Topic => "test-topic";

        public bool IsAcknowledged => _acknowledged;
    }
}