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
using OneImlx.Test.FluentAssertions;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class CommandBuilderTests
    {
        public CommandBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Add_Native_With_Owner_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "root1", "root1_desc", CommandType.Native, CommandFlags.None)
                                                            .Owners(new("owner1", "owner2"));

            Action act = () => commandBuilder.Add();
            act.Should().Throw<TerminalException>()
                        .WithErrorCode("invalid_command")
                        .WithErrorDescription("The command cannot have an owner. command_type=Native command=id1");
        }

        [Fact]
        public void Add_Root_With_Owner_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "root1", "root1_desc", CommandType.Root, CommandFlags.None)
                                                            .Owners(new("owner1", "owner2"));

            Action act = () => commandBuilder.Add();
            act.Should().Throw<TerminalException>()
                        .WithErrorCode("invalid_command")
                        .WithErrorDescription("The command cannot have an owner. command_type=Root command=id1");
        }

        [Fact]
        public void Build_Adds_Command_To_Global_ServiceCollection()
        {
            // Begin with no command
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ServiceDescriptor? serviceDescriptor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().BeNull();

            // Add command to local
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>();

            // Build
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);

            // Build adds to global
            serviceDescriptor = terminalBuilder.Services.First(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();
            CommandDescriptor instance = (CommandDescriptor)serviceDescriptor.ImplementationInstance!;
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("Command description");
        }

        [Fact]
        public void Build_Returns_Same_TerminalBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>();
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            CommandBuilder commandBuilder = new(terminalBuilder);
            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
        }

        [Fact]
        public void Build_Adds_Command_With_Arguments()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>();
            commandBuilder.DefineArgument(1, "arg1", nameof(String), "arg1 desc", ArgumentFlags.None).Add();
            commandBuilder.DefineArgument(2, "arg2", nameof(String), "arg2 desc", ArgumentFlags.Required).Add();
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var cmdDesc = sp.GetServices<CommandDescriptor>().First(c => c.Id == "id1");
            cmdDesc.ArgumentDescriptors.Should().NotBeNull();
            cmdDesc.ArgumentDescriptors!.Count.Should().Be(2);
        }

        [Fact]
        public void Build_Adds_Command_With_Options()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>();
            commandBuilder.DefineOption("opt1", nameof(String), "opt1 desc", OptionFlags.None).Add();
            commandBuilder.DefineOption("opt2", nameof(String), "opt2 desc", OptionFlags.Required).Add();
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var cmdDesc = sp.GetServices<CommandDescriptor>().First(c => c.Id == "id1");
            cmdDesc.OptionDescriptors.Should().NotBeNull();
            cmdDesc.OptionDescriptors!.Count.Should().Be(2);
        }

        [Fact]
        public void Build_Adds_Command_With_Tags()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>().Tags(new("tag1", "tag2", "tag3"));
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var cmdDesc = sp.GetServices<CommandDescriptor>().First(c => c.Id == "id1");
            cmdDesc.TagIds.Should().NotBeNull();
            cmdDesc.TagIds!.Should().BeEquivalentTo(["tag1", "tag2", "tag3"]);
        }

        [Fact]
        public void Build_Adds_Command_With_CustomProperties()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.Leaf, CommandFlags.None).Checker<MockCommandChecker>().CustomProperty("key1", "value1").CustomProperty("key2", 42);
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var cmdDesc = sp.GetServices<CommandDescriptor>().First(c => c.Id == "id1");
            cmdDesc.CustomProperties.Should().NotBeNull();
            cmdDesc.CustomProperties!.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
            cmdDesc.CustomProperties.Should().ContainKey("key2").WhoseValue.Should().Be(42);
        }

        [Fact]
        public void Build_Adds_Command_With_RunMethods()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.CompositeGroup, CommandFlags.None).Checker<MockCommandChecker>();
            commandBuilder.DefineRunMethod("method1", "TestMethod1").Add();
            commandBuilder.DefineRunMethod("method2", "TestMethod2").Add();
            ITerminalBuilder tb = commandBuilder.Add();
            ServiceProvider sp = tb.Services.BuildServiceProvider();
            var runMethods = sp.GetServices<RunMethod>();
            runMethods.Count().Should().Be(2);
        }

        ~CommandBuilderTests()
        {
            host.Dispose();
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}