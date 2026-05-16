//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar.Abstractions;

namespace OneImlx.Terminal.Server.Pulsar
{
    /// <summary>
    /// An abstraction for accessing the Apache Pulsar client components.
    /// </summary>
    /// <remarks>
    /// This interface provides access to Apache Pulsar consumer and producer instances required for terminal routing.
    /// Implementations should manage the life-cycle and configuration of the underlying Pulsar clients.
    /// </remarks>
    public interface ITerminalPulsarAccessor
    {
        /// <summary>
        /// Gets the Pulsar consumer for receiving terminal messages.
        /// </summary>
        /// <returns>A configured Pulsar consumer instance.</returns>
        IConsumer<byte[]> GetConsumer();

        /// <summary>
        /// Gets the Pulsar producer for sending terminal responses.
        /// </summary>
        /// <returns>A configured Pulsar producer instance.</returns>
        IProducer<byte[]> GetProducer();
    }
}