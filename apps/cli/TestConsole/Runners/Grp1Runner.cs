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
    /// The group <c>grp1</c> runner demonstrating CompositeGroup with run methods.
    /// </summary>
    [CommandOwners("test")]
    [CommandDescriptor("grp1", "Group 1", "Group 1 as a composite group", CommandTypes.CompositeGroup)]
    [CommandChecker(typeof(CommandChecker))]
    [CommandTags("group", "composite")]
    public class Grp1Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp1Runner> logger;

        public Grp1Runner(ITerminalConsole terminalConsole, ILogger<Grp1Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            logger.LogInformation("Executing grp1 base command");
            await terminalConsole.WriteLineAsync("Group 1 (CompositeGroup)");
            await terminalConsole.WriteLineAsync("========================");
            await terminalConsole.WriteLineAsync("Available subcommands:");
            await terminalConsole.WriteLineAsync("  cmd1 - Command 1");
            await terminalConsole.WriteLineAsync("  cmd2 - Command 2");
            await terminalConsole.WriteLineAsync("  cmd3 - Command 3");
            await terminalConsole.WriteLineAsync("  grp2 - Sub-group 2");
            await terminalConsole.WriteLineAsync("");
            await terminalConsole.WriteLineAsync("Usage: grp1 <subcommand> [arguments] [options]");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd1", "Command 1", "Command 1 in grp1.", CommandTypes.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(String), "First argument", BehaviorFlags.None)]
        [OptionDescriptor("opt1", nameof(String), "Option 1", BehaviorFlags.None)]
        public async Task<CommandRunnerResult> Cmd1Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 cmd1");
            string arg1 = context.EnsureCommand().GetRequiredArgumentValue<string>("arg1");
            string opt1 = context.EnsureCommand().GetRequiredOptionValue<string>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 cmd1 executed: arg1={arg1}, opt1={opt1}");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd2", "Command 2", "Command 2 in grp1.", CommandTypes.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(Int32), "Integer argument", BehaviorFlags.Required)]
        [ArgumentValidation("arg1", typeof(RequiredAttribute))]
        [OptionDescriptor("opt1", nameof(Boolean), "Boolean option", BehaviorFlags.None)]
        public async Task<CommandRunnerResult> Cmd2Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 cmd2");
            int arg1 = context.EnsureCommand().GetRequiredArgumentValue<int>("arg1");
            bool opt1 = context.EnsureCommand().GetRequiredOptionValue<bool>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 cmd2 executed: arg1={arg1}, opt1={opt1}");
            return new CommandRunnerResult();
        }

        [CommandDescriptor("cmd3", "Command 3", "Command 3 in grp1.", CommandTypes.Leaf)]
        [CommandTags("command", "leaf")]
        [ArgumentDescriptor(1, "arg1", nameof(String), "String argument", BehaviorFlags.Required)]
        [ArgumentValidation("arg1", typeof(RequiredAttribute))]
        [ArgumentValidation("arg1", typeof(StringLengthAttribute), 50)]
        [OptionDescriptor("opt1", nameof(String), "String option", BehaviorFlags.None)]
        public async Task<CommandRunnerResult> Cmd3Async(CommandContext context)
        {
            logger.LogInformation("Executing grp1 cmd3");
            string arg1 = context.EnsureCommand().GetRequiredArgumentValue<string>("arg1");
            string opt1 = context.EnsureCommand().GetRequiredOptionValue<string>("opt1");
            await terminalConsole.WriteLineAsync($"grp1 cmd3 executed: arg1={arg1}, opt1={opt1}");
            return new CommandRunnerResult();
        }
    }
}
