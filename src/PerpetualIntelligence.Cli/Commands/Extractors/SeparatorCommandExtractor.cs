﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The separator based command extractor.
    /// </summary>
    /// <seealso cref="ExtractorOptions.Separator"/>
    public class SeparatorCommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandStore">The command descriptor store.</param>
        /// <param name="argumentExtractor">The argument extractor.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="argumentDefaultValueProvider">The optional argument default value provider.</param>
        public SeparatorCommandExtractor(ICommandIdentityStore commandStore, IArgumentExtractor argumentExtractor, CliOptions options, ILogger<SeparatorCommandExtractor> logger, IArgumentDefaultValueProvider? argumentDefaultValueProvider = null)
        {
            this.commandStore = commandStore ?? throw new ArgumentNullException(nameof(commandStore));
            this.argumentExtractor = argumentExtractor ?? throw new ArgumentNullException(nameof(argumentExtractor));
            this.argumentDefaultValueProvider = argumentDefaultValueProvider;
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            // Ensure that extractor options are compatible.
            EnsureOptionsCompatibility();

            // Find the command identify by prefix
            CommandDescriptor commandDescriptor = await MatchByPrefixAsync(context.CommandString);

            // Extract the arguments. Arguments are optional for commands.
            Arguments? arguments = null;
            int prefixEndIndex = context.CommandString.IndexOf(commandDescriptor.Prefix, StringComparison.Ordinal);
            string argString = context.CommandString.Remove(prefixEndIndex, commandDescriptor.Prefix.Length);
            if (!string.IsNullOrWhiteSpace(argString))
            {
                // Make sure there is a separator between the command prefix and arguments
                if (!argString.StartsWith(options.Extractor.Separator, StringComparison.Ordinal))
                {
                    throw new ErrorException(Errors.InvalidCommand, "The command separator is missing. command_string={0}", context.CommandString);
                }

                arguments = await ExtractArgumentsOrThrowAsync(commandDescriptor, argString);
            }



            // OK, return the extracted command object.
            Command command = new(commandDescriptor)
            {
                Arguments = arguments
            };

            return new CommandExtractorResult(command, commandDescriptor);
        }

        private void EnsureOptionsCompatibility()
        {
            // Separator can be whitespace
            if (options.Extractor.Separator == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator is null or not configured.", options.Extractor.Separator);
            }

            // Command separator and argument separator cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentSeparator, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument separator cannot be same. separator={0}", options.Extractor.Separator);
            }

            // Command separator and argument prefix cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument prefix cannot be same. separator={0}", options.Extractor.Separator);
            }

            // Argument separator cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentSeparator))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument separator cannot be null or whitespace.", options.Extractor.ArgumentSeparator);
            }

            // Argument separator and argument prefix cannot be same
            if (options.Extractor.ArgumentSeparator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument separator and argument prefix cannot be same. separator={0}", options.Extractor.ArgumentSeparator);
            }

            // Argument prefix cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentPrefix))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument prefix cannot be null or whitespace.", options.Extractor.ArgumentPrefix);
            }

            // Argument default value provider is missing
            if (options.Extractor.ArgumentDefaultValue.GetValueOrDefault() && argumentDefaultValueProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection.");
            }
        }

        private async Task<Arguments> ExtractArgumentsOrThrowAsync(CommandDescriptor commandDescriptor, string argString)
        {
            Arguments arguments = new();

            string argSplit = string.Concat(options.Extractor.Separator, options.Extractor.ArgumentPrefix);
            string[] args = argString.Split(new string[] { argSplit }, StringSplitOptions.RemoveEmptyEntries);
            List<Error> errors = new();
            foreach (string arg in args)
            {
                // Restore the arg prefix for the extractor
                string prefixArg = string.Concat(options.Extractor.ArgumentPrefix, arg);

                // We capture all the argument extraction errors
                TryResultError<ArgumentExtractorResult> tryResult = await Formatter.EnsureResultAsync<ArgumentExtractorContext, ArgumentExtractorResult>(argumentExtractor.ExtractAsync, new ArgumentExtractorContext(prefixArg, commandDescriptor));
                if (tryResult.Error != null)
                {
                    errors.Add(tryResult.Error);
                }
                else
                {
                    // Protect for bad custom implementation.
                    if (tryResult.Result == null)
                    {
                        errors.Add(new Error(Errors.InvalidArgument, "The argument string did not return an error or extract the argument. argument_string={0}", arg));
                    }
                    else
                    {
                        arguments.Add(tryResult.Result.Argument);
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new MultiErrorException(errors.ToArray());
            }

            // Check for default arguments if enabled. Default values are added at the end if there is no explicit input
            if (options.Extractor.ArgumentDefaultValue.GetValueOrDefault())
            {
                // TODO: Performance ArgumentDescriptor collection should be an ordered dictionary
                ArgumentDefaultValueProviderResult defaultResult = await argumentDefaultValueProvider.ProvideAsync(new ArgumentDefaultValueProviderContext(commandDescriptor));
                foreach(ArgumentDescriptor argumentDescriptor in defaultResult.DefaultValueArgumentDescriptors)
                {
                }

                if(defaultResult.DefaultValueArgumentDescriptors.Count > 0)
                {
                    
                }

            }


            return arguments;
        }

        /// <summary>
        /// Matches the command string and finds the <see cref="CommandDescriptor"/>.
        /// </summary>
        /// <param name="commandString">The command string to match.</param>
        /// <returns></returns>
        /// <exception cref="ErrorException">If command string did not match any command descriptor.</exception>
        private async Task<CommandDescriptor> MatchByPrefixAsync(string commandString)
        {
            string prefix = commandString;

            // Find the prefix, prefix is the entire string till first argument.
            int idx = commandString.IndexOf(options.Extractor.ArgumentPrefix, StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                prefix = commandString.Substring(0, idx);
            }

            // Make sure we trim the command separator. prefix from the previous step will most likely have a command
            // separator pi auth login -key=value
            prefix = prefix.TrimEnd(options.Extractor.Separator);

            var result = await commandStore.TryFindByPrefixAsync(prefix);
            if (result.IsError)
            {
                throw new ErrorException(result.FirstError);
            }

            // Make sure we have the result to proceed. Protect bad custom implementations.
            if (result.Result == null)
            {
                throw new ErrorException(Errors.InvalidCommand, "The command string did not return an error or match the command prefix. command_string={0}", commandString);
            }

            return result.Result;
        }

        private readonly IArgumentDefaultValueProvider? argumentDefaultValueProvider;
        private readonly IArgumentExtractor argumentExtractor;
        private readonly ICommandIdentityStore commandStore;
        private readonly ILogger<SeparatorCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
