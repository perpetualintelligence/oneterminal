//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="int"/>.
    /// </summary>
    public static class IntExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the specified flag is set.
        /// </summary>
        /// <param name="flags">The flags value.</param>
        /// <param name="flag">The flag to check.</param>
        /// <returns><c>true</c> if the flag is set; otherwise <c>false</c>.</returns>
        public static bool HasFlag(this int flags, int flag)
        {
            return (flags & flag) == flag;
        }

        /// <summary>
        /// Adds the specified flag.
        /// </summary>
        /// <param name="flags">The flags value.</param>
        /// <param name="flag">The flag to add.</param>
        /// <returns>The updated flags value.</returns>
        public static int AddFlag(this int flags, int flag)
        {
            return flags | flag;
        }

        /// <summary>
        /// Removes the specified flag.
        /// </summary>
        /// <param name="flags">The flags value.</param>
        /// <param name="flag">The flag to remove.</param>
        /// <returns>The updated flags value.</returns>
        public static int RemoveFlag(this int flags, int flag)
        {
            return flags & ~flag;
        }
    }
}