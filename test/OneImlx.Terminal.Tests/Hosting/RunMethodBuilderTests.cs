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
    public class RunMethodBuilderTests
    {
        public RunMethodBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Build_Adds_RunMethod_To_CommandBuilder_ServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("cmd1", "name1", "Command description", ReservedCommandTypes.Leaf).Checker<MockCommandChecker>();
            ServiceDescriptor? serviceDescriptor = commandBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(RunMethod)));
            serviceDescriptor.Should().BeNull();

            IRunMethodBuilder runMethodBuilder = commandBuilder.DefineRunMethod("method1", "TestMethod");
            serviceDescriptor = runMethodBuilder.Services.First(static e => e.ServiceType.Equals(typeof(RunMethod)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();
            RunMethod instance = (RunMethod)serviceDescriptor.ImplementationInstance!;
            instance.Id.Should().Be("method1");
            instance.MethodName.Should().Be("TestMethod");
        }

        [Fact]
        public void Build_Returns_Same_CommandBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("cmd1", "name1", "Command description", ReservedCommandTypes.Leaf).Checker<MockCommandChecker>();
            IRunMethodBuilder runMethodBuilder = commandBuilder.DefineRunMethod("method1", "TestMethod");
            ICommandBuilder commandBuilderFromRunMethodBuilder = runMethodBuilder.Add();
            commandBuilder.Should().BeSameAs(commandBuilderFromRunMethodBuilder);
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("cmd1", "name1", "Command description", ReservedCommandTypes.Leaf).Checker<MockCommandChecker>();
            RunMethodBuilder runMethodBuilder = new(commandBuilder);
            runMethodBuilder.Services.Should().NotBeSameAs(serviceCollection);
            runMethodBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        [Fact]
        public void Build_Multiple_RunMethods_Adds_All()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("cmd1", "name1", "Command description", ReservedCommandTypes.Leaf).Checker<MockCommandChecker>();

            IRunMethodBuilder runMethodBuilder = commandBuilder.DefineRunMethod("method1", "TestMethod1");
            runMethodBuilder.Services.AddSingleton(new RunMethod("method2", "TestMethod2"));
            runMethodBuilder.Services.AddSingleton(new RunMethod("method3", "TestMethod3"));

            var serviceDescriptors = runMethodBuilder.Services.Where(static e => e.ServiceType.Equals(typeof(RunMethod))).ToList();
            serviceDescriptors.Should().HaveCount(3);

            RunMethod instance1 = (RunMethod)serviceDescriptors[0].ImplementationInstance!;
            instance1.Id.Should().Be("method1");
            instance1.MethodName.Should().Be("TestMethod1");

            RunMethod instance2 = (RunMethod)serviceDescriptors[1].ImplementationInstance!;
            instance2.Id.Should().Be("method2");
            instance2.MethodName.Should().Be("TestMethod2");

            RunMethod instance3 = (RunMethod)serviceDescriptors[2].ImplementationInstance!;
            instance3.Id.Should().Be("method3");
            instance3.MethodName.Should().Be("TestMethod3");
        }

        ~RunMethodBuilderTests()
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