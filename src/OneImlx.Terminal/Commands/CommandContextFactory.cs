//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Default implementation of <see cref="ICommandContextFactory"/>.
    /// </summary>
    public sealed class CommandContextFactory : ICommandContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="CommandContext"/>.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="context">The <see cref="TerminalRouterContext"/>.</param>
        /// <param name="properties">Additional properties.</param>
        /// <returns>A new <see cref="CommandContext"/> instance.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ICommandContext Create(CommandRequest request, TerminalRouterContext context, Dictionary<string, object> properties)
        {
            return new CommandContext(request, context, properties);
        }
    }
}