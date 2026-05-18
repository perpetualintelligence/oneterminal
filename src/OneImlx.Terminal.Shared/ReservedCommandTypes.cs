//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// Defines the command types.
    /// </summary>
    public static class ReservedCommandTypes
    {
        /// <summary>
        /// The command represents the root of the command hierarchy.
        /// </summary>
        public const int Root = 1;

        /// <summary>
        /// The command represents an isolated group that is independent of its nested groups or individual leaf commands.
        /// </summary>
        public const int IsolatedGroup = 2;

        /// <summary>
        /// The command represents a composite group that self-contains its nested groups or individual leaf commands.
        /// </summary>
        public const int CompositeGroup = 3;

        /// <summary>
        /// The command represents a leaf or a sub-command that can belong to a group or root.
        /// </summary>
        public const int Leaf = 4;

        /// <summary>
        /// The command represents a native terminal command that does not follow the command hierarchy such as <c>cls</c> for clearing the terminal or <c>run</c> for executing OS commands.
        /// </summary>
        public const int Native = 5;
    }
}