//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// The generic command router context.
    /// </summary>
    /// <remarks>
    /// The command string.
    /// </remarks>
    /// <remarks>
    /// Initialize a new instance of <see cref="CommandContext"/>.
    /// </remarks>
    /// <param name="properties">The additional router properties.</param>
    public class CommandContext(Dictionary<string, object> properties)
    {
        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; } = properties ?? throw new ArgumentNullException(nameof(properties));
    }
}