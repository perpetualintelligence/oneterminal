//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// Represents a strongly-typed delegate for executing a command runner method.
    /// </summary>
    /// <param name="runner">The command runner instance.</param>
    /// <param name="context">The command context.</param>
    /// <returns>The command runner result.</returns>
    public delegate Task<CommandRunnerResult> CommandRunnerDelegate(object runner, CommandContext context);
}