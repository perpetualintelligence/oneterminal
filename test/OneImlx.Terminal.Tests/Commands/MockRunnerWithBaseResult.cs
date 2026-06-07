//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands
{
    public class MockRunnerWithBaseResult : CommandRunner<CommandContext, CommandRunnerResult>
    {
        public bool MethodCalled { get; private set; }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            MethodCalled = false;
            return Task.FromResult(new CommandRunnerResult());
        }

        public Task<CommandRunnerResult> TestMethodBase(CommandContext context)
        {
            MethodCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}