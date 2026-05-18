//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Shared
{
    public class ReservedFlagsTests
    {
        [Fact]
        public void FlagValuesAreValid()
        {
            ReservedFlags.None.Should().Be(0);
            ReservedFlags.Required.Should().Be(1);
            ReservedFlags.Obsolete.Should().Be(2);
            ReservedFlags.Disabled.Should().Be(4);
            ReservedFlags.Authorize.Should().Be(8);
            ReservedFlags.Future1.Should().Be(16);
            ReservedFlags.Future2.Should().Be(32);
            ReservedFlags.Future3.Should().Be(64);
            ReservedFlags.Future4.Should().Be(128);
            ReservedFlags.Future5.Should().Be(256);
        }
    }
}