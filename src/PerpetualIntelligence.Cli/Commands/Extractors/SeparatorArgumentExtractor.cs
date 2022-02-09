﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The separator based argument extractor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syntax for a separator based argument is <c>{arg}={value}</c> for e.g. <c>name=oneimlx</c>. The syntax has 4 parts:
    /// <list type="number">
    /// <item>
    /// <description><c>-</c> is an argument prefix. You can configure it via <see cref="ExtractorOptions.ArgumentPrefix"/></description>
    /// </item>
    /// <item>
    /// <description>{arg} is an argument id. For e.g. <c>name</c></description>
    /// </item>
    /// <item>
    /// <description><c>=</c> is an argument separator. You can configure it via <see cref="ExtractorOptions.ArgumentSeparator"/></description>
    /// </item>
    /// <item>
    /// <description>{value} is an argument value. For e.g. <c>oneimlx</c></description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="ExtractorOptions.ArgumentPrefix"/>
    /// <seealso cref="ExtractorOptions.ArgumentSeparator"/>
    [WriteDocumentation]
    public class SeparatorArgumentExtractor : IArgumentExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public SeparatorArgumentExtractor(CliOptions options, ILogger<SeparatorArgumentExtractor> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<ArgumentExtractorResult> ExtractAsync(ArgumentExtractorContext context)
        {
            string argumentString = context.ArgumentString;

            // Extract the argument and value by default or custom patterns.
            string argIdValueRegex = context.IsAlias ? ArgumentAliasValueRegexPattern : ArgumentIdValueRegexPattern;
            Match argIdValueMatch = Regex.Match(argumentString, argIdValueRegex);
            string? argIdOrAlias = null;
            string? argValue = null;
            string? argPrefix = null;
            bool matched = false;
            if (argIdValueMatch.Success)
            {
                argPrefix = argIdValueMatch.Groups[1].Value;
                argIdOrAlias = argIdValueMatch.Groups[2].Value;
                argValue = argIdValueMatch.Groups[3].Value;

                // Check if we need to extract the value with_in a token
                if (options.Extractor.ArgumentValueWithIn != null)
                {
                    Match argValueWithInMatch = Regex.Match(argValue, ArgumentValueWithinRegexPattern);
                    if (argValueWithInMatch.Success)
                    {
                        // (.*) captures the group so we can get the string with_in
                        argValue = argValueWithInMatch.Groups[1].Value;
                    }
                }

                matched = true;
            }
            else
            {
                string argIdOnlyRegex = context.IsAlias ? ArgumentAliasNoValueRegexPattern : ArgumentIdNoValueRegexPattern;
                Match argIdOnlyMatch = Regex.Match(argumentString, argIdOnlyRegex);
                if (argIdOnlyMatch.Success)
                {
                    argPrefix = argIdOnlyMatch.Groups[1].Value;
                    argIdOrAlias = argIdOnlyMatch.Groups[2].Value;
                    matched = true;
                }
            }

            // Not matched
            if (!matched)
            {
                throw new ErrorException(Errors.InvalidArgument, "The argument string is not valid. argument_string={0}", argumentString);
            }

            // For error handling
            string prefixArgValue = $"{argPrefix}{argIdOrAlias}{options.Extractor.ArgumentSeparator}{argValue}";
            if (argIdOrAlias == null || string.IsNullOrWhiteSpace(argIdOrAlias))
            {
                throw new ErrorException(Errors.InvalidArgument, "The argument identifier is null or empty. argument_string={0}", prefixArgValue);
            }

            // Find by alias only if configured.
            bool argAndAliasPrefixSame = options.Extractor.ArgumentPrefix.Equals(options.Extractor.ArgumentAliasPrefix, StringComparison.Ordinal);
            bool aliasEnabled = options.Extractor.ArgumentAlias.GetValueOrDefault();

            // Compatibility check: If ArgumentAlias is not enabled and the prefix is used to identify by alias then
            // this is an error. If ArgumentPrefix and ArgumentAliasPrefix are same then bypass the compatiblity check.
            // find it.
            if (!argAndAliasPrefixSame && !aliasEnabled)
            {
                if (options.Extractor.ArgumentAliasPrefix.Equals(argPrefix, StringComparison.Ordinal))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument extraction by alias prefix is not configured. argument_string={0}", prefixArgValue);
                }
            }

            // Now find the argument by id or alias.
            ArgumentDescriptor argDescriptor;
            if (aliasEnabled && argAndAliasPrefixSame)
            {
                // If alias is not enabled then we should not be here as we can only find by id. If alias is enabled but
                // the prefix are same then there is no way for us to find the arg precisely, so we first find by id
                // then by it alias or throw.
                argDescriptor = context.CommandDescriptor.GetArgumentDescriptorByIdOrAlias(argIdOrAlias);
            }
            else
            {
                // If we are here then we can by precisely find by id or alias as the prefix are different. But we need
                // to now see whether we should find by alias.
                bool findByAlias = context.IsAlias && aliasEnabled;
                argDescriptor = context.CommandDescriptor.GetArgumentDescriptor(argIdOrAlias, findByAlias);
            }

            // Key only (treat it as a boolean) value=true
            if (nameof(Boolean).Equals(argDescriptor.CustomDataType))
            {
                // The value will not be white space because we have already removed all the separators.
                string value = (argValue == null || argValue.Equals(string.Empty)) ? true.ToString() : argValue;
                return Task.FromResult(new ArgumentExtractorResult(new Argument(argDescriptor, value)));
            }
            else
            {
                if (argValue == null)
                {
                    throw new ErrorException(Errors.InvalidArgument, "The argument value is missing. argument_string={0}", prefixArgValue);
                }

                return Task.FromResult(new ArgumentExtractorResult(new Argument(argDescriptor, argValue)));
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the argument alias with no value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Lets assume that you have configured <see cref="ExtractorOptions.Separator"/> as a single whitespace string
        /// ' ', and <see cref="ExtractorOptions.ArgumentAliasPrefix"/> as a single dash character string '-'.
        /// </para>
        /// <para>
        /// The default implementation for <see cref="ArgumentAliasNoValueRegexPattern"/> will match using the following criteria:
        /// </para>
        /// <list type="number">
        /// <item>
        /// <term>'^ *(-)+(.+)$'</term>
        /// <description>Default example REGEX pattern</description>
        /// </item>
        /// <item>
        /// <term>'^'</term>
        /// <description>Matches the beginning of the string</description>
        /// </item>
        /// <item>
        /// <term>' *'</term>
        /// <description>Matches 0 or more <see cref="ExtractorOptions.Separator"/></description>
        /// </item>
        /// <item>
        /// <term>'(-)+'</term>
        /// <description>Create a new capture group and matches 1 or more <see cref="ExtractorOptions.ArgumentAliasPrefix"/></description>
        /// </item>
        /// <item>
        /// <term>'(.+?)'</term>
        /// <description>Create a new capture group and matches characters (1 or more) except line breaks</description>
        /// </item>
        /// <item>
        /// <term>'$'</term>
        /// <description>Matches the end of the string</description>
        /// </item>
        /// </list>
        /// </remarks>
        protected virtual string ArgumentAliasNoValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.ArgumentAliasPrefix})+(.+?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the argument id and value using <see cref="ExtractorOptions.ArgumentPrefix"/>.
        /// </summary>
        protected virtual string ArgumentAliasValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.ArgumentAliasPrefix})+(.+?){options.Extractor.ArgumentSeparator}+(.*?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the argument alias and value using <see cref="ExtractorOptions.ArgumentAliasPrefix"/>.
        /// </summary>
        protected virtual string ArgumentIdNoValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.ArgumentPrefix})+(.+?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the argument alias and value using <see cref="ExtractorOptions.ArgumentAliasPrefix"/>.
        /// </summary>
        protected virtual string ArgumentIdValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.ArgumentPrefix})+(.+?){options.Extractor.ArgumentSeparator}+(.*?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the argument alias and value using <see cref="ExtractorOptions.ArgumentValueWithIn"/>.
        /// </summary>
        protected virtual string ArgumentValueWithinRegexPattern
        {
            get
            {
                return $"^{options.Extractor.ArgumentValueWithIn}(.*){options.Extractor.ArgumentValueWithIn}$";
            }
        }

        private readonly ILogger<SeparatorArgumentExtractor> logger;
        private readonly CliOptions options;
    }
}
