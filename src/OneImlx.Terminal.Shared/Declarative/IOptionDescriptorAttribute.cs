//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction for an option descriptor declaration.
    /// </summary>
    public interface IOptionDescriptorAttribute
    {
        /// <summary>
        /// The option data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The option description.
        /// </summary>
        /// <remarks>The option id is unique across all commands.</remarks>
        public string Description { get; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// The option flags.
        /// </summary>
        /// <seealso cref="BehaviorFlags"/>
        public int Flags { get; }
    }
}