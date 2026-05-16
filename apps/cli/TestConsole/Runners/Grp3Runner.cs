//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The group <c>grp3</c> runner - INDEPENDENT IsolatedGroup under test.
    /// </summary>
    [CommandOwners("test")]
    [CommandDescriptor("grp3", "Group 3", "Group 3 IsolatedGroup (independent) with cmd7, cmd8, cmd9.", CommandType.IsolatedGroup, CommandFlags.None)]
    [CommandChecker(typeof(CommandChecker))]
    [CommandTags("group", "isolated", "independent")]
    public class Grp3Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp3Runner> logger;

        public Grp3Runner(ITerminalConsole terminalConsole, ILogger<Grp3Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            logger.LogInformation("Executing grp3 command");
            await terminalConsole.WriteLineAsync("Group 3 (IsolatedGroup - independent)");
            await terminalConsole.WriteLineAsync("======================================");
            await terminalConsole.WriteLineAsync("This is an isolated group with separate runner classes.");
            await terminalConsole.WriteLineAsync("Available commands:");
            await terminalConsole.WriteLineAsync("  cmd7 - Command 7");
            await terminalConsole.WriteLineAsync("  cmd8 - Command 8");
            await terminalConsole.WriteLineAsync("  cmd9 - Command 9");
            await terminalConsole.WriteLineAsync("");
            await terminalConsole.WriteLineAsync("Usage: grp3 <command>");
            return new CommandRunnerResult();
        }
    }
}
