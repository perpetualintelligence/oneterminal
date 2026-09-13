//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Text;

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The runtime configuration options.
    /// </summary>
    public sealed class RuntimeOptions
    {
        /// <summary>
        /// Enables the runtime command source registration for the terminal.
        /// </summary>
        public bool CommandSourceEnabled { get; set; }

        /// <summary>
        /// Enables the declarative syntax for the terminal.
        /// </summary>
        public bool DeclarativeEnabled { get; set; }

        /// <summary>
        /// Enables the custom types and custom declaration for the terminal.
        /// </summary>
        public bool CustomEnabled { get; set; }

        /// <summary>
        /// The terminal command store used for licensing checks. Defaults to <c>memory</c>.
        /// </summary>
        /// <remarks>
        /// Valid values are <c>memory</c> and <c>custom</c>. This value must match the command store registered in
        /// the service collection and must be permitted by the active license plan.
        /// </remarks>
        public string Store { get; set; } = "in-memory";
    }
}