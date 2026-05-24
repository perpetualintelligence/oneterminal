//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalConsoleHelpProviderTests
    {
        public TerminalConsoleHelpProviderTests()
        {
            _console = new MockTerminalConsole();
            _options = new TerminalOptions
            {
                Parser = new ParserOptions { OptionPrefix = '-' }
            };
            _provider = new TerminalConsoleHelpProvider(_options, _console);
        }

        [Fact]
        public async Task ProvideHelpAsync_CommandOnly_WritesExactMessages()
        {
            var (_, command) = MockCommands.NewCommandDefinition("id4", "name4", "desc4", CommandTypes.Leaf);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(3);
            _console.Messages[0].Should().Be("Command:");
            _console.Messages[1].Should().Be("  id4 (name4) 4");
            _console.Messages[2].Should().Be("    desc4");
        }

        [Fact]
        public async Task ProvideHelpAsync_OptionWithoutAlias_WritesExactMessages()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            var optionDescriptors = new OptionDescriptors(textHandler,
            [
                new OptionDescriptor("verbose", nameof(Boolean), "Enable verbose output", BehaviorFlags.None)
            ]);
            var (_, command) = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(6);
            _console.Messages[0].Should().Be("Command:");
            _console.Messages[1].Should().Be("  id1 (name1) 4");
            _console.Messages[2].Should().Be("    desc1");
            _console.Messages[3].Should().Be("Options:");
            _console.Messages[4].Should().Be("  --verbose <Boolean>");
            _console.Messages[5].Should().Be("    Enable verbose output");
        }

        [Fact]
        public async Task ProvideHelpAsync_OptionWithAlias_WritesExactMessages()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            var optionDescriptors = new OptionDescriptors(textHandler,
            [
                new OptionDescriptor("verbose", nameof(Boolean), "Enable verbose output", BehaviorFlags.None, "v")
            ]);
            var (_, command) = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(6);
            _console.Messages[0].Should().Be("Command:");
            _console.Messages[1].Should().Be("  id1 (name1) 4");
            _console.Messages[2].Should().Be("    desc1");
            _console.Messages[3].Should().Be("Options:");
            _console.Messages[4].Should().Be("  --verbose, -v <Boolean>");
            _console.Messages[5].Should().Be("    Enable verbose output");
        }

        [Fact]
        public async Task ProvideHelpAsync_NoOptionsNoArguments_WritesOnlyCommandMessages()
        {
            var (_, command) = MockCommands.NewCommandDefinition("id4", "name4", "desc4", CommandTypes.Leaf);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(3);
            _console.Messages.Should().NotContain("Options:");
            _console.Messages.Should().NotContain("Arguments:");
        }

        [Fact]
        public async Task ProvideHelpAsync_ArgumentsOnly_WritesExactMessages()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            var argumentDescriptors = new ArgumentDescriptors(textHandler,
            [
                new ArgumentDescriptor(1, "file",  nameof(String), "The file path",  BehaviorFlags.Required),
                new ArgumentDescriptor(2, "count", nameof(Int32),  "The item count", BehaviorFlags.None),
            ]);
            var (_, command) = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandTypes.Leaf,
                argumentDescriptors: argumentDescriptors);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(8);
            _console.Messages[0].Should().Be("Command:");
            _console.Messages[1].Should().Be("  id1 (name1) 4");
            _console.Messages[2].Should().Be("    desc1");
            _console.Messages[3].Should().Be("Arguments:");
            _console.Messages[4].Should().Be("  file <String>");
            _console.Messages[5].Should().Be("    The file path");
            _console.Messages[6].Should().Be("  count <Int32>");
            _console.Messages[7].Should().Be("    The item count");
        }

        [Fact]
        public async Task ProvideHelpAsync_ArgumentsAndOptions_WritesExactMessages()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            var argumentDescriptors = new ArgumentDescriptors(textHandler,
            [
                new ArgumentDescriptor(1, "source", nameof(String), "The source path", BehaviorFlags.Required),
            ]);
            var optionDescriptors = new OptionDescriptors(textHandler,
            [
                new OptionDescriptor("verbose", nameof(Boolean), "Enable verbose output", BehaviorFlags.None, "v"),
            ]);
            var (_, command) = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandTypes.Leaf,
                args: optionDescriptors, argumentDescriptors: argumentDescriptors);

            await _provider.ProvideHelpAsync(new TerminalHelpProviderContext(command));

            _console.Messages.Should().HaveCount(9);
            _console.Messages[0].Should().Be("Command:");
            _console.Messages[1].Should().Be("  id1 (name1) 4");
            _console.Messages[2].Should().Be("    desc1");
            _console.Messages[3].Should().Be("Arguments:");
            _console.Messages[4].Should().Be("  source <String>");
            _console.Messages[5].Should().Be("    The source path");
            _console.Messages[6].Should().Be("Options:");
            _console.Messages[7].Should().Be("  --verbose, -v <Boolean>");
            _console.Messages[8].Should().Be("    Enable verbose output");
        }

        private readonly MockTerminalConsole _console;
        private readonly TerminalOptions _options;
        private readonly TerminalConsoleHelpProvider _provider;
    }
}