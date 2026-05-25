using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Checkers
{
    public class Cmd3CommandChecker : ICommandChecker
    {
        public Cmd3CommandChecker(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        public Task<CommandCheckerResult> CheckCommandAsync(ICommandContext context)
        {
            terminalConsole.WriteLineAsync("Cmd3 custom checker called.");
            return Task.FromResult(new CommandCheckerResult());
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
