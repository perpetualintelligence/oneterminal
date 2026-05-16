//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares a command checker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class CommandCheckerAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="checker">The command checker.</param>
        public CommandCheckerAttribute(Type checker)
        {
            Checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        /// <summary>
        /// The command checker type.
        /// </summary>
        public Type Checker { get; }
    }
}