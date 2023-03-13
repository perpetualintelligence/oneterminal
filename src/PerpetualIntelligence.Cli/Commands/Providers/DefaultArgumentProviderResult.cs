﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The argument default provider result.
    /// </summary>
    public class DefaultArgumentProviderResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultArgumentDescriptor">The default argument descriptor.</param>
        public DefaultArgumentProviderResult(OptionDescriptor defaultArgumentDescriptor)
        {
            DefaultArgumentDescriptor = defaultArgumentDescriptor ?? throw new System.ArgumentNullException(nameof(defaultArgumentDescriptor));
        }

        /// <summary>
        /// The default argument descriptor.
        /// </summary>
        public OptionDescriptor DefaultArgumentDescriptor { get; }
    }
}