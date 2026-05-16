//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Runtime;
using System.Collections.Generic;

namespace OneImlx.Terminal.Server.Pulsar
{
    /// <summary>
    /// The <see cref="TerminalPulsarRouter"/> connection context.
    /// </summary>
    public sealed class TerminalPulsarRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        public TerminalPulsarRouterContext(
            TerminalStartMode startMode,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null)
            : base(startMode, customProperties, arguments)
        {
        }
    }
}