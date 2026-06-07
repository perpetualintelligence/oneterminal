using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Apps.Test.Custom;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// Exits the client terminal application.
    /// </summary>
    [CommandDescriptor("exit", "Exit", "Exits the client terminal application.", CommandTypes.Native)]
    public class ExitRunner : CommandRunner<CustomCommandContext, CustomRunnerResult>, IDeclarativeRunner
    {
        public ExitRunner(ITerminalConsole terminalConsole, IHostApplicationLifetime applicationLifetime)
        {
            this.terminalConsole = terminalConsole;
            _applicationLifetime = applicationLifetime;
        }

        /// <inheritdoc/>
        public override async Task<CustomRunnerResult> RunCommandAsync(CustomCommandContext context)
        {
            string answer = await terminalConsole.ReadAnswerAsync("Are you sure you want to exit ?", "y", "Y");
            if (answer == "y" || answer == "Y")
            {
                _applicationLifetime.StopApplication();
                await Task.Delay(2000);
            }
            return new CustomRunnerResult();
        }

        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ITerminalConsole terminalConsole;
    }
}