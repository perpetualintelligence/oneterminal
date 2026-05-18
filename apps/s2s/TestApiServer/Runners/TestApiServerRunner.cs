using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Apps.TestApiServer.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the <see cref="TestApiServer"/>.
    /// </summary>
    [CommandDescriptor("ts", "Test Server", "Test server description.", CommandTypes.Root)]
    [OptionDescriptor("version", nameof(String), "Test server version description", BehaviorFlags.None, "v")]
    public class TestApiServerRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestApiServerRunner> logger;

        public TestApiServerRunner(ITerminalConsole terminalConsole, ILogger<TestApiServerRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Test API server root command called.");

            // Get the version option value
            if (context.EnsureParsedCommand().Command.TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }
    }
}