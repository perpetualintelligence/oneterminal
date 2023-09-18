﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a context aware terminal routing service.
    /// </summary>
    public interface ITerminalRouting<TContext, TResult> where TContext : TerminalRoutingContext where TResult : TerminalRoutingResult
    {
        /// <summary>
        /// Runs terminal routing asynchronously.
        /// </summary>
        /// <param name="context">The terminal router context.</param>
        /// <returns>The <see cref="TerminalRoutingResult"/> instance.</returns>
        public Task<TResult> RunAsync(TContext context);
    }
}