//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Shared;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class OptionDescriptorTests
    {
        [Fact]
        public void CustomDataTypeShouldSetDataType()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", BehaviorFlags.Required);
            arg.DataType.Should().Be("custom");
        }

        [Fact]
        public void MultipleFlagsShouldBeSet()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", BehaviorFlags.Required | BehaviorFlags.Obsolete | BehaviorFlags.Disabled);
            arg.Flags.Should().Be(BehaviorFlags.Required | BehaviorFlags.Obsolete | BehaviorFlags.Disabled);
        }

        [Fact]
        public void RequiredExplicitlySetShouldNotSetDataAnnotationRequiredAttribute()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", BehaviorFlags.Required);
            arg.ValueCheckers.Should().BeNull();
            arg.Flags.Should().Be(BehaviorFlags.Required);
        }

        [Fact]
        public void RequiredShouldBeSetWithDataAnnotationRequiredAttribute()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", BehaviorFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new RequiredAttribute())] };
            arg.ValueCheckers.Should().NotBeNull();
            arg.Flags.Should().Be(BehaviorFlags.Required);
        }
    }
}