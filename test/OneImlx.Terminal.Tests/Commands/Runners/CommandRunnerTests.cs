//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Runners.Mocks;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Runners
{
    public class CommandRunnerTests
    {
        public CommandRunnerTests()
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            routerContext = new MockTerminalRouterContext(TerminalStartMode.Custom, commandTokenSource.Token);
            commandContext = new CommandContext(new(Guid.NewGuid().ToString(), "test"), routerContext, null);
        }

        [Fact]
        public async Task DelegateHelpShouldCallHelpAsync()
        {
            CommandRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandTypes.Leaf));
            ParsedCommand extractedCommand = new(command, null);
            commandContext.ParsedCommand = extractedCommand;

            MockTerminalHelpProvider helpProvider = new();
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateHelpAsync(commandContext, helpProvider);
            mockCommandRunner.HelpCalled.Should().BeTrue();
            helpProvider.HelpCalled.Should().BeTrue();
            mockCommandRunner.RunCalled.Should().BeFalse();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task DelegateRunShouldCallRunAsync()
        {
            CommandRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandTypes.Leaf));
            ParsedCommand extractedCommand = new(command, null);
            commandContext.ParsedCommand = extractedCommand;

            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateRunAsync(commandContext);
            mockCommandRunner.RunCalled.Should().BeTrue();
            mockCommandRunner.HelpCalled.Should().BeFalse();
            result.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task HelpShouldThrowIfIHelpProviderIsNullAsync()
        {
            CommandRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandTypes.Leaf));
            ParsedCommand extractedCommand = new(command, null);
            commandContext.ParsedCommand = extractedCommand;

            MockDefaultCommandRunner mockCommandRunner = new();
            Func<Task> act = () => mockCommandRunner.RunHelpAsync(commandContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The help provider is missing in the configured services.");
        }

        private readonly CancellationTokenSource commandTokenSource = null!;
        private readonly CommandContext commandContext = null!;
        private readonly TerminalRouterContext routerContext = null!;
        private readonly CancellationTokenSource terminalTokenSource = null!;
    }
}