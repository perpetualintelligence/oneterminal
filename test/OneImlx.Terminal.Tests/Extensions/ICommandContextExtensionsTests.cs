//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Tests.Extensions
{
    public class ICommandContextExtensionsTests
    {
        private readonly Command command;
        private readonly ParsedCommand parsedCommand;
        private readonly ITerminalTextHandler textHandler;

        public ICommandContextExtensionsTests()
        {
            textHandler = new Mock<ITerminalTextHandler>().Object;
            var optionDescriptor = new OptionDescriptor("opt1", "System.Int32", "Option 1", 0, null);
            var options = new Options(textHandler, new[] { new Option(optionDescriptor, 123) });
            var argumentDescriptor = new ArgumentDescriptor(0, "arg1", "System.Int32", "Argument 1", 0);
            var arguments = new Arguments(textHandler, new[] { new Argument(argumentDescriptor, 42) });
            command = new Command(new CommandDescriptor("id1", "name1", "desc1", CommandTypes.Leaf), arguments, options);
            parsedCommand = new ParsedCommand(command, null);
        }

        [Fact]
        public void GetParsedCommand_ReturnsParsedCommand_WhenAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.GetParsedCommand().Should().Be(parsedCommand);
        }

        [Fact]
        public void GetParsedCommand_ThrowsTerminalException_WhenNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            mockContext.Setup(x => x.Properties).Returns([]);
            var act = () => mockContext.Object.GetParsedCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The parsed command is missing in the context.");
        }

        [Fact]
        public void GetParsedCommand_ThrowsTerminalException_WhenWrongType()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, "wrong type" } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            var act = () => mockContext.Object.GetParsedCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The parsed command is missing in the context.");
        }

        [Fact]
        public void GetCommand_ReturnsCommand_WhenParsedCommandAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.GetCommand().Should().Be(command);
        }

        [Fact]
        public void GetCommand_ThrowsTerminalException_WhenParsedCommandNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            mockContext.Setup(x => x.Properties).Returns([]);
            var act = () => mockContext.Object.GetCommand();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The parsed command is missing in the context.");
        }

        [Fact]
        public void GetCommandResult_ReturnsCommandResult_WhenAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var commandResult = new CommandResult();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.CommandResult, commandResult } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.GetCommandResult().Should().Be(commandResult);
        }

        [Fact]
        public void GetCommandResult_ThrowsTerminalException_WhenNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            mockContext.Setup(x => x.Properties).Returns([]);
            var act = () => mockContext.Object.GetCommandResult();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command result is missing in the context.");
        }

        [Fact]
        public void GetCommandResult_ThrowsTerminalException_WhenWrongType()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.CommandResult, "wrong type" } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            var act = () => mockContext.Object.GetCommandResult();
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command result is missing in the context.");
        }

        [Fact]
        public void TryGetCommandResult_ReturnsTrue_WhenCommandResultAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var commandResult = new CommandResult();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.CommandResult, commandResult } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.TryGetCommandResult(out var outResult).Should().BeTrue();
            outResult.Should().Be(commandResult);
        }

        [Fact]
        public void TryGetCommandResult_ReturnsFalse_WhenCommandResultNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            mockContext.Setup(x => x.Properties).Returns([]);
            mockContext.Object.TryGetCommandResult(out var outResult).Should().BeFalse();
        }

        [Fact]
        public void TryGetParsedCommand_ReturnsTrue_WhenParsedCommandAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.TryGetParsedCommand(out var outResult).Should().BeTrue();
            outResult.Should().Be(parsedCommand);
        }

        [Fact]
        public void TryGetParsedCommand_ReturnsFalse_WhenParsedCommandNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            mockContext.Setup(x => x.Properties).Returns([]);
            mockContext.Object.TryGetParsedCommand(out var outResult).Should().BeFalse();
        }

        [Fact]
        public void SetParsedCommand_AddsParsedCommandToProperties()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object>();
            mockContext.Setup(x => x.Properties).Returns(properties);
            mockContext.Object.SetParsedCommand(parsedCommand);
            properties.Should().ContainKey(TerminalIdentifiers.ParsedCommand);
            properties[TerminalIdentifiers.ParsedCommand].Should().Be(parsedCommand);
        }

        [Fact]
        public void SetParsedCommand_OverwritesExistingParsedCommand()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            var newParsedCommand = new ParsedCommand(command, null);
            mockContext.Object.SetParsedCommand(newParsedCommand);
            properties[TerminalIdentifiers.ParsedCommand].Should().Be(newParsedCommand);
        }

        [Fact]
        public void SetCommandResult_AddsCommandResultToProperties()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object>();
            mockContext.Setup(x => x.Properties).Returns(properties);
            var commandResult = new CommandResult();
            mockContext.Object.SetCommandResult(commandResult);
            properties.Should().ContainKey(TerminalIdentifiers.CommandResult);
            properties[TerminalIdentifiers.CommandResult].Should().Be(commandResult);
        }

        [Fact]
        public void SetCommandResult_OverwritesExistingCommandResult()
        {
            var mockContext = new Mock<ICommandContext>();
            var commandResult = new CommandResult();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.CommandResult, commandResult } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            var newCommandResult = new CommandResult();
            mockContext.Object.SetCommandResult(newCommandResult);
            properties[TerminalIdentifiers.CommandResult].Should().Be(newCommandResult);
        }

        [Fact]
        public void GetRequiredOptionValue_ReturnsOptionValue_WhenAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            int value = mockContext.Object.GetRequiredOptionValue<int>("opt1");
            value.Should().Be(123);
        }

        [Fact]
        public void GetRequiredOptionValue_ThrowsTerminalException_WhenOptionsNull()
        {
            var textHandler = new Mock<ITerminalTextHandler>().Object;
            var cmd = new Command(new CommandDescriptor("id2", "name2", "desc2", CommandTypes.Leaf), new Arguments(textHandler, new List<Argument>()), null);
            var mockContext = new Mock<ICommandContext>();
            var parsedCmd = new ParsedCommand(cmd, null);
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCmd } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            var act = () => mockContext.Object.GetRequiredOptionValue<int>("opt1");
            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.UnsupportedOption);
        }

        [Fact]
        public void TryGetOptionValue_ReturnsFalse_WhenOptionNotAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            bool found = mockContext.Object.TryGetOptionValue<int>("notfound", out var value);
            found.Should().BeFalse();
        }

        [Fact]
        public void TryGetOptionValue_ReturnsTrue_WhenOptionAvailable()
        {
            var mockContext = new Mock<ICommandContext>();
            var properties = new Dictionary<string, object> { { TerminalIdentifiers.ParsedCommand, parsedCommand } };
            mockContext.Setup(x => x.Properties).Returns(properties);
            bool found = mockContext.Object.TryGetOptionValue<int>("opt1", out var value);
            found.Should().BeTrue();
            value.Should().Be(123);
        }
    }
}