//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// Declares an argument validation attribute.
    /// </summary>
    /// <remarks>
    /// Initialize a new instance.
    /// </remarks>
    /// <param name="argumentId">The argument identifier.</param>
    /// <param name="validationAttribute">The argument validation attribute.</param>
    /// <param name="validationParams">The validation attribute parameters.</param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class ArgumentValidationAttribute(string argumentId, Type validationAttribute, params object[] validationParams) : Attribute, IArgumentValidationAttribute
    {
        /// <summary>
        /// The argument identifier.
        /// </summary>
        public string ArgumentId { get; } = argumentId;

        /// <summary>
        /// The <see cref="ValidationAttribute"/> parameters.
        /// </summary>
        public object[]? ValidationParams { get; } = validationParams;

        /// <summary>
        /// The attribute validation attribute.
        /// </summary>
        public Type ValidationAttribute { get; } = validationAttribute;
    }
}