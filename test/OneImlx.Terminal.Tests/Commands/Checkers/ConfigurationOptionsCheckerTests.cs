﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    [TestClass]
    public class ConfigurationOptionsCheckerTests
    {
        public ConfigurationOptionsCheckerTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            textHandler = new UnicodeTextHandler();

            hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton(textHandler);
                services.AddSingleton(options);
            });
            host = hostBuilder.Start();

            optionsChecker = new ConfigurationOptionsChecker(host.Services);
        }

        [TestMethod]
        public async Task Terminal_Id_Is_Required()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            options.Id = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Id = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

            options.Id = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

            options.Id = "asasd";
            await func.Invoke();
        }

        [TestMethod]
        public async Task LinkedToRoot_Requires_Terminal_Name()
        {
            options.Driver.Enabled = true;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            options.Driver.Name = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            options.Driver.Name = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            options.Driver.Name = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            options.Driver.Enabled = false;

            options.Driver.Name = "";
            await func.Invoke();

            options.Driver.Name = null;
            await func.Invoke();

            options.Driver.Name = "   ";
            await func.Invoke();

            options.Driver.Enabled = null;

            options.Driver.Name = "";
            await func.Invoke();

            options.Driver.Name = null;
            await func.Invoke();

            options.Driver.Name = "   ";
            await func.Invoke();
        }

        [TestMethod]
        public async Task OptionAliasPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Parser.OptionAliasPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option alias prefix cannot be null or whitespace.");

            options.Parser.OptionAliasPrefix = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option alias prefix cannot be null or whitespace.");

            options.Parser.OptionAliasPrefix = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option alias prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणू")]
        [DataRow("女性")]
        public async Task OptionAliasPrefixCannotStartWithOptionPrefix(string prefix)
        {
            options.Parser.OptionPrefix = prefix;
            options.Parser.OptionAliasPrefix = $"{prefix}:";

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option alias prefix cannot start with option prefix. prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("@@@@")]
        [DataRow("~~~~")]
        [DataRow("####")]
        [DataRow("sp----")]
        [DataRow("öööö")]
        [DataRow("माणूसस")]
        [DataRow("女性女性")]
        [DataRow("-女माö")]
        public async Task OptionAliasPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Parser.OptionAliasPrefix = prefix;
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option alias prefix cannot be more than 3 Unicode characters. option_alias_prefix={prefix}");
        }

        [TestMethod]
        public async Task OptionPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Parser.OptionPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option prefix cannot be null or whitespace.");

            options.Parser.OptionPrefix = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option prefix cannot be null or whitespace.");

            options.Parser.OptionPrefix = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@@@@")]
        [DataRow("~~~~")]
        [DataRow("####")]
        [DataRow("sp----")]
        [DataRow("öööö")]
        [DataRow("माणूसस")]
        [DataRow("女性女性")]
        [DataRow("-女माö")]
        public async Task OptionPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Parser.OptionPrefix = prefix;
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option prefix cannot be more than 3 Unicode characters. option_prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task OptionSeparatorAndOptionAliasPrefixCannotBeSameAsync(string separator)
        {
            options.Parser.OptionValueSeparator = separator;
            options.Parser.OptionPrefix = ":";
            options.Parser.OptionAliasPrefix = separator;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option separator and option alias prefix cannot be same. separator={separator}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task OptionSeparatorAndOptionPrefixCannotBeSameAsync(string separator)
        {
            options.Parser.OptionValueSeparator = separator;
            options.Parser.OptionPrefix = separator;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option separator and option prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task OptionSeparatorCannotBeNullOrEmptyAsync()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Parser.OptionValueSeparator = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option separator cannot be null or empty.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Parser.OptionValueSeparator = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option separator cannot be null or empty.");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndOptionPrefixCannotBeSameAsync(string separator)
        {
            options.Parser.Separator = separator;
            options.Parser.OptionPrefix = separator;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The command separator and option prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task CommandSeparatorCannotBeNullOrEmptyAsync()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Parser.Separator = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The command separator cannot be null or empty.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Parser.Separator = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The command separator cannot be null or empty.");
        }

        [TestMethod]
        public async Task ValueDelimiterStringCannotBeSameAsArgAliasPrefix()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.OptionPrefix = "#";
            options.Parser.ValueDelimiter = "^";
            options.Parser.OptionAliasPrefix = "^";

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the option alias prefix. delimiter=^");
        }

        [TestMethod]
        public async Task ValueDelimiterStringCannotBeSameAsArgPrefix()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.OptionPrefix = "^";
            options.Parser.ValueDelimiter = "^";

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the option prefix. delimiter=^");
        }

        [TestMethod]
        public async Task ValueDelimiterStringCannotBeSameAsArgSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.OptionValueSeparator = "^";
            options.Parser.ValueDelimiter = "^";

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the option value separator. delimiter=^");
        }

        [TestMethod]
        public async Task ValueDelimiterStringCannotBeSameAsSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.Separator = "^";
            options.Parser.ValueDelimiter = "^";

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the separator. delimiter=^");
        }

        [TestMethod]
        public async Task ValueDelimiterStringCannotBeNullOrWhitespace()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.ValueDelimiter = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be null or whitespace.");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Parser.ValueDelimiter = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be null or whitespace.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private readonly IHost host;
        private readonly IHostBuilder hostBuilder;
        private readonly TerminalOptions options;
        private readonly IConfigurationOptionsChecker optionsChecker;
        private readonly ITextHandler textHandler;
    }
}