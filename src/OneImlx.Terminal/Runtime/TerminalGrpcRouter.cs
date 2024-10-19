﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the gRPC router responsible for managing gRPC communication in the terminal. This router handles
    /// incoming gRPC commands and routes them to the appropriate command runners.
    /// </summary>
    public class TerminalGrpcRouter : ITerminalRouter<TerminalGrpcRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalGrpcRouter"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router for routing commands to specific handlers.</param>
        /// <param name="exceptionHandler">The exception handler for handling errors that occur during command routing.</param>
        /// <param name="options">The options configuration for the terminal router.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalGrpcRouter(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            IOptions<TerminalOptions> options,
            ILogger<TerminalGrpcRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// The command queue for the terminal router.
        /// </summary>
        public TerminalQueue? CommandQueue => commandQueue;

        /// <summary>
        /// Runs the gRPC server asynchronously and begins handling client requests indefinitely. The server will
        /// continue running until a cancellation is requested via the context.
        /// </summary>
        /// <param name="context">The terminal context containing configuration and cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation of running the server.</returns>
        /// <exception cref="TerminalException">Thrown when the start mode is not configured for gRPC.</exception>
        public async Task RunAsync(TerminalGrpcRouterContext context)
        {
            if (context.StartContext.StartMode != TerminalStartMode.Grpc)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Invalid start mode for gRPC.");
            }

            // Initialize the command queue for remote message processing.
            commandQueue = new TerminalQueue(commandRouter, exceptionHandler, options.Value, context, logger);
            try
            {
                logger.LogDebug("Terminal gRPC router started.");

                // Start background command processing and blocking the current thread.
                await commandQueue.StartBackgroundProcessingAsync(context.StartContext.TerminalCancellationToken);
            }
            finally
            {
                logger.LogInformation("Terminal gRPC router stopped.");
            }
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalGrpcRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private TerminalQueue? commandQueue;
    }
}
