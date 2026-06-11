//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandRunner : ICommandRunner<CommandContext, CommandRunnerResult>
    {
        public bool HelpCalled { get; set; }

        public bool RunCalled { get; set; }

        public Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            RunCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }

        public Task RunHelpAsync(CommandContext context)
        {
            HelpCalled = true;
            return Task.CompletedTask;
        }
    }
}