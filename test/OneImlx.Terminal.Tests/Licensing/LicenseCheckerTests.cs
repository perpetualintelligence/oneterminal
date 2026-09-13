//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseCheckerTests
    {
        public LicenseCheckerTests()
        {
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.UTF8);
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            commandStore = new TerminalInMemoryCommandStore(MockCommands.LicensingCommands.TextHandler, MockCommands.LicensingCommands.Values);
            licenseChecker = new LicenseChecker(commandStore, textHandler, terminalOptions, new LoggerFactory().CreateLogger<LicenseChecker>());
            license = new License(ProductCatalog.TerminalPlanCorporate, LicenseUsage.RnD, "testLicKey2", MockLicenses.TestClaims, LicenseQuota.Create(ProductCatalog.TerminalPlanCorporate));
        }

        [Fact]
        public async Task CheckAsync_DeclarationsCheck_ShouldBehaveCorrectly()
        {
            // Error, custom declarations enabled but not allowed by plan
            var features = new Dictionary<string, string[]>(license.Quota.Features) { ["declarations"] = new[] { "standard" } };
            license.Quota.Features = features;
            terminalOptions.Descriptor.CustomDeclarations = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal declaration `custom` is not allowed for your license plan.");

            // No error, custom not allowed and not configured
            features["declarations"] = new[] { "standard" };
            license.Quota.Features = features;
            terminalOptions.Descriptor.CustomDeclarations = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, custom allowed and not configured
            features["declarations"] = new[] { "standard", "custom" };
            license.Quota.Features = features;
            terminalOptions.Descriptor.CustomDeclarations = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, custom allowed and configured
            features["declarations"] = new[] { "standard", "custom" };
            license.Quota.Features = features;
            terminalOptions.Descriptor.CustomDeclarations = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_DriverCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches) { ["driver"] = false };
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal driver is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["driver"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["driver"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["driver"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Driver.Enabled = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_DynamicsCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches) { ["dynamics"] = false };
            license.Quota.Switches = switches;
            terminalOptions.Runtime.CommandSourceEnabled = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal dynamics is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["dynamics"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Runtime.CommandSourceEnabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["dynamics"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Runtime.CommandSourceEnabled = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["dynamics"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Runtime.CommandSourceEnabled = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_EncodingCheck_ShouldBehaveCorrectly()
        {
            // Error, configured encoding not in allowed list
            var features = new Dictionary<string, string[]>(license.Quota.Features) { ["encodings"] = new[] { "ascii" } };
            license.Quota.Features = features;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal encoding `utf8` is not allowed for your license plan.");

            // No error, configured encoding in allowed list
            features["encodings"] = new[] { "ascii", "utf8", "utf16", "utf32" };
            license.Quota.Features = features;
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_ExceededCommandLimit_ShouldError()
        {
            // Commands are 11
            var limits = new Dictionary<string, object?>(license.Quota.Limits) { ["commands"] = 2 };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The command limit exceeded. max_limit=2 current=11");
        }

        [Fact]
        public async Task CheckAsync_ExceededInputLimit_ShouldError()
        {
            // Total options 90
            var limits = new Dictionary<string, object?>(license.Quota.Limits) { ["inputs"] = 89 };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The input limit exceeded. max_limit=89 current=90");
        }

        [Fact]
        public async Task CheckAsync_ExceededTerminalLimit_ShouldError()
        {
            // TODO For now the terminal count are 1 always
            var limits = new Dictionary<string, object?>(license.Quota.Limits) { ["terminals"] = 0 };
            license.Quota.Limits = limits;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal limit exceeded. max_limit=0 current=1");
        }

        [Fact]
        public async Task CheckAsync_Failure_Sets_Failed()
        {
            // Commands are 11
            var limits = new Dictionary<string, object?>(license.Quota.Limits) { ["commands"] = 2 };
            license.Quota.Limits = limits;

            Exception? ex = null;
            try
            {
                await licenseChecker.CheckLicenseAsync(license);
            }
            catch (Exception caught)
            {
                ex = caught;
                license.Failed.Should().NotBeNull();
                license.Failed.ErrorCode.Should().Be(TerminalErrors.InvalidLicense);
                license.Failed.FormatDescription().Should().Be("The command limit exceeded. max_limit=2 current=11");
            }

            ex.Should().NotBeNull("An exception during license check was expected.");
        }

        [Fact]
        public async Task CheckAsync_InitializeShouldSetPropertiesCorrectly()
        {
            // Use unlimited license so this will not fail.
            var result = await licenseChecker.CheckLicenseAsync(license);
            ((LicenseChecker)licenseChecker).Initialized.Should().Be(true);

            result.License.Should().NotBeNull();
            result.License.Should().BeSameAs(license);
            result.License.Failed.Should().BeNull();

            result.TerminalCount.Should().Be(1);
            result.CommandCount.Should().Be(11);
            result.InputCount.Should().Be(90);
        }

        [Fact]
        public async Task CheckAsync_OptionsValid_ShouldBehaveCorrectly()
        {
            // Valid value should not error
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_StoreCheck_ShouldBehaveCorrectly()
        {
            // Error, configured store not in allowed list
            var features = new Dictionary<string, string[]>(license.Quota.Features) { ["stores"] = new[] { "custom" } };
            license.Quota.Features = features;
            terminalOptions.Runtime.Store = "memory";
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The terminal store `memory` is not allowed for your license plan.");

            // No error, configured store in allowed list
            features["stores"] = new[] { "memory", "custom" };
            license.Quota.Features = features;
            terminalOptions.Runtime.Store = "memory";
            await licenseChecker.CheckLicenseAsync(license);
        }

        [Fact]
        public async Task CheckAsync_StrictTypeCheck_ShouldBehaveCorrectly()
        {
            // Error, not allowed but configured
            var switches = new Dictionary<string, bool>(license.Quota.Switches) { ["datatype"] = false };
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = true;
            Func<Task> func = async () => await licenseChecker.CheckLicenseAsync(license);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidLicense).WithErrorDescription("The strict option value type is not allowed for your license plan.");

            // No error, not allowed not configured
            switches["datatype"] = false;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed not configured
            switches["datatype"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = false;
            await licenseChecker.CheckLicenseAsync(license);

            // No error, allowed and configured
            switches["datatype"] = true;
            license.Quota.Switches = switches;
            terminalOptions.Checker.ValueDataType = true;
            await licenseChecker.CheckLicenseAsync(license);
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly License license;
        private readonly ILicenseChecker licenseChecker;
        private readonly TerminalOptions terminalOptions;
        private readonly TerminalTextHandler textHandler;
    }
}