//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.Generic;

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// An abstraction for the command execution context.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; }

        /// <summary>
        /// The terminal request.
        /// </summary>
        public CommandRequest Request { get; }

        /// <summary>
        /// The terminal router context.
        /// </summary>
        public TerminalRouterContext RouterContext { get; }
    }
}