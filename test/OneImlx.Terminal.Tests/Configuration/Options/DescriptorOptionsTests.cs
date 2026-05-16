//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Configuration.Options
{
    public class DescriptorOptionsTests
    {
        public void HasCorrectDefaultValues()
        {
            DescriptorOptions options = new();
            options.CompositeGroups.Should().BeFalse();
            options.CustomDeclarations.Should().BeFalse();
            options.Definition.Should().Be(TerminalIdentifiers.DeclaritiveDefinition);
        }
    }
}