﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Extensions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="IErrorHandler"/> to handle an <see cref="Error"/>.
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ErrorHandler(TerminalOptions options, ILogger<ExceptionHandler> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Publish the <see cref="Error"/> asynchronously
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task HandleAsync(ErrorHandlerContext context)
        {
            logger.FormatAndLog(LogLevel.Error, options.Logging, context.Error.ErrorDescription ?? context.Error.ErrorCode, context.Error.Args ?? Array.Empty<object?>());
            return Task.CompletedTask;
        }

        private ILogger<ExceptionHandler> logger;
        private TerminalOptions options;
    }
}
