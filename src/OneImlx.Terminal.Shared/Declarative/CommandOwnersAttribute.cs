//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// Declares a command owners. A command owner is a group or a root.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance.
    /// </remarks>
    /// <param name="owners">The command owner identifiers.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandOwnersAttribute(params string[] owners) : Attribute, ICommandOwnersAttribute
    {
        /// <summary>
        /// The command owner identifiers.
        /// </summary>
        public OwnerIdCollection Owners { get; } = [.. owners];
    }
}