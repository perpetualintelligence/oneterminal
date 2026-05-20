//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// Defines the reserved behavior flags.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The framework operates on the underlying numeric flag values instead of fixed enum names.
    /// This allows applications to define their own flag constants, enums, and naming conventions
    /// while preserving compatibility with the framework behavior model.
    /// </para>
    /// <para>
    /// Applications can map their own semantic naming to the reserved framework values. For example,
    /// an application may define <c>Secured = 8</c> instead of <c>Authorize = 8</c>. The underlying
    /// numeric value preserves the expected framework behavior.
    /// </para>
    /// <para>
    /// Reserved framework behavior flag values:
    /// </para>
    /// <list type="bullet">
    /// <item><description>1 = Required</description></item>
    /// <item><description>2 = Obsolete</description></item>
    /// <item><description>4 = Disabled</description></item>
    /// <item><description>8 = Authorize</description></item>
    /// </list>
    /// </remarks>
    public static class BehaviorFlags
    {
        /// <summary>
        /// No special flags.
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// Required.
        /// </summary>
        public const int Required = 1;

        /// <summary>
        /// Obsolete.
        /// </summary>
        public const int Obsolete = 2;

        /// <summary>
        /// Disabled.
        /// </summary>
        public const int Disabled = 4;

        /// <summary>
        /// Authorize.
        /// </summary>
        public const int Authorize = 8;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future1 = 16;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future2 = 32;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future3 = 64;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future4 = 128;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future5 = 256;
    }
}