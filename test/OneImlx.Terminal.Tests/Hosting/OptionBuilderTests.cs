//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class OptionBuilderTests : IDisposable
    {
        public OptionBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Build_Adds_OptionDescriptor_To_CommandDescriptor()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandTypes.Leaf).Checker<MockCommandChecker>();

            commandBuilder.DefineOption("opt1", nameof(String), "test opt desc1", ReservedFlags.None).Add()
                          .DefineOption("opt2", nameof(String), "test opt desc2", ReservedFlags.None).Add()
                          .DefineOption("opt3", nameof(String), "test opt desc3", ReservedFlags.None).Add();

            ServiceProvider serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var optDescriptors = serviceProvider.GetServices<OptionDescriptor>();
            optDescriptors.Count().Should().Be(3);
            optDescriptors.Should().Contain(static x => x.Id == "opt1");
            optDescriptors.Should().Contain(static x => x.Id == "opt2");
            optDescriptors.Should().Contain(static x => x.Id == "opt3");
        }

        [Fact]
        public void Build_Returns_Same_CommandBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandTypes.Leaf).Checker<MockCommandChecker>();

            IOptionBuilder optionBuilder = commandBuilder.DefineOption("opt1", nameof(String), "test opt desc1", ReservedFlags.None);
            ICommandBuilder cmdBuilderFromArgBuilder = optionBuilder.Add();
            commandBuilder.Should().BeSameAs(cmdBuilderFromArgBuilder);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            CommandBuilder commandBuilder = new(terminalBuilder);
            OptionBuilder argumentBuilder = new(commandBuilder);

            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        [Fact]
        public void Nos_OptionDescriptor_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandTypes.Leaf).Checker<MockCommandChecker>();

            OptionBuilder optionBuilder = new(commandBuilder);
            Action act = () => optionBuilder.Add();
            act.Should().Throw<TerminalException>().WithMessage("The option builder is missing an option descriptor.");
        }

        [Fact]
        public void Build_Adds_Option_With_ValidationAttributes()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandTypes.Leaf).Checker<MockCommandChecker>();
            commandBuilder.DefineOption("opt1", nameof(String), "test opt desc1", ReservedFlags.None).ValidationAttribute(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute)).ValidationAttribute(typeof(System.ComponentModel.DataAnnotations.StringLengthAttribute), 10).Add();
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var cmdDesc = sp.GetServices<CommandDescriptor>().First(c => c.Id == "id1");
            cmdDesc.OptionDescriptors.Should().NotBeNull();
            cmdDesc.OptionDescriptors!["opt1"].ValueCheckers.Should().NotBeNull();
            cmdDesc.OptionDescriptors["opt1"].ValueCheckers!.Count().Should().Be(2);
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}