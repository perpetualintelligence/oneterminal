//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Declarative
{
    public class DeclarativeRunMethodsTests : IAsyncDisposable
    {
        public DeclarativeRunMethodsTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
            terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
        }

        [Fact]
        public void Build_Should_Register_RunMethods_For_CompositeGroup()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var runMethodsList = serviceProvider.GetServices<RunMethod>();
            runMethodsList.Should().HaveCount(3);
        }

        [Fact]
        public void Build_Should_Only_Add_RunMethods_For_CompositeGroup()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var runMethodsList = serviceProvider.GetServices<RunMethod>();
            runMethodsList.Should().BeEmpty();
        }

        [Fact]
        public void Build_Should_Store_MethodInfo_In_RunMethod()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var runMethodsList = serviceProvider.GetServices<RunMethod>();
            runMethodsList.Should().HaveCount(3);
            foreach (var runMethod in runMethodsList)
            {
                runMethod.MethodInfo.Should().NotBeNull();
                runMethod.MethodInfo!.DeclaringType.Should().Be<MockDeclarativeRunMethodsRunner>();
                runMethod.MethodInfo.ReturnType.Should().Be<Task<CommandRunnerResult>>();
            }
        }

        [Fact]
        public void Build_Should_Map_RunMethod_Id_To_CompositeGroup_Id()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var runMethodsList = serviceProvider.GetServices<RunMethod>();
            runMethodsList.Should().HaveCount(3);
            foreach (var runMethod in runMethodsList)
            {
                runMethod.Id.Should().Be("composite1");
            }
        }

        [Fact]
        public void Build_Should_Create_CommandDescriptors_For_Each_RunMethod()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(4);
            cmdDescs.Where(c => c.Type == CommandType.Leaf).Should().HaveCount(3);
            cmdDescs.Where(c => c.Type == CommandType.CompositeGroup).Should().HaveCount(1);
        }

        [Fact]
        public void Build_Should_Read_Method1_CommandDescriptor_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.Id.Should().Be("method1");
            method1Cmd.Name.Should().Be("method1_name");
            method1Cmd.Description.Should().Be("method1 description");
            method1Cmd.Type.Should().Be(CommandType.Leaf);
            method1Cmd.Flags.Should().Be(CommandFlags.None);
        }

        [Fact]
        public void Build_Should_Read_Method2_CommandDescriptor_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method2Cmd = cmdDescs.First(c => c.Id == "method2");
            method2Cmd.Id.Should().Be("method2");
            method2Cmd.Name.Should().Be("method2_name");
            method2Cmd.Description.Should().Be("method2 description");
            method2Cmd.Type.Should().Be(CommandType.Leaf);
            method2Cmd.Flags.Should().Be(CommandFlags.Obsolete);
        }

        [Fact]
        public void Build_Should_Read_Method3_CommandDescriptor_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method3Cmd = cmdDescs.First(c => c.Id == "method3");
            method3Cmd.Id.Should().Be("method3");
            method3Cmd.Name.Should().Be("method3_name");
            method3Cmd.Description.Should().Be("method3 description");
            method3Cmd.Type.Should().Be(CommandType.Leaf);
            method3Cmd.Flags.Should().Be(CommandFlags.Authorize);
        }

        [Fact]
        public void Build_Should_Read_Method1_CommandTags_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.TagIds.Should().BeEquivalentTo(["m1_tag1", "m1_tag2", "m1_tag3"]);
        }

        [Fact]
        public void Build_Should_Read_Method1_CustomProperties_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.CustomProperties.Should().NotBeNull();
            method1Cmd.CustomProperties!.Keys.Should().Equal(["m1_key1", "m1_key2"]);
            method1Cmd.CustomProperties!.Values.Should().Equal(["m1_value1", "m1_value2"]);
        }

        [Fact]
        public void Build_Should_Read_Method1_CommandChecker_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.Checker.Should().Be<MockCommandChecker>();
        }

        [Fact]
        public void Build_Should_Read_Method2_CommandChecker_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method2Cmd = cmdDescs.First(c => c.Id == "method2");
            method2Cmd.Checker.Should().Be<MockCommandCheckerInner>();
        }

        [Fact]
        public void Build_Should_Read_Method3_NoCommandChecker_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method3Cmd = cmdDescs.First(c => c.Id == "method3");
            method3Cmd.Checker.Should().Be<CommandChecker>();
        }

        [Fact]
        public void Build_Should_Read_Method1_Arguments_And_Options_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.ArgumentDescriptors.Should().HaveCount(2);
            var argDescs = method1Cmd.ArgumentDescriptors;
            argDescs.Should().NotBeNull();
            argDescs!["m1_arg1"].Id.Should().Be("m1_arg1");
            argDescs["m1_arg1"].Order.Should().Be(1);
            argDescs["m1_arg1"].DataType.Should().Be(nameof(String));
            argDescs["m1_arg1"].Description.Should().Be("method1 arg1 desc");
            argDescs["m1_arg1"].Flags.Should().Be(ArgumentFlags.None);
            argDescs["m1_arg2"].Id.Should().Be("m1_arg2");
            argDescs["m1_arg2"].Order.Should().Be(2);
            argDescs["m1_arg2"].DataType.Should().Be(nameof(Int32));
            argDescs["m1_arg2"].Description.Should().Be("method1 arg2 desc");
            argDescs["m1_arg2"].Flags.Should().Be(ArgumentFlags.Required);
            method1Cmd.OptionDescriptors.Should().HaveCount(3);
            var optDescs = method1Cmd.OptionDescriptors;
            optDescs.Should().NotBeNull();
            optDescs!["m1_opt1"].Id.Should().Be("m1_opt1");
            optDescs["m1_opt1"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt1"].Description.Should().Be("method1 option1 desc");
            optDescs["m1_opt1"].Flags.Should().Be(OptionFlags.None);
            optDescs["m1_opt1"].Alias.Should().BeNull();
            optDescs["m1_opt2"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt2"].Description.Should().Be("method1 option2 desc");
            optDescs["m1_opt2"].Flags.Should().Be(OptionFlags.Required);
            optDescs["m1_opt2"].Alias.Should().Be("m1_opt2_alias");
            optDescs["m1_opt2_alias"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2_alias"].Alias.Should().Be("m1_opt2_alias");
        }

        [Fact]
        public void Build_Should_Read_Method1_ArgumentValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.ArgumentDescriptors.Should().NotBeNull();
            ArgumentDescriptor arg1 = method1Cmd.ArgumentDescriptors!["m1_arg1"];
            arg1.ValueCheckers.Should().BeNull();
            ArgumentDescriptor arg2 = method1Cmd.ArgumentDescriptors["m1_arg2"];
            arg2.ValueCheckers.Should().NotBeNull();
            arg2.ValueCheckers!.Count().Should().Be(1);
            DataValidationValueChecker<Argument> val2Checker1 = (DataValidationValueChecker<Argument>)arg2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RangeAttribute>();
            RangeAttribute val2Range = (RangeAttribute)val2Checker1.ValidationAttribute;
            val2Range.Minimum.Should().Be(10);
            val2Range.Maximum.Should().Be(100);
        }

        [Fact]
        public void Build_Should_Read_Method1_OptionValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.OptionDescriptors.Should().NotBeNull();
            OptionDescriptor opt1 = method1Cmd.OptionDescriptors!["m1_opt1"];
            opt1.ValueCheckers.Should().BeNull();
            OptionDescriptor opt2 = method1Cmd.OptionDescriptors["m1_opt2"];
            opt2.ValueCheckers.Should().NotBeNull();
            opt2.ValueCheckers!.Count().Should().Be(2);
            DataValidationValueChecker<Option> val2Checker1 = (DataValidationValueChecker<Option>)opt2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            DataValidationValueChecker<Option> val2Checker2 = (DataValidationValueChecker<Option>)opt2.ValueCheckers!.Last();
            val2Checker2.ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            OneOfAttribute val2OneOf = (OneOfAttribute)val2Checker2.ValidationAttribute;
            val2OneOf.AllowedValues.Should().BeEquivalentTo(["m1val1", "m1val2", "m1val3"]);
        }

        [Fact]
        public void Build_Should_Read_Method2_Arguments_And_Options_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method2Cmd = cmdDescs.First(c => c.Id == "method2");
            method2Cmd.ArgumentDescriptors.Should().HaveCount(2);
            method2Cmd.ArgumentDescriptors!["m2_arg1"].Id.Should().Be("m2_arg1");
            method2Cmd.ArgumentDescriptors["m2_arg1"].Flags.Should().Be(ArgumentFlags.Required | ArgumentFlags.Obsolete);
            method2Cmd.ArgumentDescriptors["m2_arg2"].Id.Should().Be("m2_arg2");
            method2Cmd.ArgumentDescriptors["m2_arg2"].Flags.Should().Be(ArgumentFlags.None);
            method2Cmd.OptionDescriptors.Should().HaveCount(1);
            method2Cmd.OptionDescriptors!["m2_opt1"].Id.Should().Be("m2_opt1");
            method2Cmd.OptionDescriptors["m2_opt1"].Flags.Should().Be(OptionFlags.Disabled);
        }

        [Fact]
        public void Build_Should_Read_Method3_Arguments_And_Options_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method3Cmd = cmdDescs.First(c => c.Id == "method3");
            method3Cmd.ArgumentDescriptors.Should().HaveCount(3);
            method3Cmd.ArgumentDescriptors!["m3_arg1"].Id.Should().Be("m3_arg1");
            method3Cmd.ArgumentDescriptors["m3_arg2"].Id.Should().Be("m3_arg2");
            method3Cmd.ArgumentDescriptors["m3_arg3"].Id.Should().Be("m3_arg3");
            method3Cmd.ArgumentDescriptors["m3_arg3"].Flags.Should().Be(ArgumentFlags.Required | ArgumentFlags.Disabled);
            method3Cmd.OptionDescriptors.Should().HaveCount(3);
            method3Cmd.OptionDescriptors!["m3_opt1"].Id.Should().Be("m3_opt1");
            method3Cmd.OptionDescriptors["m3_opt2"].Id.Should().Be("m3_opt2");
            method3Cmd.OptionDescriptors["m3_opt2"].Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete);
            method3Cmd.OptionDescriptors["m3_opt1_alias"].Id.Should().Be("m3_opt1");
        }

        [Fact]
        public void Methods_Inherit_Owners_From_CompositeGroup()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            CommandDescriptor method1Cmd = cmdDescs.First(c => c.Id == "method1");
            method1Cmd.OwnerIds.Should().BeEquivalentTo(["owner1", "owner2"]);
            CommandDescriptor method2Cmd = cmdDescs.First(c => c.Id == "method2");
            method2Cmd.OwnerIds.Should().BeEquivalentTo(["owner1", "owner2"]);
            CommandDescriptor method3Cmd = cmdDescs.First(c => c.Id == "method3");
            method3Cmd.OwnerIds.Should().BeEquivalentTo(["owner1", "owner2"]);
        }

        public ValueTask DisposeAsync()
        {
            host.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private readonly TerminalBuilder terminalBuilder;
        private IServiceCollection serviceCollection = null!;
    }
}