//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Buffers;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Server.Pulsar
{
    /// <summary>
    /// Represents the Pulsar router responsible for managing Pulsar communication in the terminal. This router handles
    /// incoming Pulsar messages and routes them to the appropriate command runners.
    /// </summary>
    public class TerminalPulsarRouter : ITerminalRouter<TerminalPulsarRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalPulsarRouter"/> class.
        /// </summary>
        /// <param name="terminalProcessor">The terminal router queue.</param>
        /// <param name="options">The options configuration for the terminal router.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        /// <param name="terminalPulsarAccessor">The terminal Pulsar accessor.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        public TerminalPulsarRouter(
            ITerminalProcessor terminalProcessor,
            IOptions<TerminalOptions> options,
            ILogger<TerminalPulsarRouter> logger,
            ITerminalPulsarAccessor terminalPulsarAccessor,
            ITerminalExceptionHandler exceptionHandler)
        {
            this.terminalProcessor = terminalProcessor;
            this.options = options;
            this.logger = logger;
            this.terminalPulsarAccessor = terminalPulsarAccessor;
            this.exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalPulsarRouter"/> is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        public string Name => "pulsar";

        /// <summary>
        /// Runs the Pulsar consumer asynchronously and begins handling messages indefinitely. The consumer will
        /// continue running until a cancellation is requested via the context.
        /// </summary>
        /// <param name="context">The terminal context containing configuration and cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation of running the consumer.</returns>
        /// <exception cref="TerminalException">Thrown when the start mode is not configured for Pulsar.</exception>
        public async Task RunAsync(TerminalPulsarRouterContext context)
        {
            if (context.StartMode != TerminalStartMode.Pulsar)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Invalid start mode for Pulsar.");
            }

            try
            {
                // Pulsar is command and response so start command processing without background queue.
                logger.LogDebug("Terminal Pulsar router started.");
                IsRunning = true;
                terminalProcessor.StartProcessing(context, background: false);

                // Asynchronously consume messages from the Pulsar topic and process them until cancellation is requested.
                // The Messages is an extension method that yields messages as they arrive, until cancelled.
                IConsumer<byte[]> consumer = terminalPulsarAccessor.GetConsumer();
                IProducer<byte[]> producer = terminalPulsarAccessor.GetProducer();
                await foreach (var message in consumer.Messages(context.TerminalCancellationToken))
                {
                    TerminalInputOutput? input = JsonSerializer.Deserialize<TerminalInputOutput>(message.Data.ToArray());
                    if (input == null || input.Count <= 0)
                    {
                        throw new TerminalException(TerminalErrors.MissingCommand, "The input requests are missing in the Pulsar message.");
                    }

                    await terminalProcessor.ExecuteAsync(input);
                    byte[] outputJson = JsonSerializer.SerializeToUtf8Bytes(input);
                    await producer.Send(outputJson, context.TerminalCancellationToken);
                    await consumer.Acknowledge(message, context.TerminalCancellationToken);
                }
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                await terminalProcessor.StopProcessingAsync(options.Value.Router.Timeout);
                logger.LogInformation("Terminal Pulsar router stopped.");
                IsRunning = false;
            }
        }

        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalPulsarRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ITerminalPulsarAccessor terminalPulsarAccessor;
        private readonly ITerminalProcessor terminalProcessor;
    }
}