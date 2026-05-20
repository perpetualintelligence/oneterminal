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
            int flags = BehaviorFlags.Required | BehaviorFlags.Disabled;

            bool result = flags.HasFlag(BehaviorFlags.Required);

            result.Should().BeTrue();
        }

        [Fact]
        public void HasFlag_Returns_False_When_Flag_Is_Not_Set()
        {
            int flags = BehaviorFlags.Required | BehaviorFlags.Authorize;

            bool result = flags.HasFlag(BehaviorFlags.Disabled);

            result.Should().BeFalse();
        }

        [Fact]
        public void AddFlag_Adds_Flag()
        {
            int flags = BehaviorFlags.None;

            flags = flags.AddFlag(BehaviorFlags.Required);
            flags.HasFlag(BehaviorFlags.Required).Should().BeTrue();

            flags = flags.AddFlag(BehaviorFlags.Disabled);
            flags.HasFlag(BehaviorFlags.Disabled).Should().BeTrue();
        }

        [Fact]
        public void AddFlag_Preserves_Existing_Flags()
        {
            int flags = BehaviorFlags.Required;

            flags = flags.AddFlag(BehaviorFlags.Authorize);

            flags.HasFlag(BehaviorFlags.Required).Should().BeTrue();
            flags.HasFlag(BehaviorFlags.Authorize).Should().BeTrue();
        }

        [Fact]
        public void RemoveFlag_Removes_Flag()
        {
            int flags = BehaviorFlags.Required | BehaviorFlags.Disabled;

            flags = flags.RemoveFlag(BehaviorFlags.Required);

            flags.HasFlag(BehaviorFlags.Required).Should().BeFalse();
            flags.HasFlag(BehaviorFlags.Disabled).Should().BeTrue();
        }

        [Fact]
        public void RemoveFlag_Does_Not_Remove_Other_Flags()
        {
            int flags = BehaviorFlags.Required | BehaviorFlags.Authorize;

            flags = flags.RemoveFlag(BehaviorFlags.Authorize);

            flags.HasFlag(BehaviorFlags.Required).Should().BeTrue();
            flags.HasFlag(BehaviorFlags.Authorize).Should().BeFalse();
        }

        [Fact]
        public void RemoveFlag_On_Missing_Flag_Does_Nothing()
        {
            int flags = BehaviorFlags.Required;

            flags = flags.RemoveFlag(BehaviorFlags.Disabled);

            flags.HasFlag(BehaviorFlags.Required).Should().BeTrue();
            flags.HasFlag(BehaviorFlags.Disabled).Should().BeFalse();
        }
    }
}