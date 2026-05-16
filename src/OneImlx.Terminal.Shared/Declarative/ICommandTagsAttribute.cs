//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction of a command tags attribute.
    /// </summary>
    public interface ICommandTagsAttribute
    {
        /// <summary>
        /// The command tags.
        /// </summary>
        public TagIdCollection Tags { get; }
    }
}