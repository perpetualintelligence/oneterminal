//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalSystemConsoleTests : IDisposable
    {
        public TerminalSystemConsoleTests()
        {
            _console = new TerminalSystemConsole();
            _originalOut = Console.Out;
            _originalIn = Console.In;
            _writer = new StringWriter();
            Console.SetOut(_writer);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOut);
            Console.SetIn(_originalIn);
            _writer.Dispose();
        }

        [Fact]
        public void Ignore_Null_ReturnsTrue()
        {
            _console.Ignore(null).Should().BeTrue();
        }

        [Fact]
        public void Ignore_Empty_ReturnsTrue()
        {
            _console.Ignore(string.Empty).Should().BeTrue();
        }

        [Fact]
        public void Ignore_Whitespace_ReturnsTrue()
        {
            _console.Ignore("   ").Should().BeTrue();
        }

        [Fact]
        public void Ignore_NonWhitespace_ReturnsFalse()
        {
            _console.Ignore("value").Should().BeFalse();
        }

        [Fact]
        public async Task WriteAsync_WritesFormattedString()
        {
            await _console.WriteAsync("hello {0}", "world");

            _writer.ToString().Should().Be("hello world");
        }

        [Fact]
        public async Task WriteAsync_NoFormatArgs_WritesPlainString()
        {
            await _console.WriteAsync("hello");

            _writer.ToString().Should().Be("hello");
        }

        [Fact]
        public async Task WriteColorAsync_WritesFormattedString()
        {
            await _console.WriteColorAsync(ConsoleColor.Cyan, "hello {0}", "world");

            _writer.ToString().Should().Be("hello world");
        }

        [Fact]
        public async Task WriteLineAsync_NoArgs_WritesNewLine()
        {
            await _console.WriteLineAsync();

            _writer.ToString().Should().Be(Environment.NewLine);
        }

        [Fact]
        public async Task WriteLineAsync_PlainString_WritesLine()
        {
            await _console.WriteLineAsync("hello");

            _writer.ToString().Should().Be("hello" + Environment.NewLine);
        }

        [Fact]
        public async Task WriteLineAsync_FormattedString_WritesLine()
        {
            await _console.WriteLineAsync("hello {0}", "world");

            _writer.ToString().Should().Be("hello world" + Environment.NewLine);
        }

        [Fact]
        public async Task WriteLineColorAsync_WritesFormattedLine()
        {
            await _console.WriteLineColorAsync(ConsoleColor.Green, "hello {0}", "world");

            _writer.ToString().Should().Be("hello world" + Environment.NewLine);
        }

        [Fact]
        public async Task ReadAnswerAsync_NullAnswers_PromptsWithQuestionMark()
        {
            Console.SetIn(new StringReader("yes"));

            string result = await _console.ReadAnswerAsync("Continue", "yes", "no");

            _writer.ToString().Should().Be("Continue (yes/no)? ");
            result.Should().Be("yes");
        }

        [Fact]
        public async Task ReadAnswerAsync_WithAnswers_PromptsWithChoices()
        {
            Console.SetIn(new StringReader("yes"));

            string result = await _console.ReadAnswerAsync("Continue", "yes", "no");

            _writer.ToString().Should().Be("Continue (yes/no)? ");
            result.Should().Be("yes");
        }

        [Fact]
        public async Task ReadLineAsync_ReturnsInputLine()
        {
            Console.SetIn(new StringReader("some input"));

            string? result = await _console.ReadLineAsync();

            result.Should().Be("some input");
        }

        [Fact]
        public async Task ReadLineAsync_NoInput_ReturnsNull()
        {
            Console.SetIn(new StringReader(string.Empty));

            string? result = await _console.ReadLineAsync();

            result.Should().BeNull();
        }

        [Fact]
        public async Task WriteLineAsync_ConcurrentWrites_AllLinesPresent()
        {
            Task[] tasks = new Task[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                int captured = i;
                tasks[i] = _console.WriteLineAsync("line {0}", captured);
            }

            await Task.WhenAll(tasks);

            string output = _writer.ToString();
            for (int i = 0; i < tasks.Length; i++)
            {
                output.Should().Contain($"line {i}");
            }
        }

        private readonly TerminalSystemConsole _console;
        private readonly TextWriter _originalOut;
        private readonly TextReader _originalIn;
        private readonly StringWriter _writer;
    }
}