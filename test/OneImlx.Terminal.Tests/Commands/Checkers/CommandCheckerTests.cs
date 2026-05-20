//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class CommandCheckerTests
    {
        public CommandCheckerTests()
        {
            LoggerFactory loggerFactory = new();

            request = new CommandRequest(Guid.NewGuid().ToString(), "test_raw");
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);
            optionMapper = new DataTypeMapper<Option>(terminalOptions, loggerFactory.CreateLogger<DataTypeMapper<Option>>());
            argumentMapper = new DataTypeMapper<Argument>(terminalOptions, loggerFactory.CreateLogger<DataTypeMapper<Argument>>());
            valueChecker = new OptionChecker(optionMapper, terminalOptions);
            argumentChecker = new ArgumentChecker(argumentMapper, terminalOptions);
            checker = new CommandChecker(valueChecker, argumentChecker, terminalOptions, loggerFactory.CreateLogger<CommandChecker>());
            commands = new TerminalInMemoryCommandStore(textHandler, MockCommands.Commands.Values);
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            routerContext = new MockTerminalRouterContext(TerminalStartMode.Custom, commandTokenSource.Token);
            commandContext = new CommandContext(request, routerContext, []);
        }

        [Fact]
        public async Task DisabledOptionShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.Disabled);
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "value1")]);

            Command argsCommand = new(disabledArgsDescriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            Func<Task> func = () => checker.CheckCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option is disabled. command=id1 option=key1");
        }

        [Fact]
        public async Task EnabledOptionShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "value1")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            await checker.CheckCommandAsync(commandContext);
        }

        [Fact]
        public async Task ObsoleteOptionAndObsoleteAllowedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.Obsolete);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "value1")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            terminalOptions.Checker.AllowObsolete = true;
            await checker.CheckCommandAsync(commandContext);
        }

        [Fact]
        public async Task ObsoleteOptionAndObsoleteNotAllowedShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.Obsolete);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "value1")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            terminalOptions.Checker.AllowObsolete = false;
            Func<Task> func = async () => await checker.CheckCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option is obsolete. command=id1 option=key1");
        }

        [Fact]
        public async Task RequiredOptionMissingShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.Required);
            OptionDescriptor optionDescriptor2 = new("key2", nameof(String), "desc1", BehaviorFlags.Required);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor2, "value2")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            Func<Task> func = async () => await checker.CheckCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.MissingOption).WithErrorDescription("The required option is missing. command=id1 option=key1");
        }

        [Fact]
        public async Task RequiredOptionPassedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", BehaviorFlags.Required);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "value1")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            await checker.CheckCommandAsync(commandContext);
        }

        [Fact]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            terminalOptions.Checker.ValueDataType = false;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "non-date")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            await checker.CheckCommandAsync(commandContext);

            options["key1"].Value.Should().Be("non-date");
        }

        [Fact]
        public async Task StrictTypeCheckingValueDelimiterValidValueTypeShouldErrorAsync()
        {
            terminalOptions.Checker.ValueDataType = true;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "non-date")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            Func<Task> func = async () => await checker.CheckCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value does not match the mapped type. option=key1 type=System.DateTime data_type=DateTime value_type=String value=non-date");
        }

        [Fact]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            terminalOptions.Checker.ValueDataType = true;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, "25-Mar-2021")]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            object oldValue = options["key1"].Value;
            oldValue.Should().BeOfType<string>();

            // Value should pass and converted to date
            await checker.CheckCommandAsync(commandContext);

            object newValue = options["key1"].Value;
            newValue.Should().BeOfType<DateTime>();
            ((DateTime)newValue).ToString("dd-MMM-yyyy").Should().Be((string)oldValue);
        }

        [Fact]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(Double), "test desc", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, 25.36)]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            var result = await checker.CheckCommandAsync(commandContext);
        }

        [Fact]
        public async Task ValueValidShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", BehaviorFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandTypes.Leaf, optionDescriptors: new(textHandler, [optionDescriptor]));

            Options options = new(textHandler, [new(optionDescriptor, DateTime.Now)]);

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(argsCommand, null);
            commandContext.SetParsedCommand(extractedCommand);

            await checker.CheckCommandAsync(commandContext);
        }

        private readonly IArgumentChecker argumentChecker = null!;
        private readonly IDataTypeMapper<Argument> argumentMapper = null!;
        private readonly CommandChecker checker = null!;
        private readonly ITerminalCommandStore commands = null!;
        private readonly CancellationTokenSource commandTokenSource = null!;
        private readonly IDataTypeMapper<Option> optionMapper = null!;
        private readonly CommandRequest request = null!;
        private readonly CommandContext commandContext = null!;
        private readonly TerminalRouterContext routerContext = null!;
        private readonly TerminalOptions terminalOptions = null!;
        private readonly CancellationTokenSource terminalTokenSource = null!;
        private readonly ITerminalTextHandler textHandler = null!;
        private readonly IOptionChecker valueChecker = null!;
    }
}