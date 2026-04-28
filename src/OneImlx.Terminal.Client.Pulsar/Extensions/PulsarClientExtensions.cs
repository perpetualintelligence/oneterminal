//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using OneImlx.Terminal.Shared;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Client.Pulsar.Extensions
{
    /// <summary>
    /// Extension methods for sending terminal commands via Pulsar.
    /// </summary>
    public static class PulsarClientExtensions
    {
        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> to the terminal via Pulsar producer.
        /// </summary>
        /// <param name="producer">The Pulsar producer.</param>
        /// <param name="input">The terminal input to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="serializeOptions">Optional JSON serialization options for customizing the serialization behavior. If <c>null</c>, default options are used.</param>
        /// <returns>The Pulsar message ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="producer"/> or <paramref name="input"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This method serializes the <paramref name="input"/> to UTF-8 JSON bytes,
        /// and sends the message to Pulsar.
        /// </remarks>
        public static async Task<MessageId> SendToTerminalAsync(this IProducer<byte[]> producer, TerminalInputOutput input, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (producer == null)
            {
                throw new ArgumentNullException(nameof(producer));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            byte[] messageBytes = JsonSerializer.SerializeToUtf8Bytes(input, serializeOptions);
            return await producer.Send(messageBytes, cancellationToken);
        }
    }
}