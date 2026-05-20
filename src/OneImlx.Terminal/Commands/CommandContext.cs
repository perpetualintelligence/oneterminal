//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The generic command router context.
    /// </summary>
    /// <remarks>
    /// The command string.
    /// </remarks>
    public sealed class CommandContext : ICommandContext
    {
        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; }

        /// <summary>
        /// The terminal request.
        /// </summary>
        public CommandRequest Request { get; }

        /// <summary>
        /// The terminal router context.
        /// </summary>
        public TerminalRouterContext RouterContext { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="CommandContext"/>.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        internal CommandContext(
            CommandRequest request,
            TerminalRouterContext context,
            Dictionary<string, object> properties)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            RouterContext = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}