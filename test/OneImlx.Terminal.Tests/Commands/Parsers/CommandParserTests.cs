﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class CommandParserTests
    {
        public CommandParserTests()
        {
            textHandler = new TerminalAsciiTextHandler();

            ArgumentDescriptors arguments = new(textHandler,
            [
                new (1, "arg1", nameof(String), "arg1_desc", ArgumentFlags.None),
                new (2, "arg2", nameof(String), "arg2_desc", ArgumentFlags.None)
            ]);

            OptionDescriptors options = new(textHandler,
            [
                new ("opt1", nameof(String), "opt1_desc", OptionFlags.None),
                new ("opt2", nameof(Int32), "opt2_desc", OptionFlags.None),
                new ("opt3", nameof(Boolean), "opt3_desc", OptionFlags.None, alias: "o3"),
                new ("opt4", nameof(Double), "opt4_desc", OptionFlags.None, alias: "o4")

            ]);

            commandDescriptors = new(textHandler,
            [
                new("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None),
                new("root2", "root2_name", "root2_desc", CommandType.Root, CommandFlags.None),
                new("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("root1"), argumentDescriptors: arguments),
                new("grp2", "grp2_name", "grp2_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("root2")),
                new("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp1")),
                new("cmd2", "cmd2_name", "cmd2_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp2"), argumentDescriptors: arguments,  optionDescriptors: options),
                new("cmd_nr1", "cmd_nr1_name", "cmd_nr1_desc", CommandType.SubCommand, CommandFlags.None),
                new("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None)
            ]);
            commandStore = new TerminalInMemoryCommandStore(textHandler, commandDescriptors.Values);

            terminalOptions = MockTerminalOptions.NewAliasOptions();
            var terminalIOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(terminalOptions);

            logger = new LoggerFactory().CreateLogger<CommandParser>();
            requestParser = new TerminalRequestQueueParser(textHandler, terminalIOptions, new LoggerFactory().CreateLogger<TerminalRequestQueueParser>());
            parser = new CommandParser(requestParser, textHandler, commandStore, terminalIOptions, logger);
        }

        [Fact]
        public async Task Arguments_And_Options_Processed_Correctly()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = ' ';

            var result = await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2 grp2 cmd2 arg1 arg2 --opt1 val1 --opt2 23 -o3 -o4 36.69")));
            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("cmd2");

            result.ParsedCommand.Hierarchy.Should().NotBeNull();
            result.ParsedCommand.Hierarchy.Should().HaveCount(2);
            result.ParsedCommand.Hierarchy!.ElementAt(0).Id.Should().Be("root2");
            result.ParsedCommand.Hierarchy!.ElementAt(1).Id.Should().Be("grp2");

            result.ParsedCommand.Command.Arguments.Should().HaveCount(2);
            result.ParsedCommand.Command.Arguments![0].Id.Should().Be("arg1");
            result.ParsedCommand.Command.Arguments[1].Id.Should().Be("arg2");

            result.ParsedCommand.Command.Options.Should().HaveCount(6);
            result.ParsedCommand.Command.Options!["opt1"].Value.Should().Be("val1");
            result.ParsedCommand.Command.Options["opt1"].ByAlias.Should().BeFalse();

            result.ParsedCommand.Command.Options["opt2"].Value.Should().Be("23");
            result.ParsedCommand.Command.Options["opt2"].ByAlias.Should().BeFalse();

            result.ParsedCommand.Command.Options["opt3"].Value.Should().Be("True");
            result.ParsedCommand.Command.Options["opt3"].ByAlias.Should().BeTrue();

            result.ParsedCommand.Command.Options["opt4"].Value.Should().Be("36.69");
            result.ParsedCommand.Command.Options["opt4"].ByAlias.Should().BeTrue();

            result.ParsedCommand.Command.Options["o3"].Value.Should().Be("True");
            result.ParsedCommand.Command.Options["o3"].ByAlias.Should().BeTrue();

            result.ParsedCommand.Command.Options["o4"].Value.Should().Be("36.69");
            result.ParsedCommand.Command.Options["o4"].ByAlias.Should().BeTrue();
        }

        [Fact]
        public async Task No_Options_Processes_Correctly()
        {
            var result = await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root1 grp1 arg1 arg2")));
            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("grp1");
            result.ParsedCommand.Command.Arguments.Should().HaveCount(2);
            result.ParsedCommand.Command.Arguments![0].Id.Should().Be("arg1");
            result.ParsedCommand.Command.Arguments[1].Id.Should().Be("arg2");
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Options_Processed_Correctly()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            Dictionary<string, ValueTuple<string, bool>> options = new()
            {
                ["opt1"] = new("val1", false),
                ["opt2"] = new(23.ToString(), false),
                ["opt3"] = new(true.ToString(), false),
                ["opt4"] = new(36.69.ToString(), false)
            };

            var result = await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 --opt3 --opt4 36.69")));
            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("cmd2");
            result.ParsedCommand.Command.Arguments.Should().BeNull();

            result.ParsedCommand.Command.Options.Should().HaveCount(6);
            result.ParsedCommand.Command.Options!["opt1"].Value.Should().Be("val1");
            result.ParsedCommand.Command.Options["opt2"].Value.Should().Be("23");
            result.ParsedCommand.Command.Options["opt3"].Value.Should().Be("True");
            result.ParsedCommand.Command.Options["opt4"].Value.Should().Be("36.69");

            result.ParsedCommand.Command.Options["o3"].Value.Should().Be("True");
            result.ParsedCommand.Command.Options["o4"].Value.Should().Be("36.69");
        }

        [Fact]
        public async Task Request_Is_Set_In_Result()
        {
            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1");
            var result = await parser.ParseCommandAsync(new(request));
            result.Should().NotBeNull();

            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("root1");
            result.ParsedCommand.Hierarchy.Should().BeNull();
            result.ParsedCommand.Request.Should().BeSameAs(request);
        }

        [Fact]
        public async Task Root_Processed_Correctly()
        {
            var result = await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2")));
            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("root2");
            result.ParsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Root);

            result.ParsedCommand.Hierarchy.Should().BeNull();
            result.ParsedCommand.Command.Arguments.Should().BeNull();
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Single_Non_Root_Processed_Correctly()
        {
            var result = await parser.ParseCommandAsync(new(new TerminalRequest("id1", "cmd_nr1")));
            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("cmd_nr1");
            result.ParsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            result.ParsedCommand.Hierarchy.Should().BeNull();
            result.ParsedCommand.Command.Arguments.Should().BeNull();
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Throws_If_Alias_Prefix_Is_Specified_For_Option()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            Func<Task> act = async () => await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 -opt3 --opt4 36.69")));
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The alias prefix is not valid for an option. alias=o3 option=opt3");
        }

        [Fact]
        public async Task Throws_If_Arguments_Found_Without_Command()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "arg1 arg2 arg3"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The arguments were provided, but no command was found or specified.");
        }

        [Fact]
        public async Task Throws_If_Command_Does_Not_Define_An_Owner()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root1 root2 grp2"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command does not define an owner. command=root2");
        }

        [Fact]
        public async Task Throws_If_Command_Found_But_Returns_Null_Descriptor()
        {
            var parserMock = new Mock<ITerminalRequestParser>();
            parserMock.Setup(x => x.ParseRequestAsync(It.IsAny<TerminalRequest>()))
                      .ReturnsAsync((TerminalRequest request) =>
                      {
                          return new TerminalParsedRequest(["root1", "grp1", "cmd1"], []);
                      });

            Mock<ITerminalCommandStore> storeMock = new();
            CommandDescriptor? commandDescriptor = null;
            storeMock.Setup(x => x.TryFindByIdAsync(It.IsAny<string>(), out commandDescriptor))
                     .ReturnsAsync(true);

            var iOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(terminalOptions);
            parser = new CommandParser(parserMock.Object, textHandler, storeMock.Object, iOptions, logger);

            var context = new CommandParserContext(new TerminalRequest("id1", "root1 grp1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command is found in the store but returned null descriptor. command=root1");
        }

        [Fact]
        public async Task Throws_If_Command_Is_Present_In_Arguments()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root1 grp1 arg1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_argument")
                .WithErrorDescription("The command is found in arguments. command=cmd1");
        }

        [Fact]
        public async Task Throws_If_Commands_Are_Duplicated()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root1 grp1 cmd1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command owner is not valid. owner=cmd1 command=cmd1");
        }

        [Fact]
        public async Task Throws_If_More_Than_Supported_Arguments()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root1 grp1 arg1 arg2 arg3"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_argument")
                .WithErrorDescription("The command does not support 3 arguments. command=grp1");
        }

        [Fact]
        public async Task Throws_If_No_Root_Is_Not_Specified()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "grp1 cmd1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command owner is missing. command=grp1");
        }

        [Fact]
        public async Task Throws_If_Option_Is_Unsupported()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            Func<Task> act = async () => await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2 grp2 cmd2 --opt1 val1 --invalid_opt1 23 --opt3 --opt4 36.69")));
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_option")
                .WithErrorDescription("The command does not support option or its alias. command=cmd2 option=invalid_opt1");
        }

        [Fact]
        public async Task Throws_If_Option_Prefix_Is_Specified_For_Alias()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            Func<Task> act = async () => await parser.ParseCommandAsync(new(new TerminalRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 --o3 --opt4 36.69")));
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The option prefix is not valid for an alias. option=opt3 alias=o3");
        }

        [Fact]
        public async Task Throws_If_Owner_Is_Invalid()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root2 grp1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command owner is not valid. owner=root2 command=grp1");
        }

        [Fact]
        public async Task Throws_If_Owner_Is_Missing()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "grp1 cmd1"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command owner is missing. command=grp1");
        }

        [Fact]
        public async Task Throws_If_Unsupported_Arguments()
        {
            var context = new CommandParserContext(new TerminalRequest("id1", "root1 grp1 cmd1 arg1 arg2"));
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_argument")
                .WithErrorDescription("The command does not support arguments. command=cmd1");
        }

        private readonly CommandDescriptors commandDescriptors;
        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<CommandParser> logger;
        private readonly ITerminalRequestParser requestParser;
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private CommandParser parser;
    }
}
