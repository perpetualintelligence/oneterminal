//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.Generic;

namespace OneImlx.Terminal.Shared
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
        /// <param name="routerContext">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        /// <returns>A new instance of <see cref="CommandContext"/>.</returns>
        public CommandContext Create(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties);

        /// <summary>
        /// Creates a new instance of <see cref="CommandContext"/> of the specified type.
        /// </summary>
        /// <typeparam name="TContext">The context type. Must inherit from <see cref="CommandContext"/>.</typeparam>
        /// <param name="request">The request to process.</param>
        /// <param name="routerContext">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        /// <returns>A new instance of <typeparamref name="TContext"/>.</returns>
        public TContext Create<TContext>(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties) where TContext : CommandContext;
    }
}