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
            BehaviorFlags.None.Should().Be(0);
            BehaviorFlags.Required.Should().Be(1);
            BehaviorFlags.Obsolete.Should().Be(2);
            BehaviorFlags.Disabled.Should().Be(4);
            BehaviorFlags.Authorize.Should().Be(8);
            BehaviorFlags.Future1.Should().Be(16);
            BehaviorFlags.Future2.Should().Be(32);
            BehaviorFlags.Future3.Should().Be(64);
            BehaviorFlags.Future4.Should().Be(128);
            BehaviorFlags.Future5.Should().Be(256);
        }
    }
}