﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext, TResult}"/> for console based terminals.
    /// </summary>
    public class TerminalConsoleRouting : ITerminalRouting<TerminalConsoleRoutingContext, TerminalConsoleRoutingResult>
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly TerminalOptions options;
        private readonly ILogger<TerminalConsoleRouting> logger;

        /// <summary>
        /// Initialize a new <see cref="TerminalConsoleRouting"/> instance.
        /// </summary>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalConsoleRouting(ITerminalConsole terminalConsole, IHostApplicationLifetime applicationLifetime, ICommandRouter commandRouter, IExceptionHandler exceptionHandler, IErrorHandler errorHandler, TerminalOptions options, ILogger<TerminalConsoleRouting> logger)
        {
            this.terminalConsole = terminalConsole;
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.errorHandler = errorHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Routes to the console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public virtual Task<TerminalConsoleRoutingResult> RunAsync(TerminalConsoleRoutingContext context)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                while (true)
                {
                    // Avoid blocking threads during cancellation and let the
                    // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                    // app shutdown
                    await Task.Delay(options.Router.SyncDelay.GetValueOrDefault());

                    // Honor the cancellation request.
                    if (context.StartContext.CancellationToken.IsCancellationRequested)
                    {
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(TerminalErrors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                        await errorHandler.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Check if application is stopping
                    if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(TerminalErrors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                        await errorHandler.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Print the caret
                    if (options.Router.Caret != null)
                    {
                        await terminalConsole.WriteAsync(options.Router.Caret);
                    }

                    // Read the user input
                    string? raw = await terminalConsole.ReadLineAsync();

                    // Ignore empty commands
                    if (raw == null || terminalConsole.Ignore(raw))
                    {
                        // Wait for next command.
                        continue;
                    }

                    try
                    {
                        // Route the request.
                        CommandRouterContext routerContext = new(raw, context.StartContext.CancellationToken);
                        Task<CommandRouterResult> routeTask = commandRouter.RouteAsync(routerContext);

                        bool success = routeTask.Wait(options.Router.Timeout, context.StartContext.CancellationToken);
                        if (!success)
                        {
                            throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        ExceptionHandlerContext exContext = new(ex.InnerException ?? ex, raw);
                        await exceptionHandler.HandleAsync(exContext);
                    }
                };

                // Return Result
                return new TerminalConsoleRoutingResult();
            });
        }
    }
}