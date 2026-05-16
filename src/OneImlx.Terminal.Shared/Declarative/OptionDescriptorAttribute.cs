//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// Declares an option for a command.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance.
    /// </remarks>
    /// <param name="id">The option id.</param>
    /// <param name="dataType">The option data type.</param>
    /// <param name="description">The option description.</param>
    /// <param name="flags">The option flags.</param>
    /// <param name="alias">The option alias.</param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class OptionDescriptorAttribute(string id, string dataType, string description, OptionFlags flags, string? alias = null) : Attribute, IOptionDescriptorAttribute
    {
        /// <summary>
        /// The option alias.
        /// </summary>
        /// <remarks>
        /// The option alias is unique within a command. Option alias supports the legacy apps that identified a
        /// command option with an id and an alias string. For modern console apps, we recommend using just an
        /// option identifier. The core data model is optimized to work with option id. In general, an app should
        /// not identify the same option with multiple strings.
        /// </remarks>
        public string? Alias { get; } = alias;

        /// <summary>
        /// The option data type.
        /// </summary>
        public string DataType { get; } = dataType;

        /// <summary>
        /// The option description.
        /// </summary>
        /// <remarks>The option id is unique across all commands.</remarks>
        public string Description { get; } = description;

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; } = id;

        /// <summary>
        /// The option flags.
        /// </summary>
        public OptionFlags Flags { get; } = flags;
    }
}