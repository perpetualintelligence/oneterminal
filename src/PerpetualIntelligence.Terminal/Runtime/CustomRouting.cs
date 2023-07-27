﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext, TResult}"/> for custom routing.
    /// </summary>
    public abstract class CustomRouting : ITerminalRouting<CustomRoutingContext, CustomRoutingResult>
    {
        /// <summary>
        /// Routes to a custom service implementation.
        /// </summary>
        /// <param name="context">The custom routing service context.</param>
        public abstract Task<CustomRoutingResult> RunAsync(CustomRoutingContext context);
    }
}