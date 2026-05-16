//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// An abstraction of an entity with a key as an identifier.
    /// </summary>
    public interface IKeyAsId
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public string Id { get; }
    }
}