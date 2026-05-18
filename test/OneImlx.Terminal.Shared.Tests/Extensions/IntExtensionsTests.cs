//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Extensions;
using Xunit;

namespace OneImlx.Terminal.Tests.Shared.Extensions
{
    /// <summary>
    /// Tests for <see cref="IntExtensions"/>.
    /// </summary>
    public class IntExtensionsTests
    {
        [Fact]
        public void HasFlag_Returns_True_When_Flag_Is_Set()
        {
            int flags = ReservedFlags.Required | ReservedFlags.Disabled;

            bool result = flags.HasFlag(ReservedFlags.Required);

            result.Should().BeTrue();
        }

        [Fact]
        public void HasFlag_Returns_False_When_Flag_Is_Not_Set()
        {
            int flags = ReservedFlags.Required | ReservedFlags.Authorize;

            bool result = flags.HasFlag(ReservedFlags.Disabled);

            result.Should().BeFalse();
        }

        [Fact]
        public void AddFlag_Adds_Flag()
        {
            int flags = ReservedFlags.None;

            flags = flags.AddFlag(ReservedFlags.Required);
            flags.HasFlag(ReservedFlags.Required).Should().BeTrue();

            flags = flags.AddFlag(ReservedFlags.Disabled);
            flags.HasFlag(ReservedFlags.Disabled).Should().BeTrue();
        }

        [Fact]
        public void AddFlag_Preserves_Existing_Flags()
        {
            int flags = ReservedFlags.Required;

            flags = flags.AddFlag(ReservedFlags.Authorize);

            flags.HasFlag(ReservedFlags.Required).Should().BeTrue();
            flags.HasFlag(ReservedFlags.Authorize).Should().BeTrue();
        }

        [Fact]
        public void RemoveFlag_Removes_Flag()
        {
            int flags = ReservedFlags.Required | ReservedFlags.Disabled;

            flags = flags.RemoveFlag(ReservedFlags.Required);

            flags.HasFlag(ReservedFlags.Required).Should().BeFalse();
            flags.HasFlag(ReservedFlags.Disabled).Should().BeTrue();
        }

        [Fact]
        public void RemoveFlag_Does_Not_Remove_Other_Flags()
        {
            int flags = ReservedFlags.Required | ReservedFlags.Authorize;

            flags = flags.RemoveFlag(ReservedFlags.Authorize);

            flags.HasFlag(ReservedFlags.Required).Should().BeTrue();
            flags.HasFlag(ReservedFlags.Authorize).Should().BeFalse();
        }

        [Fact]
        public void RemoveFlag_On_Missing_Flag_Does_Nothing()
        {
            int flags = ReservedFlags.Required;

            flags = flags.RemoveFlag(ReservedFlags.Disabled);

            flags.HasFlag(ReservedFlags.Required).Should().BeTrue();
            flags.HasFlag(ReservedFlags.Disabled).Should().BeFalse();
        }
    }
}