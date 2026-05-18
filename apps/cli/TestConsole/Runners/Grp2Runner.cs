using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The group <c>grp2</c> runner under grp1 demonstrating CompositeGroup with run methods.
    /// </summary>
    [CommandOwners("grp1")]
    [CommandDescriptor("grp2", "Group 2", "Group 2 CompositeGroup under grp1 with cmd4, cmd5, cmd6.", CommandType.CompositeGroup)]
    [CommandChecker(typeof(CommandChecker))]
    [CommandTags("group", "composite", "nested")]
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
            logger.LogInformation("Executing grp2 base command");
            await terminalConsole.WriteLineAsync("Group 2 (CompositeGroup under grp1)");
            await terminalConsole.WriteLineAsync("====================================");
            await terminalConsole.WriteLineAsync("Available subcommands:");
            await terminalConsole.WriteLineAsync("  cmd4 - Command 4");
            await terminalConsole.WriteLineAsync("  cmd5 - Command 5");
            await terminalConsole.WriteLineAsync("  cmd6 - Command 6");
            await terminalConsole.WriteLineAsync("");
            await terminalConsole.WriteLineAsync("Usage: grp1 grp2 <subcommand> [arguments] [options]");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd4", "Command 4", "Command 4 in grp2.", CommandType.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(String), "First argument", ReservedFlags.None)]
        [ArgumentDescriptor(2, "arg2", nameof(String), "Second argument", ReservedFlags.None)]
        [OptionDescriptor("opt1", nameof(String), "Option 1", ReservedFlags.None)]
        public async Task<CommandRunnerResult> Cmd4Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 grp2 cmd4");
            string arg1 = context.EnsureCommand().GetRequiredArgumentValue<string>("arg1");
            string arg2 = context.EnsureCommand().GetRequiredArgumentValue<string>("arg2");
            string opt1 = context.EnsureCommand().GetRequiredOptionValue<string>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 grp2 cmd4 executed: arg1={arg1}, arg2={arg2}, opt1={opt1}");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd5", "Command 5", "Command 5 in grp2.", CommandType.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(Int32), "Integer argument", ReservedFlags.Required)]
        [ArgumentValidation("arg1", typeof(RequiredAttribute))]
        [ArgumentValidation("arg1", typeof(RangeAttribute), 1, 100)]
        [OptionDescriptor("opt1", nameof(Boolean), "Boolean option", ReservedFlags.None)]
        public async Task<CommandRunnerResult> Cmd5Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 grp2 cmd5");
            int arg1 = context.EnsureCommand().GetRequiredArgumentValue<int>("arg1");
            bool opt1 = context.EnsureCommand().GetRequiredOptionValue<bool>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 grp2 cmd5 executed: arg1={arg1}, opt1={opt1}");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd6", "Command 6", "Command 6 in grp2.", CommandType.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(Boolean), "Boolean argument", ReservedFlags.None)]
        [OptionDescriptor("opt1", nameof(String), "String option", ReservedFlags.Required)]
        [OptionValidation("opt1", typeof(RequiredAttribute))]
        public async Task<CommandRunnerResult> Cmd6Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 grp2 cmd6");
            bool arg1 = context.EnsureCommand().GetRequiredArgumentValue<bool>("arg1");
            string opt1 = context.EnsureCommand().GetRequiredOptionValue<string>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 grp2 cmd6 executed: arg1={arg1}, opt1={opt1}");
            return new CommandRunnerResult();
        }
    }
}
