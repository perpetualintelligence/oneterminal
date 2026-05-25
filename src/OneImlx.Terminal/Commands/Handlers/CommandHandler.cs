//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default handler to handle a command request routed from a <see cref="CommandRouter"/>.
    /// </summary>
    public sealed class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(ICommandResolver commandResolver, IOptions<TerminalOptions> options, ITerminalHelpProvider terminalHelpProvider, ILogger<CommandHandler> logger, ITerminalEventHandler? terminalEventHandler = null)
        {
            this.commandRuntime = commandResolver ?? throw new ArgumentNullException(nameof(commandResolver));
            this.terminalOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.terminalHelpProvider = terminalHelpProvider ?? throw new ArgumentNullException(nameof(terminalHelpProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.terminalEventHandler = terminalEventHandler;
        }

        /// <inheritdoc/>
        public async Task HandleCommandAsync(ICommandContext context)
        {
            logger.LogDebug("Handle request. request={0}", context.Request.Id);

            // Check and run the command
            (CommandCheckerResult checkerResult, CommandRunnerResult runnerResult) = await CheckAndRunCommandInnerAsync(context).ConfigureAwait(false);

            // Return the processed result
            context.SetCommandResult(new CommandResult(checkerResult, runnerResult));
        }

        private async Task<(CommandCheckerResult, CommandRunnerResult)> CheckAndRunCommandInnerAsync(ICommandContext context)
        {
            Command command = context.GetCommand();

            // If we are executing a help command then we need to bypass all the checks.
            if (terminalOptions.Help.Enabled)
            {
                if (command.TryGetOption(terminalOptions.Help.OptionId, out Option? helpOption) || command.TryGetOption(terminalOptions.Help.OptionAlias, out helpOption))
                {
                    logger.LogDebug("Found help option. option={0}", helpOption?.CommandId ?? "?");
                    CommandRunnerResult runnerResult = await RunCommandInnerAsync(context, command, runHelp: true).ConfigureAwait(false);
                    return (new CommandCheckerResult(), runnerResult);
                }
            }

            CommandCheckerResult checkerResult = await CheckCommandInnerAsync(context, command).ConfigureAwait(false);
            CommandRunnerResult runResult = await RunCommandInnerAsync(context, command, runHelp: false).ConfigureAwait(false);
            return (checkerResult, runResult);
        }

        private async Task<CommandCheckerResult> CheckCommandInnerAsync(ICommandContext context, Command command)
        {
            // Issue a before check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandCheckAsync), command.Id);
                await terminalEventHandler.BeforeCommandCheckAsync(command).ConfigureAwait(false);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = commandRuntime.ResolveCommandChecker(command.Descriptor);
            CommandCheckerResult result = await commandChecker.CheckCommandAsync(context).ConfigureAwait(false);

            // Issue a after check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandCheckAsync), command.Id);
                await terminalEventHandler.AfterCommandCheckAsync(command, result).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<CommandRunnerResult> RunCommandInnerAsync(ICommandContext context, Command command, bool runHelp)
        {
            // Issue a before run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandRunAsync), command.Id);
                await terminalEventHandler.BeforeCommandRunAsync(command).ConfigureAwait(false);
            }

            // Find the runner to run the command
            IDelegateCommandRunner commandRunner = commandRuntime.ResolveCommandRunner(command.Descriptor);
            CommandRunnerResult runnerResult;

            // Run or Help
            if (runHelp)
            {
                logger.LogDebug("Skip runner. Delegate to help provider. type={0}", terminalHelpProvider.GetType().Name);
                runnerResult = await commandRunner.DelegateHelpAsync(context, terminalHelpProvider, logger).ConfigureAwait(false);
            }
            else
            {
                runnerResult = await commandRunner.DelegateRunAsync(context, logger).ConfigureAwait(false);
            }

            // Issue a after run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandRunAsync), command.Id);
                await terminalEventHandler.AfterCommandRunAsync(command, runnerResult).ConfigureAwait(false);
            }

            return runnerResult;
        }

        private readonly ICommandResolver commandRuntime;
        private readonly ILogger<CommandHandler> logger;
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalEventHandler? terminalEventHandler;
        private readonly ITerminalHelpProvider terminalHelpProvider;
    }
}