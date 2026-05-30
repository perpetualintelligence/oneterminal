//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace OneImlx.Terminal.Tests.Extensions
{
    public class ICommandContextExtensionsTests
    {
        private readonly Command command;
        private readonly ParsedCommand parsedCommand;

        public ICommandContextExtensionsTests()
        {
            ITerminalTextHandler textHandler = new Mock<ITerminalTextHandler>().Object;

            OptionDescriptor optionDescriptor = new("opt1", "System.Int32", "Option 1", 0, null);
            Option option = new(optionDescriptor, 123);
            Options options = new(textHandler, new[] { option });

            ArgumentDescriptor argumentDescriptor = new(0, "arg1", "System.Int32", "Argument 1", 0);
            Argument argument = new(argumentDescriptor, 42);
            Arguments arguments = new(textHandler, new[] { argument });

            CommandDescriptor commandDescriptor = new("id1", "name1", "desc1", CommandTypes.Leaf);
            command = new Command(commandDescriptor, arguments, options);
            parsedCommand = new ParsedCommand(command, null);
        }

        private static ICommandContext Context(Dictionary<string, object>? properties = null)
        {
            Mock<ICommandContext> mock = new();
            Dictionary<string, object> props = properties ?? new Dictionary<string, object>();
            mock.Setup(x => x.Properties).Returns(props);
            return mock.Object;
        }

        [Fact]
        public void GetParsedCommand_Returns_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            ParsedCommand result = context.GetParsedCommand();
            result.Should().Be(parsedCommand);
        }

        [Fact]
        public void GetParsedCommand_Throws_WhenMissing()
        {
            ICommandContext context = Context();
            Action act = () => context.GetParsedCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError)
                .WithErrorDescription("The parsed command is missing in the context.");
        }

        [Fact]
        public void GetParsedCommand_Throws_WhenWrongType()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, "wrong" } };
            ICommandContext context = Context(properties);
            Action act = () => context.GetParsedCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError);
        }

        [Fact]
        public void TryGetParsedCommand_ReturnsTrue_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetParsedCommand(out ParsedCommand? result);
            found.Should().BeTrue();
            result.Should().Be(parsedCommand);
        }

        [Fact]
        public void TryGetParsedCommand_ReturnsFalse_WhenMissing()
        {
            ICommandContext context = Context();
            bool found = context.TryGetParsedCommand(out ParsedCommand? result);
            found.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void SetParsedCommand_SetsValue()
        {
            ICommandContext context = Context(new Dictionary<string, object>());
            context.SetParsedCommand(parsedCommand);
            context.Properties[TerminalIdentifiers.ParsedCommand].Should().Be(parsedCommand);
        }

        [Fact]
        public void GetCommand_Returns_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            Command result = context.GetCommand();
            result.Should().Be(command);
        }

        [Fact]
        public void GetCommand_Throws_WhenParsedCommandMissing()
        {
            ICommandContext context = Context();
            Action act = () => context.GetCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError);
        }

        [Fact]
        public void GetCommandRequest_Returns_WhenAvailable()
        {
            CommandRequest request = new("req-1", "cmd");
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.CommandRequest, request } };
            ICommandContext context = Context(properties);
            CommandRequest result = context.GetCommandRequest();
            result.Should().BeSameAs(request);
        }

        [Fact]
        public void GetCommandRequest_Throws_WhenMissing()
        {
            ICommandContext context = Context();
            Action act = () => context.GetCommandRequest();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError)
                .WithErrorDescription("The command request is missing in the context.");
        }

        [Fact]
        public void TryGetCommandRequest_ReturnsTrue_WhenAvailable()
        {
            CommandRequest request = new("req-1", "cmd");
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.CommandRequest, request } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetCommandRequest(out CommandRequest? result);
            found.Should().BeTrue();
            result.Should().BeSameAs(request);
        }

        [Fact]
        public void TryGetCommandRequest_ReturnsFalse_WhenMissing()
        {
            ICommandContext context = Context();
            bool found = context.TryGetCommandRequest(out CommandRequest? result);
            found.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void SetCommandRequest_SetsValue()
        {
            CommandRequest request = new("req-1", "cmd");
            ICommandContext context = Context(new Dictionary<string, object>());
            context.SetCommandRequest(request);
            context.Properties[TerminalIdentifiers.CommandRequest].Should().BeSameAs(request);
        }

        [Fact]
        public void GetRouterContext_Returns_WhenAvailable()
        {
            MockRoutingContext routerContext = new(TerminalStartMode.Console, CancellationToken.None);
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.RouterContext, routerContext } };
            ICommandContext context = Context(properties);
            TerminalRouterContext result = context.GetRouterContext();
            result.Should().BeSameAs(routerContext);
        }

        [Fact]
        public void GetRouterContext_Throws_WhenMissing()
        {
            ICommandContext context = Context();
            Action act = () => context.GetRouterContext();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The router context is missing in the context.");
        }

        [Fact]
        public void TryGetRouterContext_ReturnsTrue_WhenAvailable()
        {
            MockRoutingContext routerContext = new(TerminalStartMode.Console, CancellationToken.None);
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.RouterContext, routerContext } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetRouterContext(out TerminalRouterContext? result);
            found.Should().BeTrue();
            result.Should().BeSameAs(routerContext);
        }

        [Fact]
        public void TryGetRouterContext_ReturnsFalse_WhenMissing()
        {
            ICommandContext context = Context();
            bool found = context.TryGetRouterContext(out TerminalRouterContext? result);
            found.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void SetRouterContext_SetsValue()
        {
            MockRoutingContext routerContext = new(TerminalStartMode.Console, CancellationToken.None);
            ICommandContext context = Context(new Dictionary<string, object>());
            context.SetRouterContext(routerContext);
            context.Properties[TerminalIdentifiers.RouterContext].Should().BeSameAs(routerContext);
        }

        [Fact]
        public void GetCommandResult_Returns_WhenAvailable()
        {
            CommandResult commandResult = new();
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.CommandResult, commandResult } };
            ICommandContext context = Context(properties);
            CommandResult result = context.GetCommandResult();
            result.Should().Be(commandResult);
        }

        [Fact]
        public void GetCommandResult_Throws_WhenMissing()
        {
            ICommandContext context = Context();
            Action act = () => context.GetCommandResult();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command result is missing in the context.");
        }

        [Fact]
        public void TryGetCommandResult_ReturnsTrue_WhenAvailable()
        {
            CommandResult commandResult = new();
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.CommandResult, commandResult } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetCommandResult(out CommandResult? result);
            found.Should().BeTrue();
            result.Should().Be(commandResult);
        }

        [Fact]
        public void TryGetCommandResult_ReturnsFalse_WhenMissing()
        {
            ICommandContext context = Context();
            bool found = context.TryGetCommandResult(out CommandResult? result);
            found.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void SetCommandResult_SetsValue()
        {
            CommandResult commandResult = new();
            ICommandContext context = Context(new Dictionary<string, object>());
            context.SetCommandResult(commandResult);
            context.Properties[TerminalIdentifiers.CommandResult].Should().Be(commandResult);
        }

        [Fact]
        public void GetRequiredOptionValue_Returns_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            int value = context.GetRequiredOptionValue<int>("opt1");
            value.Should().Be(123);
        }

        [Fact]
        public void GetRequiredOptionValue_Throws_WhenOptionsNull()
        {
            ITerminalTextHandler textHandler = new Mock<ITerminalTextHandler>().Object;
            CommandDescriptor commandDescriptor = new("id2", "name2", "desc2", CommandTypes.Leaf);
            Arguments arguments = new(textHandler, new List<Argument>());
            Command cmd = new(commandDescriptor, arguments, null);
            ParsedCommand parsedCmd = new(cmd, null);
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCmd } };
            ICommandContext context = Context(properties);
            Action act = () => context.GetRequiredOptionValue<int>("opt1");
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.UnsupportedOption);
        }

        [Fact]
        public void TryGetOptionValue_ReturnsTrue_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetOptionValue<int>("opt1", out int value);
            found.Should().BeTrue();
            value.Should().Be(123);
        }

        [Fact]
        public void TryGetOptionValue_ReturnsFalse_WhenNotAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetOptionValue<int>("notfound", out int value);
            found.Should().BeFalse();
        }

        [Fact]
        public void GetRequiredArgumentValue_ById_Returns_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            int value = context.GetRequiredArgumentValue<int>("arg1");
            value.Should().Be(42);
        }

        [Fact]
        public void GetRequiredArgumentValue_ByIndex_Returns_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            int value = context.GetRequiredArgumentValue<int>(0);
            value.Should().Be(42);
        }

        [Fact]
        public void TryGetArgumentValue_ReturnsTrue_WhenAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetArgumentValue<int>("arg1", out int value);
            found.Should().BeTrue();
            value.Should().Be(42);
        }

        [Fact]
        public void TryGetArgumentValue_ReturnsFalse_WhenNotAvailable()
        {
            Dictionary<string, object> properties = new() { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            ICommandContext context = Context(properties);
            bool found = context.TryGetArgumentValue<int>("notfound", out int value);
            found.Should().BeFalse();
        }
    }
}
