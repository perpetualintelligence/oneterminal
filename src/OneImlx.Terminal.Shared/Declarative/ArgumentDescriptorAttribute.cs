//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// Declares an argument for a command.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance.
    /// </remarks>
    /// <param name="order">The argument order.</param>
    /// <param name="id">The argument id.</param>
    /// <param name="dataType">The argument data type.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="flags">The argument flags.</param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class ArgumentDescriptorAttribute(int order, string id, string dataType, string description, ArgumentFlags flags) : Attribute, IArgumentDescriptorAttribute
    {
        /// <summary>
        /// The argument data type.
        /// </summary>
        public string DataType { get; } = dataType;

        /// <summary>
        /// The argument description.
        /// </summary>
        /// <remarks>The argument id is unique across all commands.</remarks>
        public string Description { get; } = description;

        /// <summary>
        /// The argument order.
        /// </summary>
        public int Order { get; } = order;

        /// <summary>
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique within a command.</remarks>
        public string Id { get; } = id;

        /// <summary>
        /// The argument flags.
        /// </summary>
        public ArgumentFlags Flags { get; } = flags;
    }
}