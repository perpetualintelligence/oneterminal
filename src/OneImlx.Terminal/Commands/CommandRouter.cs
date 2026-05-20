//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Shared;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The default <see cref="ICommandRouter"/>.
    /// </summary>
    public sealed class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="licenseExtractor">The license extractor.</param>
        /// <param name="commandParser">The command parser.</param>
        /// <param name="commandHandler">The command handler.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="asyncEventHandler">The event handler.</param>
        public CommandRouter(TerminalOptions terminalOptions, ICommandParser commandParser, ICommandHandler commandHandler, ILogger<CommandRouter> logger, ITerminalEventHandler? asyncEventHandler = null)
        {
            this.commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            this.logger = logger;
            this.asyncEventHandler = asyncEventHandler;
        }

        /// <summary>
        /// Routes the command request to the registered handler.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The <see cref="CommandResult"/> instance.</returns>
        public async Task RouteCommandAsync(ICommandContext context)
        {
            ParsedCommand? parsedCommand = null;
            CommandResult? commandResult = null!;
            CommandRequest request = context.Request;
            string requestId = request.Id;

            try
            {
                logger.LogDebug("Start command router. type={0} request={1}", GetType().Name, requestId);

                // Issue a before request event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} request={1}", nameof(asyncEventHandler.BeforeCommandRouteAsync), requestId);
                    await asyncEventHandler.BeforeCommandRouteAsync(request).ConfigureAwait(false);
                }

                // Parse the command
                await commandParser.ParseCommandAsync(context).ConfigureAwait(false);
                context.TryGetParsedCommand(out parsedCommand);

                // Handle the command
                await commandHandler.HandleCommandAsync(context).ConfigureAwait(false);
                context.TryGetCommandResult(out commandResult);
            }
            finally
            {
                // Issue a after request event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} request={1}", nameof(asyncEventHandler.AfterCommandRouteAsync), requestId);
                    await asyncEventHandler.AfterCommandRouteAsync(request, parsedCommand?.Command, commandResult).ConfigureAwait(false);
                }

                logger.LogDebug("End command router. request={0}", requestId);
            }
        }

        private readonly ITerminalEventHandler? asyncEventHandler;
        private readonly ICommandHandler commandHandler;
        private readonly ICommandParser commandParser;
        private readonly ILogger<CommandRouter> logger;
        private readonly TerminalOptions terminalOptions;
    }
}