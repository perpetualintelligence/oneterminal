﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Mocks;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Stores.InMemory
{
    public class InMemoryCommandDescriptorStoreTests
    {
        public InMemoryCommandDescriptorStoreTests()
        {
            cmdStore = new InMemoryCommandStore(MockCommands.Commands);
        }

        [Fact]
        public async Task FindByIdShouldNotErrorIfNotFoundAsync()
        {
            bool found = await cmdStore.TryFindByIdAsync("invalid_id", out CommandDescriptor? commandDescriptor);
            found.Should().BeFalse();
            commandDescriptor.Should().BeNull();
        }

        [Fact]
        public async Task TryFindByIdShouldNotErrorIfFoundAsync()
        {
            bool found = await cmdStore.TryFindByIdAsync("id1", out CommandDescriptor? commandDescriptor);
            found.Should().BeTrue();
            commandDescriptor.Should().NotBeNull();
            commandDescriptor!.Id.Should().Be("id1");
        }

        [Fact]
        public async Task AllShouldReturnAllCommands()
        {
            var result = await cmdStore.AllAsync();
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result.Keys.Should().Contain("id1");
            result.Keys.Should().Contain("id2");
            result.Keys.Should().Contain("id3");
            result.Keys.Should().Contain("id4");
            result.Keys.Should().Contain("id5");
        }

        private InMemoryCommandStore cmdStore = null!;
    }
}