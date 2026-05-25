//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalNoConsoleTests
    {
        public TerminalNoConsoleTests()
        {
            _console = new TerminalNoConsole();
        }

        [Fact]
        public void BackgroundColor_GetSet_RoundTrips()
        {
            _console.BackgroundColor = ConsoleColor.Red;
            _console.BackgroundColor.Should().Be(ConsoleColor.Red);
        }

        [Fact]
        public void ForegroundColor_GetSet_RoundTrips()
        {
            _console.ForegroundColor = ConsoleColor.Green;
            _console.ForegroundColor.Should().Be(ConsoleColor.Green);
        }

        [Fact]
        public async Task ClearAsync_ReturnsCompletedTask()
        {
            Task result = _console.ClearAsync();
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        [Fact]
        public void Ignore_Always_ReturnsTrue()
        {
            _console.Ignore("any value").Should().BeTrue();
            _console.Ignore(string.Empty).Should().BeTrue();
            _console.Ignore(null).Should().BeTrue();
        }

        [Fact]
        public async Task ReadAnswerAsync_Always_ReturnsEmptyString()
        {
            string result = await _console.ReadAnswerAsync("question?", "yes", "no");
            result.Should().Be(string.Empty);
        }

        [Fact]
        public async Task ReadLineAsync_Always_ReturnsNull()
        {
            string? result = await _console.ReadLineAsync();
            result.Should().BeNull();
        }

        [Fact]
        public async Task WriteAsync_ReturnsCompletedTask()
        {
            Task result = _console.WriteAsync("hello {0}", "world");
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        [Fact]
        public async Task WriteColorAsync_ReturnsCompletedTask()
        {
            Task result = _console.WriteColorAsync(ConsoleColor.Cyan, "hello {0}", "world");
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        [Fact]
        public async Task WriteLineAsync_NoArgs_ReturnsCompletedTask()
        {
            Task result = _console.WriteLineAsync();
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        [Fact]
        public async Task WriteLineAsync_WithArgs_ReturnsCompletedTask()
        {
            Task result = _console.WriteLineAsync("hello {0}", "world");
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        [Fact]
        public async Task WriteLineColorAsync_ReturnsCompletedTask()
        {
            Task result = _console.WriteLineColorAsync(ConsoleColor.Yellow, "hello {0}", "world");
            result.IsCompleted.Should().BeTrue();
            await result;
        }

        private readonly TerminalNoConsole _console;
    }
}