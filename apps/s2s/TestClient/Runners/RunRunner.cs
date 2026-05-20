//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    /// <summary>
    /// Runs native OS commands.
    /// </summary>
    [CommandDescriptor("run", "Run Command", "Runs a native OS command.", CommandTypes.Native)]
    [ArgumentDescriptor(0, "os", nameof(String), "The full native command to execute, e.g., 'ls -all'", BehaviorFlags.Required)]
    public class RunRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunRunner"/> class.
        /// </summary>
        public RunRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(ICommandContext context)
        {
            var command = context.GetParsedCommand().Command;
            var osCommand = command.GetRequiredArgumentValue<string>("os");

            await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"Running OS command: {osCommand}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{osCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException($"Command error: {error.Trim()}");
            }

            await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"OS command complete: {output}");
            return new CommandRunnerResult(output);
        }

        private readonly ITerminalConsole terminalConsole;
    }
}