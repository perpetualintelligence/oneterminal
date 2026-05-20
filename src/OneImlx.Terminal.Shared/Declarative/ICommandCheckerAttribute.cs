//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction of a command checker attribute.
    /// </summary>
    public interface ICommandCheckerAttribute
    {
        /// <summary>
        /// The command checker type.
        /// </summary>
        Type Checker { get; }
    }
}