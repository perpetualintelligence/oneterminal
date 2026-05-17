using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Apps.TestApiServer.Runners
{
    /// <summary>
    /// The group <c>grp1</c> runner for the <see cref="TestApiServer"/>.
    /// </summary>
    [CommandOwners("grp1")]
    [CommandDescriptor("grp2", "Group 2", "Group2 description.", CommandType.IsolatedGroup)]
    [CommandChecker(typeof(CommandChecker))]
    public class Grp2Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp2Runner> logger;

        public Grp2Runner(ITerminalConsole terminalConsole, ILogger<Grp2Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Group2 command called.");
            return new CommandRunnerResult("Response from grp2");
        }
    }
}