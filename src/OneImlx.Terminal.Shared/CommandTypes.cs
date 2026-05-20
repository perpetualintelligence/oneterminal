//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// Defines the reserved command types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The framework operates on the underlying numeric command type values instead of fixed enum names.
    /// This allows applications to define their own command type constants, enums, and naming conventions
    /// while preserving compatibility with the framework command hierarchy and execution model.
    /// </para>
    /// <para>
    /// Applications can map their own semantic naming to the reserved framework values. For example,
    /// an application may define <c>Suite = 3</c> instead of <c>CompositeGroup = 3</c>. The underlying
    /// numeric value preserves the expected framework behavior.
    /// </para>
    /// <para>
    /// Reserved framework command type values:
    /// </para>
    /// <list type="bullet">
    /// <item><description>1 = Root</description></item>
    /// <item><description>2 = IsolatedGroup</description></item>
    /// <item><description>3 = CompositeGroup</description></item>
    /// <item><description>4 = Leaf</description></item>
    /// <item><description>5 = Native</description></item>
    /// </list>
    /// </remarks>
    public static class CommandTypes
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

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future1 = 6;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future2 = 7;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future3 = 8;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future4 = 9;

        /// <summary>
        /// Reserved for future.
        /// </summary>
        public const int Future5 = 10;
    }
}