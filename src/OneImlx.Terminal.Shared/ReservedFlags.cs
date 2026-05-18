//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// Defines special reserved flags.
    /// </summary>
    public static class ReservedFlags
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
        /// Future reserved.
        /// </summary>
        public const int Future1 = 16;

        /// <summary>
        /// Future reserved.
        /// </summary>
        public const int Future2 = 32;

        /// <summary>
        /// Future reserved.
        /// </summary>
        public const int Future3 = 64;

        /// <summary>
        /// Future reserved.
        /// </summary>
        public const int Future4 = 128;

        /// <summary>
        /// Future reserved.
        /// </summary>
        public const int Future5 = 256;
    }
}