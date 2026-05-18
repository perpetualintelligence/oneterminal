//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// An abstraction for the command execution context.
    /// </summary>
    public interface ICommandContext
    {
        /// <summary>
        /// The extracted license.
        /// </summary>
        public License? License { get; }

        /// <summary>
        /// The parsed command.
        /// </summary>
        public ParsedCommand? ParsedCommand { get; }

        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// The terminal request.
        /// </summary>
        public CommandRequest Request { get; }

        /// <summary>
        /// The result of the command execution.
        /// </summary>
        public CommandResult? Result { get; }

        /// <summary>
        /// The terminal router context.
        /// </summary>
        public TerminalRouterContext RouterContext { get; }

        /// <summary>
        /// Ensures the license is available.
        /// </summary>
        /// <returns>The available license.</returns>
        /// <exception cref="TerminalException">Thrown when the license is not available.</exception>
        public License EnsureLicense();

        /// <summary>
        /// Ensures the parsed command is available.
        /// </summary>
        /// <returns>The available parsed command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public ParsedCommand EnsureParsedCommand();

        /// <summary>
        /// Ensures the command is available.
        /// </summary>
        /// <returns>The available command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public Command EnsureCommand();

        /// <summary>
        /// Ensures the result is available.
        /// </summary>
        /// <returns>The available result.</returns>
        /// <exception cref="TerminalException">Thrown when the result is not available.</exception>
        public CommandResult EnsureResult();
    }
}