﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> command builder.
    /// </summary>
    public interface ICommandBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Build a new <see cref="CommandDescriptor"/> and add it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        ICliBuilder Add();
    }
}