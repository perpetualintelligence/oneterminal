//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Identifies <see cref="CommandRunner{TResult}.RunCommandAsync(ICommandContext)"/> for commands in
    /// a <see cref="CommandTypes.CompositeGroup"/>.
    /// </summary>
    /// <remarks>
    /// Each <see cref="RunMethodDescriptor"/> maps to one unique command as its execution logic.
    /// </remarks>
    public sealed class RunMethodDescriptor : IKeyAsId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunMethodDescriptor"/> with specified command identifier and method name using reflection.
        /// </summary>
        /// <param name="id">The run method identifier.</param>
        /// <param name="methodName">The run method name.</param>
        public RunMethodDescriptor(string id, string methodName)
        {
            CommandId = id ?? throw new ArgumentNullException(nameof(id));
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunMethodDescriptor"/> with specified command identifier and the method info.
        /// </summary>
        /// <param name="id">The run method identifier.</param>
        /// <param name="methodInfo">The run method info.</param>
        public RunMethodDescriptor(string id, MethodInfo methodInfo)
        {
            CommandId = id ?? throw new ArgumentNullException(nameof(id));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            MethodName = methodInfo.Name;
        }

        /// <summary>
        /// Runs the command execution to the method using reflection.
        /// </summary>
        /// <remarks>
        /// THIS METHOD IS PART OF INTERNAL INFRASTRUCTURE AND IS NOT INTENDED FOR DIRECT USE BY APPLICATION CODE.
        /// </remarks>
        public async Task<TResult> RunAsync<TResult>(CommandRunner<TResult> commandRunner, ICommandContext context) where TResult : CommandRunnerResult
        {
            // Ensure command matches the passed context
            ParsedCommand parsedCommand = context.GetParsedCommand();
            if (parsedCommand.Command.Id != CommandId)
            {
                throw new TerminalException(TerminalErrors.InvalidCommand, "The method's command is invalid. command={0}", CommandId);
            }

            if (MethodInfo != null)
            {
                Task<TResult> resultTask = (Task<TResult>)MethodInfo.Invoke(commandRunner, [context]);
                await resultTask.ConfigureAwait(false);
                return resultTask.Result;
            }
            else if (MethodName != null)
            {
                MethodInfo methodInfo = commandRunner.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public) ?? throw new TerminalException(TerminalErrors.InvalidCommand, "No public run method found on the command. command={0}, name={1}", CommandId, MethodName);
                Task<TResult> resultTask = (Task<TResult>)methodInfo.Invoke(commandRunner, [context]);
                await resultTask.ConfigureAwait(false);
                return resultTask.Result;
            }
            else
            {
                throw new TerminalException(TerminalErrors.InvalidCommand, "The method name or method info is not registered. command={0}", CommandId);
            }
        }

        /// <summary>
        /// The command identifier.
        /// </summary>
        public string CommandId { get; }

        /// <summary>
        /// The method info.
        /// </summary>
        public MethodInfo? MethodInfo { get; }

        /// <summary>
        /// The method name.
        /// </summary>
        public string? MethodName { get; }
    }
}