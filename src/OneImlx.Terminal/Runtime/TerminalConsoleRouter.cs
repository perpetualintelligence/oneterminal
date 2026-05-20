//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for console based terminals.
    /// </summary>
    public sealed class TerminalConsoleRouter : ITerminalRouter<TerminalConsoleRouterContext>
    {
        /// <summary>
        /// Initialize a new <see cref="TerminalConsoleRouter"/> instance.
        /// </summary>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="terminalProcessor">The terminal processor.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalConsoleRouter(
            ITerminalConsole terminalConsole,
            ITerminalProcessor terminalProcessor,
            ITerminalExceptionHandler exceptionHandler,
            TerminalOptions options,
            ILogger<TerminalConsoleRouter> logger)
        {
            this.terminalConsole = terminalConsole;
            this.terminalProcessor = terminalProcessor;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether the console terminal is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        public string Name => "console";

        /// <summary>
        /// Runs to the terminal as a console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public async Task RunAsync(TerminalConsoleRouterContext context)
        {
            // Make sure we have supported start context
            if (context.StartMode != TerminalStartMode.Console)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartMode);
            }

            try
            {
                // Start the terminal processing
                terminalProcessor.StartProcessing(context, background: false, responseHandler: null);
                IsRunning = true;
                bool routeOnce = context.RouteOnce.GetValueOrDefault();
                bool routed = false;

                while (true)
                {
                    TerminalInputOutput? terminalIO = null;

                    try
                    {
                        // Wait for a bit to avoid CPU hogging and give time for cancellation token to be set.
                        await Task.Delay(options.Router.RouteDelay).ConfigureAwait(false);

                        // Honor the cancellation request.
                        if (context.TerminalCancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException("Received terminal cancellation token, the terminal console router is canceled.");
                        }

                        // Route once handling for driver programs or indefinite routing for interactive terminals.
                        string? raw = null;
                        if (routeOnce)
                        {
                            // Route once is only valid for driver programs.
                            if (!options.Driver.Enabled)
                            {
                                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The route once is only valid for driver programs.");
                            }

                            // Driver programs executes the program in its entirety. So once the request is routed the
                            // router should stop, even if there was an error.
                            if (routed)
                            {
                                logger.LogDebug("The driver request is routed once, the terminal console router is complete.");
                                break;
                            }

                            // Not yet routed, so route the driver program.
                            string[] args = context.Arguments ?? [];
                            if (args != null)
                            {
                                if (args.Length != 0)
                                {
                                    raw = $"{options.Driver.RootId}{options.Parser.Separator}{args[0]}";
                                }
                                else
                                {
                                    raw = options.Driver.RootId;
                                }
                            }
                        }
                        else
                        {
                            // Print the caret and read the user input.
                            if (options.Router.Caret != null)
                            {
                                await terminalConsole.WriteAsync(options.Router.Caret).ConfigureAwait(false);
                            }
                            raw = await terminalConsole.ReadLineAsync().ConfigureAwait(false);
                        }

                        // Determine if the raw string is to be ignored.
                        if (terminalConsole.Ignore(raw))
                        {
                            // Wait for next command.
                            logger.LogDebug("The raw string is null or ignored by the terminal console.");
                            continue;
                        }

                        // Execute the command asynchronously
                        terminalIO = TerminalInputOutput.Single(Guid.NewGuid().ToString(), raw!);
                        await terminalProcessor.ExecuteAsync(terminalIO);
                    }
                    catch (OperationCanceledException oex)
                    {
                        // Routing is canceled.
                        TerminalExceptionHandlerContext exContext = new(oex, terminalIO?.Requests[0]);
                        await exceptionHandler.HandleExceptionAsync(exContext).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        TerminalExceptionHandlerContext exContext = new(ex.InnerException ?? ex, terminalIO?.Requests[0]);
                        await exceptionHandler.HandleExceptionAsync(exContext).ConfigureAwait(false);
                    }
                    finally
                    {
                        routed = true;
                    }
                }
            }
            finally
            {
                // Stop and wait for some time.
                await terminalProcessor.StopProcessingAsync(2000);
                IsRunning = false;
            }
        }

        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalConsoleRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalConsole terminalConsole;
        private readonly ITerminalProcessor terminalProcessor;
    }
}