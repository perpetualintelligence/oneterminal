//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// A command runner that executes a specific method in an integrated group runner using a pre-compiled delegate.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance with a pre-compiled delegate.
    /// </remarks>
    /// <param name="runDelegate">The pre-compiled delegate that invokes the specific method.</param>
    public sealed class CommandRunnerMethod(Func<CommandContext, Task<CommandRunnerResult>> runDelegate) : ICommandRunnerMethod
    {
        private readonly Func<CommandContext, Task<CommandRunnerResult>> runDelegate = runDelegate ?? throw new ArgumentNullException(nameof(runDelegate));

        /// <summary>
        /// Delegates the command execution to the pre-compiled method delegate.
        /// </summary>
        public Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger logger)
        {
            return runDelegate(context);
        }
    }
}