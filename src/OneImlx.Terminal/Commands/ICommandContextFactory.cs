//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// An abstraction for a <see cref="CommandContext"/> factory.
    /// </summary>
    public interface ICommandContextFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="CommandContext"/>.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        /// <returns>A new instance of <see cref="CommandContext"/>.</returns>
        public ICommandContext Create(CommandRequest request, TerminalRouterContext context, Dictionary<string, object> properties);
    }
}