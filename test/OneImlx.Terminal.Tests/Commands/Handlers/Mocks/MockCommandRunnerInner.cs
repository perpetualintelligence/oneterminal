//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public bool DelegateHelpCalled { get; private set; }

        public bool DelegateRunCalled { get; set; }

        public bool HelpCalled { get; set; }

        public bool RunCalled { get; private set; }

        public async Task<CommandRunnerResult> DelegateHelpAsync(ICommandContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;
            DelegateHelpCalled = true;
            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        public Task<CommandRunnerResult> DelegateRunAsync(ICommandContext context, ILogger? logger = null)
        {
            DelegateRunCalled = true;
            return RunCommandAsync(context);
        }

        public Task<CommandRunnerResult> RunCommandAsync(ICommandContext context)
        {
            RunCalled = true;
            return Task.FromResult<CommandRunnerResult>(new MockCommandRunnerInnerResult());
        }

        public async Task RunHelpAsync(ICommandContext context)
        {
            await helpProvider.ProvideHelpAsync(new TerminalHelpProviderContext(context.GetParsedCommand().Command));
            HelpCalled = true;
        }

        private ITerminalHelpProvider helpProvider = null!;
    }
}