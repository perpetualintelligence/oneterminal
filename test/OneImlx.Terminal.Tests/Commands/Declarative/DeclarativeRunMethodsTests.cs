//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;
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
            serviceCollection = new ServiceCollection();
            terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            cmdDescs = [.. serviceProvider.GetServices<CommandDescriptor>()];
        }

        [Fact]
        public void CompositeGroup_Has_No_RunMethod()
        {
            CommandDescriptor commandDescriptor = cmdDescs.First(e => e.Id == "composite1");
            commandDescriptor.RunMethod.Should().BeNull();
        }

        [Fact]
        public void NonCompositeGroup_Has_No_RunMethod()
        {
            var tb = new TerminalBuilder(new ServiceCollection(), new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            tb.AddDeclarativeRunner<MockDeclarativeRunner1>();
            var sp = tb.Services.BuildServiceProvider();

            var cmdDesc = sp.GetServices<CommandDescriptor>();
            cmdDesc.Count().Should().Be(1);
            cmdDesc.First().RunMethod.Should().BeNull();
        }

        [Fact]
        public void RunMethods_Not_Registered()
        {
            var runMethods = serviceProvider.GetServices<RunMethodDescriptor>();
            runMethods.Count().Should().Be(3);
        }

        [Fact]
        public void RunMethod_Stores_MethodInfo()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            method1.RunMethod.Should().NotBeNull();

            var runMethods = serviceProvider.GetServices<RunMethodDescriptor>();
            runMethods.Should().OnlyContain(rm => rm.MethodInfo != null && rm.MethodInfo.DeclaringType == typeof(MockDeclarativeRunMethodsRunner) && rm.MethodInfo.ReturnType == typeof(Task<CommandRunnerResult>));
        }

        [Fact]
        public void RunMethod_Map_To_CommandDescriptor()
        {
            var leafs = cmdDescs.Where(c => c.Type == CommandTypes.Leaf);
            leafs.Should().HaveCount(3);

            foreach (CommandDescriptor cmd in leafs)
            {
                cmd.RunMethod.Should().NotBeNull();
                cmd.RunMethod.CommandId.Should().Be(cmd.Id);
            }
        }

        [Fact]
        public void Creates_Descriptors_Maps_To_Correct_Type()
        {
            cmdDescs.Should().HaveCount(4);
            cmdDescs.Where(c => c.Type == CommandTypes.Leaf).Should().HaveCount(3);
            cmdDescs.Where(c => c.Type == CommandTypes.CompositeGroup).Should().HaveCount(1);
        }

        [Fact]
        public void Method_Descriptors_Have_Correct_Metadata()
        {
            var methodDescriptors = cmdDescs.Where(c => c.Type == CommandTypes.Leaf);
            methodDescriptors.Should().HaveCount(3);

            var method1 = methodDescriptors.First(c => c.Id == "method1");
            method1.Name.Should().Be("method1_name");
            method1.Description.Should().Be("method1_description");
            method1.Type.Should().Be(CommandTypes.Leaf);

            var method2 = methodDescriptors.First(c => c.Id == "method2");
            method2.Name.Should().Be("method2_name");
            method2.Description.Should().Be("method2_description");
            method2.Type.Should().Be(CommandTypes.Leaf);

            var method3 = methodDescriptors.First(c => c.Id == "method3");
            method3.Name.Should().Be("method3_name");
            method3.Description.Should().Be("method3_description");
            method3.Type.Should().Be(CommandTypes.Leaf);
        }

        [Fact]
        public void Method1_Has_Tags_And_CustomProperties()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            method1.TagIds.Should().BeEquivalentTo(["m1_tag1", "m1_tag2", "m1_tag3"]);
            method1.CustomProperties.Should().ContainKeys("m1_key1", "m1_key2");
        }

        [Fact]
        public void Method_Checkers_Default_Or_Override()
        {
            cmdDescs.First(c => c.Id == "method1").Checker.Should().Be<MockCommandChecker>();
            cmdDescs.First(c => c.Id == "method2").Checker.Should().Be<MockCommandChecker>();
            cmdDescs.First(c => c.Id == "method3").Checker.Should().Be<MockCommandCheckerInner>();
        }

        [Fact]
        public void Method_Runner_Is_Always_CompositeRunner()
        {
            cmdDescs.First(c => c.Id == "method1").Runner.Should().Be<MockDeclarativeRunMethodsRunner>();
            cmdDescs.First(c => c.Id == "method2").Runner.Should().Be<MockDeclarativeRunMethodsRunner>();
            cmdDescs.First(c => c.Id == "method3").Runner.Should().Be<MockDeclarativeRunMethodsRunner>();
        }

        [Fact]
        public void Method1_Has_Arguments_And_Options()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            method1.ArgumentDescriptors.Should().HaveCount(2);
            var argDescs = method1.ArgumentDescriptors!;
            argDescs["m1_arg1"].CommandId.Should().Be("m1_arg1");
            argDescs["m1_arg1"].Order.Should().Be(1);
            argDescs["m1_arg1"].DataType.Should().Be(nameof(String));
            argDescs["m1_arg1"].Description.Should().Be("method1 arg1 desc");
            argDescs["m1_arg1"].Flags.Should().Be(BehaviorFlags.None);
            argDescs["m1_arg2"].CommandId.Should().Be("m1_arg2");
            argDescs["m1_arg2"].Order.Should().Be(2);
            argDescs["m1_arg2"].DataType.Should().Be(nameof(Int32));
            argDescs["m1_arg2"].Description.Should().Be("method1 arg2 desc");
            argDescs["m1_arg2"].Flags.Should().Be(BehaviorFlags.Required);

            method1.OptionDescriptors.Should().HaveCount(3);
            var optDescs = method1.OptionDescriptors!;
            optDescs["m1_opt1"].Id.Should().Be("m1_opt1");
            optDescs["m1_opt1"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt1"].Description.Should().Be("method1 option1 desc");
            optDescs["m1_opt1"].Flags.Should().Be(BehaviorFlags.None);
            optDescs["m1_opt1"].Alias.Should().BeNull();
            optDescs["m1_opt2"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt2"].Description.Should().Be("method1 option2 desc");
            optDescs["m1_opt2"].Flags.Should().Be(BehaviorFlags.Required);
            optDescs["m1_opt2"].Alias.Should().Be("m1_opt2_alias");
            optDescs["m1_opt2_alias"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2_alias"].Alias.Should().Be("m1_opt2_alias");
        }

        [Fact]
        public void Method2_Has_Arguments_And_Options()
        {
            var method2 = cmdDescs.First(c => c.Id == "method2");
            method2.ArgumentDescriptors.Should().HaveCount(2);
            method2.ArgumentDescriptors!["m2_arg1"].CommandId.Should().Be("m2_arg1");
            method2.ArgumentDescriptors["m2_arg1"].Flags.Should().Be(BehaviorFlags.Required | BehaviorFlags.Obsolete);
            method2.ArgumentDescriptors["m2_arg2"].CommandId.Should().Be("m2_arg2");
            method2.ArgumentDescriptors["m2_arg2"].Flags.Should().Be(BehaviorFlags.None);
            method2.OptionDescriptors.Should().HaveCount(1);
            method2.OptionDescriptors!["m2_opt1"].Id.Should().Be("m2_opt1");
            method2.OptionDescriptors["m2_opt1"].Flags.Should().Be(BehaviorFlags.Disabled);
        }

        [Fact]
        public void Method3_Has_Arguments_And_Options()
        {
            var method3 = cmdDescs.First(c => c.Id == "method3");
            method3.ArgumentDescriptors.Should().HaveCount(3);
            method3.ArgumentDescriptors!["m3_arg1"].CommandId.Should().Be("m3_arg1");
            method3.ArgumentDescriptors["m3_arg2"].CommandId.Should().Be("m3_arg2");
            method3.ArgumentDescriptors["m3_arg3"].CommandId.Should().Be("m3_arg3");
            method3.ArgumentDescriptors["m3_arg3"].Flags.Should().Be(BehaviorFlags.Required | BehaviorFlags.Disabled);
            method3.OptionDescriptors.Should().HaveCount(3);
            method3.OptionDescriptors!["m3_opt1"].Id.Should().Be("m3_opt1");
            method3.OptionDescriptors["m3_opt2"].Id.Should().Be("m3_opt2");
            method3.OptionDescriptors["m3_opt2"].Flags.Should().Be(BehaviorFlags.Required | BehaviorFlags.Obsolete);
            method3.OptionDescriptors["m3_opt1_alias"].Id.Should().Be("m3_opt1");
        }

        [Fact]
        public void Method1_Has_Validation_Attributes()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            var arg2 = method1.ArgumentDescriptors!["m1_arg2"];
            arg2.ValueCheckers.Should().NotBeNull().And.HaveCount(1);
            ((DataValidationValueChecker<Argument>)arg2.ValueCheckers!.First()).ValidationAttribute.Should().BeOfType<RangeAttribute>();
            var opt2 = method1.OptionDescriptors!["m1_opt2"];
            opt2.ValueCheckers.Should().NotBeNull().And.HaveCount(2);
        }

        [Fact]
        public void Commands_Have_Correct_Owners()
        {
            cmdDescs.First(c => c.Id == "composite1").OwnerIds.Should().BeEquivalentTo(["owner1", "owner2"]);
            cmdDescs.First(c => c.Id == "method1").OwnerIds.Should().BeEquivalentTo(["composite1"]);
            cmdDescs.First(c => c.Id == "method2").OwnerIds.Should().BeEquivalentTo(["composite1"]);
            cmdDescs.First(c => c.Id == "method3").OwnerIds.Should().BeEquivalentTo(["composite1"]);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        private readonly TerminalBuilder terminalBuilder;
        private IServiceCollection serviceCollection = null!;
        private readonly ServiceProvider serviceProvider;
        private readonly List<CommandDescriptor> cmdDescs;
    }
}