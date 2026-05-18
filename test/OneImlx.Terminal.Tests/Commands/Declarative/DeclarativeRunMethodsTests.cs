//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
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
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
            terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunMethodsRunner>();
            serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            cmdDescs = serviceProvider.GetServices<CommandDescriptor>().ToList();
        }

        [Fact]
        public void CompositeGroup_Registers_Three_RunMethods()
        {
            serviceProvider.GetServices<RunMethod>().Should().HaveCount(3);
        }

        [Fact]
        public void NonCompositeGroup_Has_No_RunMethods()
        {
            var tb = new TerminalBuilder(new ServiceCollection(), new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            tb.AddDeclarativeRunner<MockDeclarativeRunner1>();
            var sp = tb.Services.BuildServiceProvider();
            sp.GetServices<RunMethod>().Should().BeEmpty();
        }

        [Fact]
        public void RunMethods_Store_MethodInfo()
        {
            var runMethods = serviceProvider.GetServices<RunMethod>();
            runMethods.Should().OnlyContain(rm => rm.MethodInfo != null && rm.MethodInfo.DeclaringType == typeof(MockDeclarativeRunMethodsRunner) && rm.MethodInfo.ReturnType == typeof(Task<CommandRunnerResult>));
        }

        [Fact]
        public void RunMethods_Map_To_Parent_CompositeGroup_Id()
        {
            serviceProvider.GetServices<RunMethod>().Should().OnlyContain(rm => rm.Id == "composite1");
        }

        [Fact]
        public void Creates_Descriptors_For_Group_And_Methods()
        {
            cmdDescs.Should().HaveCount(4);
            cmdDescs.Where(c => c.Type == CommandType.Leaf).Should().HaveCount(3);
            cmdDescs.Where(c => c.Type == CommandType.CompositeGroup).Should().HaveCount(1);
        }

        [Fact]
        public void Method_Descriptors_Have_Correct_Metadata()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            method1.Name.Should().Be("method1_name");
            method1.Description.Should().Be("method1 description");
            method1.Type.Should().Be(CommandType.Leaf);
            var method2 = cmdDescs.First(c => c.Id == "method2");
            var method3 = cmdDescs.First(c => c.Id == "method3");
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
            cmdDescs.First(c => c.Id == "method2").Checker.Should().Be<MockCommandCheckerInner>();
            cmdDescs.First(c => c.Id == "method3").Checker.Should().Be(typeof(CommandChecker));
        }

        [Fact]
        public void Method1_Has_Arguments_And_Options()
        {
            var method1 = cmdDescs.First(c => c.Id == "method1");
            method1.ArgumentDescriptors.Should().HaveCount(2);
            var argDescs = method1.ArgumentDescriptors!;
            argDescs["m1_arg1"].Id.Should().Be("m1_arg1");
            argDescs["m1_arg1"].Order.Should().Be(1);
            argDescs["m1_arg1"].DataType.Should().Be(nameof(String));
            argDescs["m1_arg1"].Description.Should().Be("method1 arg1 desc");
            argDescs["m1_arg1"].Flags.Should().Be(ReservedFlags.None);
            argDescs["m1_arg2"].Id.Should().Be("m1_arg2");
            argDescs["m1_arg2"].Order.Should().Be(2);
            argDescs["m1_arg2"].DataType.Should().Be(nameof(Int32));
            argDescs["m1_arg2"].Description.Should().Be("method1 arg2 desc");
            argDescs["m1_arg2"].Flags.Should().Be(ReservedFlags.Required);

            method1.OptionDescriptors.Should().HaveCount(3);
            var optDescs = method1.OptionDescriptors!;
            optDescs["m1_opt1"].Id.Should().Be("m1_opt1");
            optDescs["m1_opt1"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt1"].Description.Should().Be("method1 option1 desc");
            optDescs["m1_opt1"].Flags.Should().Be(ReservedFlags.None);
            optDescs["m1_opt1"].Alias.Should().BeNull();
            optDescs["m1_opt2"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2"].DataType.Should().Be(nameof(String));
            optDescs["m1_opt2"].Description.Should().Be("method1 option2 desc");
            optDescs["m1_opt2"].Flags.Should().Be(ReservedFlags.Required);
            optDescs["m1_opt2"].Alias.Should().Be("m1_opt2_alias");
            optDescs["m1_opt2_alias"].Id.Should().Be("m1_opt2");
            optDescs["m1_opt2_alias"].Alias.Should().Be("m1_opt2_alias");
        }

        [Fact]
        public void Method2_Has_Arguments_And_Options()
        {
            var method2 = cmdDescs.First(c => c.Id == "method2");
            method2.ArgumentDescriptors.Should().HaveCount(2);
            method2.ArgumentDescriptors!["m2_arg1"].Id.Should().Be("m2_arg1");
            method2.ArgumentDescriptors["m2_arg1"].Flags.Should().Be(ReservedFlags.Required | ReservedFlags.Obsolete);
            method2.ArgumentDescriptors["m2_arg2"].Id.Should().Be("m2_arg2");
            method2.ArgumentDescriptors["m2_arg2"].Flags.Should().Be(ReservedFlags.None);
            method2.OptionDescriptors.Should().HaveCount(1);
            method2.OptionDescriptors!["m2_opt1"].Id.Should().Be("m2_opt1");
            method2.OptionDescriptors["m2_opt1"].Flags.Should().Be(ReservedFlags.Disabled);
        }

        [Fact]
        public void Method3_Has_Arguments_And_Options()
        {
            var method3 = cmdDescs.First(c => c.Id == "method3");
            method3.ArgumentDescriptors.Should().HaveCount(3);
            method3.ArgumentDescriptors!["m3_arg1"].Id.Should().Be("m3_arg1");
            method3.ArgumentDescriptors["m3_arg2"].Id.Should().Be("m3_arg2");
            method3.ArgumentDescriptors["m3_arg3"].Id.Should().Be("m3_arg3");
            method3.ArgumentDescriptors["m3_arg3"].Flags.Should().Be(ReservedFlags.Required | ReservedFlags.Disabled);
            method3.OptionDescriptors.Should().HaveCount(3);
            method3.OptionDescriptors!["m3_opt1"].Id.Should().Be("m3_opt1");
            method3.OptionDescriptors["m3_opt2"].Id.Should().Be("m3_opt2");
            method3.OptionDescriptors["m3_opt2"].Flags.Should().Be(ReservedFlags.Required | ReservedFlags.Obsolete);
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
            host.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host;
        private readonly TerminalBuilder terminalBuilder;
        private IServiceCollection serviceCollection = null!;
        private readonly ServiceProvider serviceProvider;
        private readonly List<CommandDescriptor> cmdDescs;
    }
}