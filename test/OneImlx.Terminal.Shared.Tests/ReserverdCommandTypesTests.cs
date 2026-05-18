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
            ReservedCommandTypes.Root.Should().Be(1);
            ReservedCommandTypes.IsolatedGroup.Should().Be(2);
            ReservedCommandTypes.CompositeGroup.Should().Be(3);
            ReservedCommandTypes.Leaf.Should().Be(4);
            ReservedCommandTypes.Native.Should().Be(5);
        }
    }
}