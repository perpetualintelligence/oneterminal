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
        /// Gets the parsed command from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The parsed command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available in the context.</exception>
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
        /// Gets the command request from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The command request.</returns>
        /// <exception cref="TerminalException">Thrown when the command request is not available in the context.</exception>
        public static CommandRequest GetCommandRequest(this ICommandContext commandContext)
        {
            commandContext.Properties.TryGetValue(TerminalIdentifiers.CommandRequest, out object? result);
            if (result is not CommandRequest commandRequest)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command request is missing in the context.");
            }
            return commandRequest;
        }

        /// <summary>
        /// Attempts to get the command request from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="commandRequest">The command request if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the command request was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetCommandRequest(this ICommandContext commandContext, out CommandRequest? commandRequest)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.CommandRequest, out object? result);
            commandRequest = result as CommandRequest;
            return found;
        }

        /// <summary>
        /// Sets the command request in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="commandRequest">The command request to set.</param>
        public static void SetCommandRequest(this ICommandContext commandContext, CommandRequest commandRequest)
        {
            commandContext.Properties[TerminalIdentifiers.CommandRequest] = commandRequest;
        }

        /// <summary>
        /// Gets the executing command from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The executing command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available in the context.</exception>
        public static Command GetCommand(this ICommandContext commandContext)
        {
            ParsedCommand parsedCommand = commandContext.GetParsedCommand();
            return parsedCommand.Command;
        }

        /// <summary>
        /// Gets the command result from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The command result.</returns>
        /// <exception cref="TerminalException">Thrown when the command result is not available in the context.</exception>
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
        /// Attempts to get the command result from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="commandResult">The command result if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the command result was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetCommandResult(this ICommandContext commandContext, out CommandResult? commandResult)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.CommandResult, out object? result);
            commandResult = result as CommandResult;
            return found;
        }

        /// <summary>
        /// Gets the router context from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <returns>The router context.</returns>
        /// <exception cref="TerminalException">Thrown when the router context is not available in the context.</exception>
        public static TerminalRouterContext GetRouterContext(this ICommandContext commandContext)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.RouterContext, out object? result);
            if (result is not TerminalRouterContext routerContext)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The router context is missing in the context.");
            }
            return routerContext;
        }

        /// <summary>
        /// Attempts to get the <see cref="TerminalRouterContext"/> from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="routerContext">The router context if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the router context was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetRouterContext(this ICommandContext commandContext, out TerminalRouterContext? routerContext)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.RouterContext, out object? result);
            routerContext = result as TerminalRouterContext;
            return found;
        }

        /// <summary>
        /// Sets the router context in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="routerContext">The router context to set.</param>
        public static void SetRouterContext(this ICommandContext commandContext, TerminalRouterContext routerContext)
        {
            commandContext.Properties[TerminalIdentifiers.RouterContext] = routerContext;
        }

        /// <summary>
        /// Attempts to get the parsed command from the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="parsedCommand">The parsed command if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the parsed command was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetParsedCommand(this ICommandContext commandContext, out ParsedCommand? parsedCommand)
        {
            bool found = commandContext.Properties.TryGetValue(TerminalIdentifiers.ParsedCommand, out object? parsedObject);
            parsedCommand = parsedObject as ParsedCommand;
            return found;
        }

        /// <summary>
        /// Sets the parsed command in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="parsedCommand">The parsed command to set.</param>
        public static void SetParsedCommand(this ICommandContext commandContext, ParsedCommand parsedCommand)
        {
            commandContext.Properties[TerminalIdentifiers.ParsedCommand] = parsedCommand;
        }

        /// <summary>
        /// Sets the command result in the context.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="commandResult">The command result to set.</param>
        public static void SetCommandResult(this ICommandContext commandContext, CommandResult commandResult)
        {
            commandContext.Properties[TerminalIdentifiers.CommandResult] = commandResult;
        }

        /// <summary>
        /// Gets the required argument value by argument id.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="argId">The argument identifier.</param>
        /// <returns>The argument value.</returns>
        /// <exception cref="TerminalException">Thrown when the argument is not found or the value cannot be cast to <typeparamref name="TValue"/>.</exception>
        public static TValue GetRequiredArgumentValue<TValue>(this ICommandContext commandContext, string argId)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredArgumentValue<TValue>(argId);
        }

        /// <summary>
        /// Gets the required argument value by argument index.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="argIndex">The zero-based argument index.</param>
        /// <returns>The argument value.</returns>
        /// <exception cref="TerminalException">Thrown when the argument is not found or the value cannot be cast to <typeparamref name="TValue"/>.</exception>
        public static TValue GetRequiredArgumentValue<TValue>(this ICommandContext commandContext, int argIndex)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredArgumentValue<TValue>(argIndex);
        }

        /// <summary>
        /// Attempts to get the argument value by argument id.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="argId">The argument identifier.</param>
        /// <param name="value">The argument value if found; otherwise, the default value of <typeparamref name="TValue"/>.</param>
        /// <returns><see langword="true"/> if the argument was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetArgumentValue<TValue>(this ICommandContext commandContext, string argId, out TValue? value)
        {
            Command command = GetCommand(commandContext);
            return command.TryGetArgumentValue<TValue>(argId, out value);
        }

        /// <summary>
        /// Gets the required option value by option id or alias.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="idOrAlias">The option identifier or alias.</param>
        /// <returns>The option value.</returns>
        /// <exception cref="TerminalException">Thrown when the option is not found or the value cannot be cast to <typeparamref name="TValue"/>.</exception>
        public static TValue GetRequiredOptionValue<TValue>(this ICommandContext commandContext, string idOrAlias)
        {
            Command command = GetCommand(commandContext);
            return command.GetRequiredOptionValue<TValue>(idOrAlias);
        }

        /// <summary>
        /// Attempts to get the option value by option id or alias.
        /// </summary>
        /// <param name="commandContext">The command context.</param>
        /// <param name="idOrAlias">The option identifier or alias.</param>
        /// <param name="value">The option value if found; otherwise, the default value of <typeparamref name="TValue"/>.</param>
        /// <returns><see langword="true"/> if the option was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetOptionValue<TValue>(this ICommandContext commandContext, string idOrAlias, out TValue? value)
        {
            Command command = GetCommand(commandContext);
            return command.TryGetOptionValue<TValue>(idOrAlias, out value);
        }
    }
}