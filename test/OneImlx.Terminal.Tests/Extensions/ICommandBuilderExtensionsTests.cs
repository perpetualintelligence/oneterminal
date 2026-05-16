//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class ICommandBuilderExtensionsTests
    {
        public ICommandBuilderExtensionsTests()
        {
            IServiceCollection? serviceDescriptors = null;

            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            serviceDescriptors.Should().NotBeNull();
            terminalBuilder = serviceDescriptors!.CreateTerminalBuilder(new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "description1", CommandType.Leaf, CommandFlags.None)
                                            .Checker<MockCommandChecker>();
        }

        [Fact]
        public void DefineOption_Adds_Custom_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("opt1", "custom-dt", "description1", OptionFlags.Disabled, alias: null);

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(static e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("opt1");
            option.DataType.Should().Be("custom-dt");
            option.Description.Should().Be("description1");
            option.Alias.Should().BeNull();
            option.Flags.Should().Be(OptionFlags.Disabled);
        }

        [Fact]
        public void DefineOption_Adds_Std_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("opt1", nameof(Int32), "description1", OptionFlags.Required | OptionFlags.Obsolete, alias: "arg-alias1");

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(static e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("opt1");
            option.DataType.Should().Be(nameof(Int32));
            option.Description.Should().Be("description1");
            option.Alias.Should().Be("arg-alias1");
            option.Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete);
        }

        [Fact]
        public void DefineRunMethod_WithMethodName_Adds_Correctly()
        {
            IRunMethodBuilder runMethodBuilder = commandBuilder.DefineRunMethod("method1", "TestMethod");

            runMethodBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = runMethodBuilder.Services.First(static e => e.ServiceType.Equals(typeof(RunMethod)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            RunMethod runMethod = (RunMethod)serviceDescriptor.ImplementationInstance!;
            runMethod.Id.Should().Be("method1");
            runMethod.MethodName.Should().Be("TestMethod");
            runMethod.MethodInfo.Should().BeNull();
        }

        [Fact]
        public void DefineRunMethod_WithMethodInfo_Adds_Correctly()
        {
            MethodInfo methodInfo = typeof(MockCommandRunner).GetMethod(nameof(MockCommandRunner.RunCommandAsync))!;
            IRunMethodBuilder runMethodBuilder = commandBuilder.DefineRunMethod("method1", methodInfo);

            runMethodBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = runMethodBuilder.Services.First(static e => e.ServiceType.Equals(typeof(RunMethod)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            RunMethod runMethod = (RunMethod)serviceDescriptor.ImplementationInstance!;
            runMethod.Id.Should().Be("method1");
            runMethod.MethodInfo.Should().BeSameAs(methodInfo);
            runMethod.MethodName.Should().Be("RunCommandAsync");
        }

        [Fact]
        public void DefineArgument_Adds_Correctly()
        {
            IArgumentBuilder argumentBuilder = commandBuilder.DefineArgument(1, "arg1", nameof(String), "description1", ArgumentFlags.Required);

            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(static e => e.ServiceType.Equals(typeof(ArgumentDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            ArgumentDescriptor argument = (ArgumentDescriptor)serviceDescriptor.ImplementationInstance!;
            argument.Order.Should().Be(1);
            argument.Id.Should().Be("arg1");
            argument.DataType.Should().Be(nameof(String));
            argument.Description.Should().Be("description1");
            argument.Flags.Should().Be(ArgumentFlags.Required);
        }

        [Fact]
        public void Owners_Adds_Correctly()
        {
            OwnerIdCollection owners = new("owner1", "owner2", "owner3");
            ICommandBuilder result = commandBuilder.Owners(owners);

            result.Should().BeSameAs(commandBuilder);

            ServiceDescriptor serviceDescriptor = commandBuilder.Services.First(static e => e.ServiceType.Equals(typeof(OwnerIdCollection)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OwnerIdCollection ownersInstance = (OwnerIdCollection)serviceDescriptor.ImplementationInstance!;
            ownersInstance.Should().HaveCount(3);
            ownersInstance.Should().Contain("owner1");
            ownersInstance.Should().Contain("owner2");
            ownersInstance.Should().Contain("owner3");
        }

        [Fact]
        public void Owners_EmptyCollection_Throws()
        {
            OwnerIdCollection owners = [];
            Action act = () => commandBuilder.Owners(owners);
            act.Should().Throw<InvalidOperationException>().WithMessage("The owners cannot be null or empty.");
        }

        [Fact]
        public void Tags_Adds_Correctly()
        {
            TagIdCollection tags = new("tag1", "tag2");
            ICommandBuilder result = commandBuilder.Tags(tags);

            result.Should().BeSameAs(commandBuilder);

            ServiceDescriptor serviceDescriptor = commandBuilder.Services.First(static e => e.ServiceType.Equals(typeof(TagIdCollection)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            TagIdCollection tagsInstance = (TagIdCollection)serviceDescriptor.ImplementationInstance!;
            tagsInstance.Should().HaveCount(2);
            tagsInstance.Should().Contain("tag1");
            tagsInstance.Should().Contain("tag2");
        }

        [Fact]
        public void Tags_EmptyCollection_Throws()
        {
            TagIdCollection tags = [];
            Action act = () => commandBuilder.Tags(tags);
            act.Should().Throw<InvalidOperationException>().WithMessage("The tag identifiers cannot be null or empty.");
        }

        [Fact]
        public void CustomProperty_Adds_Correctly()
        {
            ICommandBuilder result = commandBuilder.CustomProperty("key1", "value1");

            result.Should().BeSameAs(commandBuilder);

            ServiceDescriptor serviceDescriptor = commandBuilder.Services.First(static e => e.ServiceType.Equals(typeof(Tuple<string, object>)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            Tuple<string, object> customProp = (Tuple<string, object>)serviceDescriptor.ImplementationInstance!;
            customProp.Item1.Should().Be("key1");
            customProp.Item2.Should().Be("value1");
        }

        [Fact]
        public void CustomProperty_Multiple_Adds_All()
        {
            commandBuilder.CustomProperty("key1", "value1");
            commandBuilder.CustomProperty("key2", 123);
            commandBuilder.CustomProperty("key3", true);

            var customProps = commandBuilder.Services.Where(static e => e.ServiceType.Equals(typeof(Tuple<string, object>))).ToList();
            customProps.Should().HaveCount(3);

            Tuple<string, object> prop1 = (Tuple<string, object>)customProps[0].ImplementationInstance!;
            prop1.Item1.Should().Be("key1");
            prop1.Item2.Should().Be("value1");

            Tuple<string, object> prop2 = (Tuple<string, object>)customProps[1].ImplementationInstance!;
            prop2.Item1.Should().Be("key2");
            prop2.Item2.Should().Be(123);

            Tuple<string, object> prop3 = (Tuple<string, object>)customProps[2].ImplementationInstance!;
            prop3.Item1.Should().Be("key3");
            prop3.Item2.Should().Be(true);
        }

        [Fact]
        public void Checker_Updates_CommandDescriptor()
        {
            ServiceDescriptor serviceDescriptor = commandBuilder.Services.First(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            CommandDescriptor commandDescriptor = (CommandDescriptor)serviceDescriptor.ImplementationInstance!;
            commandDescriptor.Checker.Should().Be<MockCommandChecker>();
        }

        [Fact]
        public void Checker_NoCommandDescriptor_Throws()
        {
            ICommandBuilder emptyBuilder = new CommandBuilder(terminalBuilder);
            Action act = () => emptyBuilder.Checker<MockCommandChecker>();
            act.Should().Throw<InvalidOperationException>().WithMessage("Command descriptor is not yet registered.");
        }

        private readonly ICommandBuilder commandBuilder = null!;
        private readonly ITerminalBuilder terminalBuilder = null!;
    }
}