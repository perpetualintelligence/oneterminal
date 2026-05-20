//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// <see cref="ICommandContext"/> extension methods for working with parsed commands, arguments, and options.
    /// </summary>
    public static class ICommandContextExtensions
    {
        /// <summary>
        /// Gets the current parsed command from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The available parsed command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public static ParsedCommand GetParsedCommand(this ICommandContext commandContext)
        {
            commandContext.Properties.TryGetValue(TerminalIdentifiers.ParsedCommand, out object? parsedCommand);
            if (parsedCommand is not ParsedCommand currentCommand || parsedCommand == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The parsed command is missing in the context.");
            }
            return currentCommand;
        }

        /// <summary>
        /// Gets the current executing command from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The available command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public static Command GetCommand(this ICommandContext commandContext)
        {
            ParsedCommand parsedCommand = commandContext.GetParsedCommand();
            return parsedCommand.Command;
        }

        /// <summary>
        /// Gets the current command result from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The available command result.</returns>
        /// <exception cref="TerminalException">Thrown when the command result is not available.</exception>
        public static CommandResult GetCommandResult(this ICommandContext commandContext)
        {
            commandContext.Properties.TryGetValue(TerminalIdentifiers.CommandResult, out object? result);
            if (result is not CommandResult commandResult)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command result is missing in the context.");
            }
            return commandResult;
        }

        /// <summary>
        /// Determines whether the command context contains a command result.
        /// </summary>
        /// <param name="commandContext">The command context to inspect for a command result. Cannot be null.</param>
        /// <param name="commandResult">The command result if available; otherwise, null.</param>
        /// <returns>True if a command result is present; otherwise, false.</returns>
        public static bool TryGetCommandResult(this ICommandContext commandContext, out CommandResult? commandResult)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.CommandResult, out object? result);
            commandResult = (CommandResult)result;
            return found;
        }

        /// <summary>
        /// Determines whether the command context contains a parsed command.
        /// </summary>
        /// <param name="commandContext">The command context to inspect for a parsed command. Cannot be null.</param>
        /// <param name="parsedCommand">The parsed command if available; otherwise, null.</param>
        /// <returns>True if a parsed command is present; otherwise, false.</returns>
        public static bool TryGetParsedCommand(this ICommandContext commandContext, out ParsedCommand? parsedCommand)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.ParsedCommand, out object? parsedObject);
            parsedCommand = (ParsedCommand)parsedObject;
            return found;
        }

        /// <summary>
        /// Sets the current parsed command in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="parsedCommand">The parsed command.</param>
        public static void SetParsedCommand(this ICommandContext commandContext, ParsedCommand parsedCommand)
        {
            commandContext.Properties[TerminalIdentifiers.ParsedCommand] = parsedCommand;
        }

        /// <summary>
        /// Sets the current command result in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="commandResult">The command result.</param>
        public static void SetCommandResult(this ICommandContext commandContext, CommandResult commandResult)
        {
            commandContext.Properties[TerminalIdentifiers.CommandResult] = commandResult;
        }

        /// <summary>
        /// Gets the required argument value by argument id.
        /// </summary>
        public static TValue GetRequiredArgumentValue<TValue>(this ICommandContext commandContext, string argId)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredArgumentValue<TValue>(argId);
        }

        /// <summary>
        /// Gets the required argument value by argument index.
        /// </summary>
        public static TValue GetRequiredArgumentValue<TValue>(this ICommandContext commandContext, int argIndex)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredArgumentValue<TValue>(argIndex);
        }

        /// <summary>
        /// Tries to get the argument value by argument id.
        /// </summary>
        public static bool TryGetArgumentValue<TValue>(this ICommandContext commandContext, string argId, out TValue? value)
        {
            Command command = GetCommand(commandContext);
            return command.TryGetArgumentValue<TValue>(argId, out value);
        }

        /// <summary>
        /// Gets the required option value by option id or alias.
        /// </summary>
        public static TValue GetRequiredOptionValue<TValue>(this ICommandContext commandContext, string idOrAlias)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredOptionValue<TValue>(idOrAlias);
        }

        /// <summary>
        /// Tries to get the option value by option id or alias.
        /// </summary>
        public static bool TryGetOptionValue<TValue>(this ICommandContext commandContext, string idOrAlias, out TValue? value)
        {
            Command command = GetCommand(commandContext);
            return command.TryGetOptionValue<TValue>(idOrAlias, out value);
        }
    }
}