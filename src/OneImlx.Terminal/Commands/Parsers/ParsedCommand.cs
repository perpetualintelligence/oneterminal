//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.Generic;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a parsed command from a command request.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance.
    /// </remarks>
    /// <param name="command">The command.</param>
    /// <param name="hierarchy">The command hierarchy.</param>
    public sealed class ParsedCommand(Command command, IEnumerable<CommandDescriptor>? hierarchy = null)
    {
        /// <summary>
        /// The parsed raw command.
        /// </summary>
        public Command Command { get; } = command ?? throw new System.ArgumentNullException(nameof(command));

        /// <summary>
        /// The parsed <see cref="Command"/> hierarchy.
        /// </summary>
        public IEnumerable<CommandDescriptor>? Hierarchy { get; } = hierarchy;
    }
}