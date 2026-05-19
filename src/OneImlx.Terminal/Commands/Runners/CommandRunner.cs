//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// Represents a command runner that defines how a parsed command is executed asynchronously.
    /// The framework resolves and invokes the appropriate runner for each command, enabling
    /// modular and isolated command execution within a terminal application.
    /// </summary>
    public abstract class CommandRunner<TResult> : IDelegateCommandRunner, ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateHelpAsync(ICommandContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;
            this.logger = logger;

            Command command = context.GetCommand();
            logger?.LogDebug("Run help. command={0}", command.Id);

            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateRunAsync(ICommandContext context, ILogger? logger = null)
        {
            this.logger = logger;

            Command command = context.GetCommand();
            logger?.LogDebug("Run command. command={0}", command.Id);

            var result = await RunCommandAsync(context);
            return (CommandRunnerResult)(object)result;
        }

        /// <inheritdoc/>
        public abstract Task<TResult> RunCommandAsync(ICommandContext context);

        /// <inheritdoc/>
        public virtual Task RunHelpAsync(ICommandContext context)
        {
            if (helpProvider == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The help provider is missing in the configured services.");
            }

            return helpProvider.ProvideHelpAsync(new TerminalHelpProviderContext(context.GetCommand()));
        }

        private ITerminalHelpProvider? helpProvider;
        private ILogger? logger;
    }
}