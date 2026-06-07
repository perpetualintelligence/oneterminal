using System.Threading.Tasks;
using OneImlx.Terminal.Apps.Test.Custom;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// Displays all supported commands.
    /// </summary>
    [CommandDescriptor("help", "Help Command", "Displays all supported commands.", CommandTypes.Native)]
    public class HelpRunner : CommandRunner<CustomCommandContext, CustomRunnerResult>, IDeclarativeRunner
    {
        public HelpRunner(ITerminalConsole console, ITerminalCommandStore commandStore)
        {
            _console = console;
            _commandStore = commandStore;
        }

        /// <inheritdoc/>
        public override async Task<CustomRunnerResult> RunCommandAsync(CustomCommandContext context)
        {
            var commands = await _commandStore.AllAsync();
            foreach (var command in commands)
            {
                await _console.WriteLineAsync($"{command.Key} ({command.Value.Name}) --> {command.Value.Description}");
            }
            return new CustomRunnerResult();
        }

        private readonly ITerminalConsole _console;
        private readonly ITerminalCommandStore _commandStore;
    }
}