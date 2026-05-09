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
        /// The command represents a composite group that delegates to individual leaf sub-commands.
        /// </summary>
        CompositeGroup = 2,

        /// <summary>
        /// The command represents an integrated group that self-contains all sub-command logic.
        /// </summary>
        IntegratedGroup = 3,

        /// <summary>
        /// The command represents a leaf sub-command that can belong to a group or root.
        /// </summary>
        Leaf = 4,

        /// <summary>
        /// The command represents a native terminal command that does not follow the hierarchy such as <c>cls</c> for clearing the terminal or <c>run</c> for executing OS commands.
        /// </summary>
        Native = 5,
    }
}