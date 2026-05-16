//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction of a command custom property attribute.
    /// </summary>
    public interface ICommandCustomPropertyAttribute
    {
        /// <summary>
        /// The property key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The property value.
        /// </summary>
        public object Value { get; }
    }
}