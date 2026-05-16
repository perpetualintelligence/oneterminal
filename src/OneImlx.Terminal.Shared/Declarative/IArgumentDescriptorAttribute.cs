//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction for an argument descriptor declaration.
    /// </summary>
    public interface IArgumentDescriptorAttribute
    {
        /// <summary>
        /// The argument data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The argument description.
        /// </summary>
        /// <remarks>The argument id is unique across all commands.</remarks>
        public string Description { get; }

        /// <summary>
        /// The argument order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// The argument flags.
        /// </summary>
        public ArgumentFlags Flags { get; }
    }
}