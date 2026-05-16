//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstract command owner attribute.
    /// </summary>
    public interface ICommandOwnersAttribute
    {
        /// <summary>
        /// The command owner identifiers.
        /// </summary>
        public OwnerIdCollection Owners { get; }
    }
}