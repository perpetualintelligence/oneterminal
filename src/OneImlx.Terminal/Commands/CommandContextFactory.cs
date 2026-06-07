//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Default  <see cref="ICommandContextFactory"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="CommandContextFactory"/> expects the <see cref="CommandContext"/> to have a constructor that accepts a <see cref="Dictionary{String, Object}"/> for properties. If the context type does not have such a constructor, an exception will be thrown at runtime.
    /// </remarks>
    public sealed class CommandContextFactory : ICommandContextFactory
    {
        /// <inheritdoc/>
        public CommandContext Create(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties)
        {
            return Create<CommandContext>(request, routerContext, properties);
        }

        /// <inheritdoc/>
        public TContext Create<TContext>(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties) where TContext : CommandContext
        {
            TContext context = (TContext)System.Activator.CreateInstance(typeof(TContext), properties);
            context.SetCommandRequest(request);
            context.SetRouterContext(routerContext);
            return context;
        }
    }
}