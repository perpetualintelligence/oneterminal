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
    /// The sub-command <c>cmd7</c> runner under grp3.
    /// </summary>
    [CommandOwners("grp3")]
    [CommandDescriptor("cmd7", "Command 7", "Command 7 under grp3.", ReservedCommandTypes.Leaf)]
    public class Cmd7Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd7Runner> logger;

        public Cmd7Runner(ITerminalConsole terminalConsole, ILogger<Cmd7Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            logger.LogInformation("Executing grp3 cmd7");
            await terminalConsole.WriteLineAsync("Executing: grp3 cmd7");
            await terminalConsole.WriteLineAsync("This is a leaf command under isolated group grp3.");
            return new CommandRunnerResult();
        }
    }
}