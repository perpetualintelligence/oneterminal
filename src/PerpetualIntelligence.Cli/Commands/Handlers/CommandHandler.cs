﻿/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The <c>cli</c> generic command handler.
    /// </summary>
    public class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(CliOptions options, ILogger<CommandHandler> logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual async Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            CommandHandlerResult result = new();

            // Find the checker
            OneImlxTryResult<ICommandChecker> commandChecker = await TryFindCheckerAsync(context);
            if (commandChecker.IsError)
            {
                result.SyncError(commandChecker);
                return result;
            }

            // Check the command, result will not be null here we already checked it in TryFindCheckerAsync.
            var checkerResult = await commandChecker.Result!.CheckAsync(new CommandCheckerContext(context.CommandIdentity, context.Command));
            if (checkerResult.IsError)
            {
                result.SyncError(checkerResult);
                return result;
            }

            // Find the runner
            OneImlxTryResult<ICommandRunner> commandRunner = await TryFindRunnerAsync(context);
            if (commandRunner.IsError)
            {
                result.SyncError(commandRunner);
                return result;
            }

            // Run the command, result will not be null here we already checked it in TryFindRunnerAsync.
            CommandRunnerResult runnerResult = await commandRunner.Result!.RunAsync(new CommandRunnerContext(context.Command));
            if (runnerResult.IsError)
            {
                result.SyncError(runnerResult);
                return result;
            }

            // Return the result to process it further.
            return result;
        }

        private Task<OneImlxTryResult<ICommandChecker>> TryFindCheckerAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.CommandIdentity.Checker == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            // Not added to service collection
            object? checkerObj = context.Services.GetService(context.CommandIdentity.Checker);
            if (checkerObj == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command checker. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, checker.GetType().FullName);
            return Task.FromResult<OneImlxTryResult<ICommandChecker>>(new(checker));
        }

        private Task<OneImlxTryResult<ICommandRunner>> TryFindRunnerAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.CommandIdentity.Runner == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            // Not added to service collection
            object? runnerObj = context.Services.GetService(context.CommandIdentity.Runner);
            if (runnerObj == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            // Invalid runner configured
            if (runnerObj is not ICommandRunner runner)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not valid. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command runner. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, runner.GetType().FullName);
            return Task.FromResult<OneImlxTryResult<ICommandRunner>>(new(runner));
        }

        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
    }
}