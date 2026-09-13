//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class RuntimeOptionsTests
    {
        [Fact]
        public void HasCorrect_DefaultValues()
        {
            RuntimeOptions options = new();

            options.CommandSourceEnabled.Should().BeFalse();
            options.DeclarativeEnabled.Should().BeFalse();
            options.CustomEnabled.Should().BeFalse();

            options.Store.Should().Be("in-memory");
        }
    }
}