//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// An abstraction of a command runner method.
    /// </summary>
    public interface ICommandRunnerMethod
    {
        /// <summary>
        /// Runs the command method as a command, represented by the specified context and logger.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a CommandRunnerResult that
        /// describes the outcome of the command execution.</returns>
        Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger logger);
    }
}