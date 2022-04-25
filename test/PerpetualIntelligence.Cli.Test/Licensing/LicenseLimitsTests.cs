﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using Xunit;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicenseLimitsTests
    {
        [Fact]
        public void CommunityEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Community);

            limits.Plan.Should().Be(SaaSPlans.Community);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.ArgumentLimit.Should().Be(500);

            limits.ArgumentAlias.Should().Be(false);
            limits.DefaultArgument.Should().Be(false);
            limits.DefaultArgumentValue.Should().Be(false);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeNull();
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online" });
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Enterprise);

            limits.Plan.Should().Be(SaaSPlans.Enterprise);

            limits.TerminalLimit.Should().Be(5);
            limits.RedistributionLimit.Should().Be(5000);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(20);
            limits.SubCommandLimit.Should().Be(100);
            limits.ArgumentLimit.Should().Be(2000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online", "offline" });
        }

        [Fact]
        public void InvalidEdition_ShouldError()
        {
            try
            {
                LicenseLimits limits = LicenseLimits.Create("invalid_plan");
            }
            catch (Exception ex)
            {
                ErrorException eex = (ErrorException)ex;
                eex.Error.ErrorCode.Should().Be(Errors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The licensing for the SaaS plan is not supported. saas_plan=invalid_plan");
            }
        }

        [Fact]
        public void ISVEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISV);

            limits.Plan.Should().Be(SaaSPlans.ISV);

            limits.TerminalLimit.Should().Be(25);
            limits.RedistributionLimit.Should().Be(10000);
            limits.RootCommandLimit.Should().Be(5);
            limits.GroupedCommandLimit.Should().Be(50);
            limits.SubCommandLimit.Should().Be(250);
            limits.ArgumentLimit.Should().Be(5000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online", "offline", "byol" });
        }

        [Fact]
        public void ISVUEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISVU);

            limits.Plan.Should().Be(SaaSPlans.ISVU);

            limits.TerminalLimit.Should().Be(null);
            limits.RedistributionLimit.Should().Be(null);
            limits.RootCommandLimit.Should().Be(null);
            limits.GroupedCommandLimit.Should().Be(null);
            limits.SubCommandLimit.Should().Be(null);
            limits.ArgumentLimit.Should().Be(null);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online", "offline", "byol" });
        }

        [Fact]
        public void MicroEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Micro);

            limits.Plan.Should().Be(SaaSPlans.Micro);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.ArgumentLimit.Should().Be(500);

            limits.ArgumentAlias.Should().Be(false);
            limits.DefaultArgument.Should().Be(false);
            limits.DefaultArgumentValue.Should().Be(false);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeNull();
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online" });
        }

        [Fact]
        public void SMBEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.SMB);

            limits.Plan.Should().Be(SaaSPlans.SMB);

            limits.TerminalLimit.Should().Be(3);
            limits.RedistributionLimit.Should().Be(1000);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(10);
            limits.SubCommandLimit.Should().Be(50);
            limits.ArgumentLimit.Should().Be(1000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.UnicodeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in-memory", "json" });
            limits.Services.Should().BeEquivalentTo(new string[] { "default" });
            limits.Licensing.Should().BeEquivalentTo(new string[] { "online" });
        }
    }
}
