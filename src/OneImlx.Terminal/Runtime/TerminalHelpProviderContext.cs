//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalHelpProvider"/> context.
    /// </summary>
    /// <remarks>
    /// Initialize a new instance.
    /// </remarks>
    /// <param name="command">The command descriptor.</param>
    public sealed class TerminalHelpProviderContext(Command command)
    {
        /// <summary>
        /// The command descriptor.
        /// </summary>
        public Command Command { get; set; } = command;
    }
}