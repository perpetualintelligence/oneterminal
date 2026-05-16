//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html
using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// An abstraction of an argument validation attribute.
    /// </summary>
    public interface IArgumentValidationAttribute
    {
        /// <summary>
        /// The argument identifier.
        /// </summary>
        public string ArgumentId { get; }

        /// <summary>
        /// The validation attribute parameters.
        /// </summary>
        public object[]? ValidationParams { get; }

        /// <summary>
        /// The option validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; }
    }
}