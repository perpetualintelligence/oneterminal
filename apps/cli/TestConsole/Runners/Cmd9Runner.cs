using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Apps.Test.Checkers;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The sub-command <c>cmd9</c> runner under grp3 with custom checker.
    /// </summary>
    [CommandOwners("grp3")]
    [CommandDescriptor("cmd9", "Command 9", "Command 9 under grp3 with custom checker.", CommandTypes.Leaf)]
    [CommandChecker(typeof(Cmd3CommandChecker))]
    public class Cmd9Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd9Runner> logger;

        public Cmd9Runner(ITerminalConsole terminalConsole, ILogger<Cmd9Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            logger.LogInformation("Executing grp3 cmd9");
            await terminalConsole.WriteLineAsync("Executing: grp3 cmd9");
            await terminalConsole.WriteLineAsync("This is a leaf command with custom checker under grp3.");
            return new CommandRunnerResult();
        }
    }
}
