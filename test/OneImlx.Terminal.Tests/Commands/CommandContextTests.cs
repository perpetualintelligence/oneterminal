//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class CommandContextTests
    {
        [Fact]
        public void Constructor_Sets_Properties()
        {
            var properties = new Dictionary<string, object> { ["key"] = "value" };
            CommandContext context = new(properties);
            context.Properties.Should().BeSameAs(properties);
        }
    }
}