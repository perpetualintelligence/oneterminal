//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.Generic;

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// An abstraction for a <see cref="ICommandContext"/> factory.
    /// </summary>
    public interface ICommandContextFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ICommandContext"/>.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        /// <returns>A new instance of <see cref="ICommandContext"/>.</returns>
        public ICommandContext Create(CommandRequest request, TerminalRouterContext context, Dictionary<string, object> properties);
    }
}