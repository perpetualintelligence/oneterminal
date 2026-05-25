//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Shared;
using System.Collections.Generic;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRouterContext"/> for <see cref="TerminalCustomRouter"/>.
    /// </summary>
    public abstract class TerminalCustomRouterContext : TerminalRouterContext
    {
        /// <inheritdoc/>
        protected TerminalCustomRouterContext(
            TerminalStartMode startMode,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null) : base(startMode, customProperties, arguments)
        {
        }
    }
}