//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines the command type.
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// The command represents the root of the command hierarchy.
        /// </summary>
        Root = 1,

        /// <summary>
        /// The command represents an isolated group that is independent of its nested groups or individual leaf commands.
        /// </summary>
        IsolatedGroup = 2,

        /// <summary>
        /// The command represents a composite group that self-contains its nested groups or individual leaf commands.
        /// </summary>
        CompositeGroup = 3,

        /// <summary>
        /// The command represents a leaf command that can belong to a group or root.
        /// </summary>
        Leaf = 4,

        /// <summary>
        /// The command represents a native terminal command that does not follow the command hierarchy such as <c>cls</c> for clearing the terminal or <c>run</c> for executing OS commands.
        /// </summary>
        Native = 5,
    }
}