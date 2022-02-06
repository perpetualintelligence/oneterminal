﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The default argument provider.
    /// </summary>
    /// <seealso cref="CommandDescriptor.DefaultArgument"/>
    public class DefaultArgumentProvider : IDefaultArgumentProvider
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultArgumentProvider(CliOptions options, ILogger<DefaultArgumentProvider> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Provides default values for all the command arguments.
        /// </summary>
        /// <param name="context">The argument default value provider context.</param>
        /// <returns>The <see cref="DefaultArgumentValueProviderResult"/> instance that contains the default values.</returns>
        /// <exception cref="ErrorException"></exception>
        public Task<DefaultArgumentProviderResult> ProvideAsync(DefaultArgumentProviderContext context)
        {
            if (context.CommandDescriptor.DefaultArgument == null)
            {
                throw new ErrorException(Errors.UnsupportedArgument, "The command does not support default argument. command_id={0} command_name={1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            return Task.FromResult(new DefaultArgumentProviderResult(context.CommandDescriptor.GetArgumentDescriptor(context.CommandDescriptor.DefaultArgument)));
        }

        private readonly ILogger<DefaultArgumentProvider> logger;
        private readonly CliOptions options;
    }
}