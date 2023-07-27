﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRoutingContext"/> for <see cref="ConsoleRouting"/>.
    /// </summary>
    public sealed class ConsoleRoutingContext : TerminalRoutingContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        public ConsoleRoutingContext(TerminalStartContext startContext, CancellationToken cancellationToken) : base(startContext, cancellationToken)
        {
        }
    }
}