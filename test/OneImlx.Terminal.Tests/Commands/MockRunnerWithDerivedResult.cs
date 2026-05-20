//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands
{
    public class MockRunnerWithDerivedResult : CommandRunner<MockRunnerDerviedResult>
    {
        public bool MethodCalled { get; private set; }

        public override Task<MockRunnerDerviedResult> RunCommandAsync(ICommandContext context)
        {
            MethodCalled = false;
            return Task.FromResult(new MockRunnerDerviedResult());
        }

        public Task<MockRunnerDerviedResult> TestMethodDerived(ICommandContext context)
        {
            MethodCalled = true;
            return Task.FromResult(new MockRunnerDerviedResult());
        }
    }
}