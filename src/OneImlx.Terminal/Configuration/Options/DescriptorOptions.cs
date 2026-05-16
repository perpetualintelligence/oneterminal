//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The descriptor configuration options.
    /// </summary>
    public sealed class DescriptorOptions
    {
        /// <summary>
        /// The descriptor definition style. It can be <see cref="TerminalIdentifiers.DeclaritiveDefinition"/> or <see cref="TerminalIdentifiers.ExplicitDefinition"/>. Defaults to <see cref="TerminalIdentifiers.DeclaritiveDefinition"/>.
        /// </summary>
        public string Definition { get; set; } = TerminalIdentifiers.DeclaritiveDefinition;

        /// <summary>
        /// Gets or sets a value indicating whether custom declarative attributes are used fo command definitions.
        /// </summary>
        /// <remarks>
        /// Once this is enabled you cannot use default declarative attributes defined in <see cref="OneImlx.Terminal.Commands.Declarative"/> namespace.
        /// </remarks>
        public bool CustomDeclarations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether composite groups are enabled.
        /// </summary>
        /// <seealso cref="CommandType.CompositeGroup"/>
        public bool CompositeGroups { get; set; }
    }
}