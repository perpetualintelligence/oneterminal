//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class CommandParserTests
    {
        public CommandParserTests()
        {
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);

            ArgumentDescriptors arguments = new(textHandler,
            [
                new (1, "arg1", nameof(String), "arg1_desc", BehaviorFlags.None),
                new (2, "arg2", nameof(String), "arg2_desc", BehaviorFlags.None)
            ]);

            OptionDescriptors options = new(textHandler,
            [
                new ("opt1", nameof(String), "opt1_desc", BehaviorFlags.None),
                new ("opt2", nameof(Int32), "opt2_desc", BehaviorFlags.None),
                new ("opt3", nameof(Boolean), "opt3_desc", BehaviorFlags.None, alias: "o3"),
                new ("opt4", nameof(Double), "opt4_desc", BehaviorFlags.None, alias: "o4")

            ]);

            commandDescriptors = new(textHandler,
            [
                new("root1", "root1_name", "root1_desc", CommandTypes.Root),
                new("root2", "root2_name", "root2_desc", CommandTypes.Root),
                new("root3", "root3_name", "root3_desc", CommandTypes.Root,  new OwnerIdCollection("root2")),
                new("grp1", "grp1_name", "grp1_desc", CommandTypes.IsolatedGroup,  new OwnerIdCollection("root1"), argumentDescriptors: arguments),
                new("grp2", "grp2_name", "grp2_desc", CommandTypes.IsolatedGroup,  new OwnerIdCollection("root2")),
                new("cmd1", "cmd1_name", "cmd1_desc", CommandTypes.Leaf,  new OwnerIdCollection("grp1")),
                new("cmd2", "cmd2_name", "cmd2_desc", CommandTypes.Leaf,  new OwnerIdCollection("grp2"), argumentDescriptors: arguments,  optionDescriptors: options),
                new("cmd_nr1", "cmd_nr1_name", "cmd_nr1_desc", CommandTypes.Leaf),
                new("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandTypes.Leaf)

            ]);
            commandStore = new TerminalInMemoryCommandStore(textHandler, commandDescriptors.Values);

            terminalOptions = MockTerminalOptions.NewAliasOptions();
            var terminalIOptions = Microsoft.Extensions.Options.Options.Create(terminalOptions);

            logger = new LoggerFactory().CreateLogger<CommandParser>();
            requestParser = new TerminalRequestQueueParser(textHandler, terminalIOptions, new LoggerFactory().CreateLogger<TerminalRequestQueueParser>());
            parser = new CommandParser(requestParser, textHandler, commandStore, terminalIOptions, logger);

            terminalContext = new Mock<TerminalCustomRouterContext>(TerminalStartMode.Custom, null!, null!).Object;
        }

        [Fact]
        public async Task Arguments_And_Options_Processed_Correctly()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = ' ';

            CommandRequest request = new("id1", "root2 grp2 cmd2 arg1 arg2 --opt1 val1 --opt2 23 -o3 -o4 36.69");
            ICommandContext context = new CommandContext(request, terminalContext, []);

            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
            parsedCommand!.Command.Id.Should().Be("cmd2");

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy.Should().HaveCount(2);
            parsedCommand.Hierarchy!.ElementAt(0).Id.Should().Be("root2");
            parsedCommand.Hierarchy!.ElementAt(1).Id.Should().Be("grp2");

            parsedCommand.Command.Arguments.Should().HaveCount(2);
            parsedCommand.Command.Arguments![0].CommandId.Should().Be("arg1");
            parsedCommand.Command.Arguments[1].CommandId.Should().Be("arg2");

            parsedCommand.Command.Options.Should().HaveCount(6);
            parsedCommand.Command.Options!["opt1"].Value.Should().Be("val1");
            parsedCommand.Command.Options["opt1"].ByAlias.Should().BeFalse();

            parsedCommand.Command.Options["opt2"].Value.Should().Be("23");
            parsedCommand.Command.Options["opt2"].ByAlias.Should().BeFalse();

            parsedCommand.Command.Options["opt3"].Value.Should().Be("True");
            parsedCommand.Command.Options["opt3"].ByAlias.Should().BeTrue();

            parsedCommand.Command.Options["opt4"].Value.Should().Be("36.69");
            parsedCommand.Command.Options["opt4"].ByAlias.Should().BeTrue();

            parsedCommand.Command.Options["o3"].Value.Should().Be("True");
            parsedCommand.Command.Options["o3"].ByAlias.Should().BeTrue();

            parsedCommand.Command.Options["o4"].Value.Should().Be("36.69");
            parsedCommand.Command.Options["o4"].ByAlias.Should().BeTrue();
        }

        [Fact]
        public async Task Hiearchy_Null_For_Root()
        {
            CommandRequest request = new(Guid.NewGuid().ToString(), "root1");
            ICommandContext context = new CommandContext(request, terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
            parsedCommand!.Command.Id.Should().Be("root1");
            parsedCommand.Hierarchy.Should().BeNull();

            context.Properties.ContainsKey(TerminalIdentifiers.CommandResult).Should().BeFalse();
        }

        [Fact]
        public async Task No_Options_Processes_Correctly()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = ' ';

            CommandRequest request = new("id1", "root1 grp1 arg1 arg2");
            ICommandContext context = new CommandContext(request, terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
            parsedCommand!.Command.Id.Should().Be("grp1");
            parsedCommand.Command.Arguments.Should().HaveCount(2);
            parsedCommand.Command.Arguments![0].CommandId.Should().Be("arg1");
            parsedCommand.Command.Arguments[1].CommandId.Should().Be("arg2");
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Options_Processed_Correctly()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            var context = new CommandContext(new CommandRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 --opt3 --opt4 36.69"), terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Command.Id.Should().Be("cmd2");
            parsedCommand.Command.Arguments.Should().BeNull();

            parsedCommand.Command.Options.Should().HaveCount(6);
            parsedCommand.Command.Options!["opt1"].Value.Should().Be("val1");
            parsedCommand.Command.Options["opt2"].Value.Should().Be("23");
            parsedCommand.Command.Options["opt3"].Value.Should().Be("True");
            parsedCommand.Command.Options["opt4"].Value.Should().Be("36.69");

            parsedCommand.Command.Options["o3"].Value.Should().Be("True");
            parsedCommand.Command.Options["o4"].Value.Should().Be("36.69");
        }

        [Fact]
        public async Task Root_Processed_Correctly()
        {
            var context = new CommandContext(new CommandRequest("id1", "root2"), terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
            parsedCommand!.Command.Id.Should().Be("root2");
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandTypes.Root);

            parsedCommand.Hierarchy.Should().BeNull();
            parsedCommand.Command.Arguments.Should().BeNull();
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Root_With_Owner_Does_Not_Throw()
        {
            // NOTE: This test should fail. But the root owner check is not performed at the parser level, it is
            // validated during the builder setup.
            CommandRequest request = new(Guid.NewGuid().ToString(), "root2 root3");
            ICommandContext context = new CommandContext(request, terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
        }

        [Fact]
        public async Task Single_Non_Root_Processed_Correctly()
        {
            var context = new CommandContext(new CommandRequest("id1", "cmd_nr1"), terminalContext, []);
            await parser.ParseCommandAsync(context);

            ParsedCommand parsedCommand = context.GetParsedCommand();
            parsedCommand.Should().NotBeNull();
            parsedCommand!.Command.Id.Should().Be("cmd_nr1");
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandTypes.Leaf);

            parsedCommand.Hierarchy.Should().BeNull();
            parsedCommand.Command.Arguments.Should().BeNull();
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Throws_If_Alias_Is_Unsupported()
        {
            var context = new CommandContext(new CommandRequest("id1", "root2 grp2 cmd2 --opt1 Val1 -invalid_alias 25 --opt3 --opt4 \"val2\" --opt5"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_option")
                .WithErrorDescription("The command does not support option or its alias. command=cmd2 option=invalid_alias");
        }

        [Fact]
        public async Task Throws_If_Alias_Prefix_Is_Specified_For_Option()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            var context = new CommandContext(new CommandRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 -opt3 --opt4 36.69"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The alias prefix is not valid for an option. alias=o3 option=opt3");
        }

        [Fact]
        public async Task Throws_If_Arguments_Found_Without_Command()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            var context = new CommandContext(new CommandRequest("id1", "arg1 arg2 arg3"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The arguments were provided, but no command was found or specified.");
        }

        [Fact]
        public async Task Throws_If_Command_Does_Not_Define_An_Owner()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 root2 grp2"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command does not define an owner. command=root2");
        }

        [Fact]
        public async Task Throws_If_Command_Found_But_Returns_Null_Descriptor()
        {
            var parserMock = new Mock<ITerminalRequestParser>();
            parserMock.Setup(x => x.ParseRequestAsync(It.IsAny<CommandRequest>()))
                      .ReturnsAsync((CommandRequest request) =>
                      {
                          return new TerminalParsedRequest(["root1", "grp1", "cmd1"], []);
                      });

            Mock<ITerminalCommandStore> storeMock = new();
            CommandDescriptor? commandDescriptor = null;
            storeMock.Setup(x => x.TryFindByIdAsync(It.IsAny<string>(), out commandDescriptor))
                     .ReturnsAsync(true);

            var iOptions = Microsoft.Extensions.Options.Options.Create(terminalOptions);
            parser = new CommandParser(parserMock.Object, textHandler, storeMock.Object, iOptions, logger);

            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 cmd1"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command is found in the store but returned null descriptor. command=root1");
        }

        [Fact]
        public async Task Throws_If_Command_Is_Present_In_Arguments()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 arg1 cmd1"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_argument")
                .WithErrorDescription("The command is found in arguments. command=cmd1");
        }

        [Fact]
        public async Task Throws_If_Commands_Are_Duplicated()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 cmd1 cmd1"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command owner is not valid. owner=cmd1 command=cmd1");
        }

        [Fact]
        public async Task Throws_If_More_Than_Supported_Arguments()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 arg1 arg2 arg3"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_argument")
                .WithErrorDescription("The command does not support 3 arguments. command=grp1");
        }

        [Fact]
        public async Task Throws_If_No_Root_Is_Not_Specified()
        {
            var context = new CommandContext(new CommandRequest("id1", "grp1 cmd1 cmd1"), terminalContext, []);
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

            var context = new CommandContext(new CommandRequest("id1", "root2 grp2 cmd2 --opt1 val1 --invalid_opt1 23 --opt3 --opt4 36.69"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_option")
                .WithErrorDescription("The command does not support option or its alias. command=cmd2 option=invalid_opt1");
        }

        [Fact]
        public async Task Throws_If_Option_Prefix_Is_Specified_For_Alias()
        {
            terminalOptions.Parser.OptionPrefix = '-';
            terminalOptions.Parser.OptionValueSeparator = TerminalIdentifiers.SpaceSeparator;

            Func<Task> act = async () => await parser.ParseCommandAsync(new CommandContext(new CommandRequest("id1", "root2 grp2 cmd2 --opt1 val1 --opt2 23 --o3 --opt4 36.69"), terminalContext, []));
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The option prefix is not valid for an alias. option=opt3 alias=o3");
        }

        [Fact]
        public async Task Throws_If_Owner_Is_Invalid()
        {
            var context = new CommandContext(new CommandRequest("id1", "root2 grp1 cmd1"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_command")
                .WithErrorDescription("The command owner is not valid. owner=root2 command=grp1");
        }

        [Fact]
        public async Task Throws_If_Owner_Is_Missing()
        {
            var context = new CommandContext(new CommandRequest("id1", "grp1 cmd1"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("missing_command")
                .WithErrorDescription("The command owner is missing. command=grp1");
        }

        [Fact]
        public async Task Throws_If_Unsupported_Arguments()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 cmd1 arg1 arg2"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_argument")
                .WithErrorDescription("The command does not support arguments. command=cmd1");
        }

        [Fact]
        public async Task Throws_If_Unsupported_Options()
        {
            var context = new CommandContext(new CommandRequest("id1", "root1 grp1 cmd1 --opt1 val1 --opt2 23"), terminalContext, []);
            Func<Task> act = async () => await parser.ParseCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("unsupported_option")
                .WithErrorDescription("The command does not support options. command=cmd1");
        }

        public readonly TerminalRouterContext terminalContext;
        private readonly CommandDescriptors commandDescriptors;
        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<CommandParser> logger;
        private readonly ITerminalRequestParser requestParser;
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private CommandParser parser;
    }
}