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
    [CommandOwners("ts")]
    [CommandDescriptor("grp1", "Group 1", "Group1 description.", CommandTypes.IsolatedGroup)]
    [CommandChecker(typeof(CommandChecker))]
    public class Grp1Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp1Runner> logger;

        public Grp1Runner(ITerminalConsole terminalConsole, ILogger<Grp1Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(ICommandContext context)
        {
            await terminalConsole.WriteLineAsync("Group1 command called.");
            return new CommandRunnerResult("Response from grp1");
        }
    }
}