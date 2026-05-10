//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandRunnerMethodInner : ICommandRunnerMethod
    {
        public Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger logger)
        {
            throw new System.NotImplementedException();
        }
    }
}