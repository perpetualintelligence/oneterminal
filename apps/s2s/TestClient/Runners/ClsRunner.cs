using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    /// <summary>
    /// Clears the console.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ClsRunner"/> class.
    /// </remarks>
    /// <param name="terminalConsole">The terminal console.</param>
    [CommandDescriptor("cls", "Clear Console", "Clears the console.", CommandTypes.Native)]
    public class ClsRunner(ITerminalConsole terminalConsole) : CommandRunner<CommandContext, CommandRunnerResult>, IDeclarativeRunner
    {

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.ClearAsync();
            return CommandRunnerResult.Empty();
        }

        private readonly ITerminalConsole terminalConsole = terminalConsole;
    }
}