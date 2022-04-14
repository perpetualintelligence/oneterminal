﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner.
    /// </summary>
    public abstract class CommandRunner : ICommandRunner
    {
        /// <summary>
        /// The configuration options.
        /// </summary>
        public CliOptions Options { get; }

        /// <inheritdoc/>
        public abstract Task<CommandRunnerResult> RunAsync(CommandRunnerContext context);

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        protected CommandRunner(CliOptions options, ILogger logger)
        {
            Options = options;
            Logger = logger;
        }

        /// <summary>
        /// The command runner logger.
        /// </summary>
        protected ILogger Logger { get; }
    }
}
