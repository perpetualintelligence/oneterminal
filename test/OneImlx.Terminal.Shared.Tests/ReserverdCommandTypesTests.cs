//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Shared
{
    public class ReserverdCommandTypesTests
    {
        [Fact]
        public void HasCorrectValues()
        {
            CommandTypes.Root.Should().Be(1);
            CommandTypes.IsolatedGroup.Should().Be(2);
            CommandTypes.CompositeGroup.Should().Be(3);
            CommandTypes.Leaf.Should().Be(4);
            CommandTypes.Native.Should().Be(5);
            CommandTypes.Future1.Should().Be(6);
            CommandTypes.Future2.Should().Be(7);
            CommandTypes.Future3.Should().Be(8);
            CommandTypes.Future4.Should().Be(9);
            CommandTypes.Future5.Should().Be(10);
        }
    }
}