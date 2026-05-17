using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The sub-command <c>cmd8</c> runner under grp3.
    /// </summary>
    [CommandOwners("grp3")]
    [CommandDescriptor("cmd8", "Command 8", "Command 8 under grp3.", CommandType.Leaf)]
    public class Cmd8Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd8Runner> logger;

        public Cmd8Runner(ITerminalConsole terminalConsole, ILogger<Cmd8Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            logger.LogInformation("Executing grp3 cmd8");
            await terminalConsole.WriteLineAsync("Executing: grp3 cmd8");
            await terminalConsole.WriteLineAsync("This is a leaf command under isolated group grp3.");
            return new CommandRunnerResult();
        }
    }
}