//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// An abstraction of a context provider for command execution.
    /// </summary>
    public interface ICommandContextProvider
    {
        /// <summary>
        /// Gets the command execution context.
        /// </summary>
        /// <returns>The command execution context.</returns>
        CommandContext GetContext();
    }
}