using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Apps.Test.Custom;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", CommandTypes.Root)]
    [OptionDescriptor("version", nameof(String), "Test version description", BehaviorFlags.None, "v")]
    [CommandChecker(typeof(CommandChecker))]
    public class TestRunner : CommandRunner<CustomCommandContext, CustomRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestRunner> logger;

        public TestRunner(ITerminalConsole terminalConsole, ILogger<TestRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CustomRunnerResult> RunCommandAsync(CustomCommandContext context)
        {
            await terminalConsole.WriteLineAsync("Test root command called.");

            // Get the version option value
            if (context.GetCommand().TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CustomRunnerResult();
        }
    }
}