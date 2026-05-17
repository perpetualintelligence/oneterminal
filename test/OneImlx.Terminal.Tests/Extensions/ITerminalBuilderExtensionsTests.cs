//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class ITerminalBuilderExtensionsTests
    {
        public ITerminalBuilderExtensionsTests()
        {
            IServiceCollection? serviceDescriptors = null;
            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            if (serviceDescriptors is null)
            {
                throw new InvalidOperationException("Service descriptors not initialized.");
            }

            terminalBuilder = serviceDescriptors.CreateTerminalBuilder(new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode));
        }

        [Fact]
        public void AddBytesParserShouldCorrectlyInitiaize()
        {
            terminalBuilder.AddBytesParser<TerminalBytesParser>();

            var arg = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(ITerminalBytesParser)));
            arg.Should().NotBeNull();
            arg.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be<TerminalBytesParser>();
        }

        [Fact]
        public void AddArgumentCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddArgumentChecker<MockArgumentMapper, MockArgumentChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(IArgumentChecker)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be<MockArgumentChecker>();

            var map = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(IDataTypeMapper<Argument>)));
            map.Should().NotBeNull();
            map!.Lifetime.Should().Be(ServiceLifetime.Transient);
            map.ImplementationType.Should().Be<MockArgumentMapper>();
        }

        [Fact]
        public void AddCommandDescriptorMultipleTimeAdds()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();

            var sp = terminalBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<CommandDescriptor>();
            cmds.Count().Should().Be(2);

            cmds.First().Id.Equals("id1");
            cmds.Last().Id.Equals("id1");

            cmds.First().Should().NotBeSameAs(cmds.Last());
        }

        [Fact]
        public void AddCommandDescriptorShouldCorrectlyInitialize()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();

            ServiceDescriptor? cmdDescriptor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            cmdDescriptor.Should().NotBeNull();
            cmdDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            CommandDescriptor? impInstance = (CommandDescriptor?)cmdDescriptor.ImplementationInstance;
            impInstance.Should().NotBeNull();
            impInstance!.Id.Should().Be("id1");
            impInstance.Name.Should().Be("name1");
            impInstance.Runner.Should().Be<MockCommandRunner>();
            impInstance.Checker.Should().Be<MockCommandChecker>();
            impInstance.Type.Should().Be(CommandType.Leaf);

            var cmdRunner = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(MockCommandRunner)));
            cmdRunner.Should().BeNull();

            var cmdChecker = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(MockCommandChecker)));
            cmdChecker.Should().BeNull();
        }

        [Fact]
        public void AddCommandDescriptorWithGroupAndNoRootShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.IsolatedGroup).Checker<MockCommandChecker>().Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Id.Should().Be("id1");
            CommandType.IsolatedGroup.Should().Be(CommandType.IsolatedGroup);
        }

        [Fact]
        public void AddCommandDescriptorWithSpecialAnnotationsFlagsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.IsolatedGroup).Checker<MockCommandChecker>().Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Type.Should().Be(CommandType.IsolatedGroup);
        }

        [Fact]
        public void AddCommandDescriptorWithSpecialAnnotationsShouldNotError()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Root).Checker<MockCommandChecker>().Add();

            IServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            CommandDescriptor cmd = serviceProvider.GetRequiredService<CommandDescriptor>();

            cmd.Type.Should().Be(CommandType.Root);
        }

        [Fact]
        public void AddCommandParserShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandParser<MockCommandParser, MockCommandRouteParser>();

            var cmd = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandParser)));
            cmd.Should().NotBeNull();
            cmd!.Lifetime.Should().Be(ServiceLifetime.Transient);
            cmd.ImplementationType.Should().Be<MockCommandParser>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalRequestParser)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be<MockCommandRouteParser>();
        }

        [Fact]
        public void AddCommandShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "description1", CommandType.Leaf).Checker<MockCommandChecker>();

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            servicesCmdBuilder.Should().BeNull();

            // Command builder creates a new local service collection
            terminalBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("description1");
            instance.Checker.Should().Be<MockCommandChecker>();
            instance.Runner.Should().Be<MockCommandRunner>();
            instance.Type.Should().Be(CommandType.Leaf);
        }

        [Fact]
        public void AddCommandSpecialAnnomtationsShouldCorrectlyInitialize()
        {
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "description1", CommandType.Root).Checker<MockCommandChecker>();

            // AddCommand does not add ICommandBuilder to service collection.
            var servicesCmdBuilder = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandBuilder)));
            servicesCmdBuilder.Should().BeNull();

            // Command builder creates a new local service collection
            terminalBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            // Lifetime of command builder service
            var serviceDescriptor = commandBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

            // Check the instance
            var serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var instance = serviceProvider.GetRequiredService<CommandDescriptor>();
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("description1");
            instance.Checker.Should().Be<MockCommandChecker>();
            instance.Runner.Should().Be<MockCommandRunner>();
            instance.Type.Should().Be(CommandType.Root);
        }

        [Fact]
        public void AddCommandStoreShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandStore<MockCommandStore>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalCommandStore)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be<MockCommandStore>();
        }

        [Fact]
        public void AddEventHandlerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddEventHandler<MockTerminalEventHandler>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalEventHandler)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be<MockTerminalEventHandler>();
        }

        [Fact]
        public void AddHelpProviderShouldCorrectlyInitialize()
        {
            terminalBuilder.AddHelpProvider<MockTerminalHelpProvider>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalHelpProvider)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            arg.ImplementationType.Should().Be<MockTerminalHelpProvider>();
        }

        [Fact]
        public void AddLicensingShouldCorrectlyInitialize()
        {
            terminalBuilder.AddLicensing();

            var debugger = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ILicenseDebugger)));
            debugger.Should().NotBeNull();
            debugger!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            debugger.ImplementationType.Should().Be<LicenseDebugger>();

            var extractor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ILicenseExtractor)));
            extractor.Should().NotBeNull();
            extractor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            extractor.ImplementationType.Should().Be<LicenseExtractor>();

            var checker = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ILicenseChecker)));
            checker.Should().NotBeNull();
            checker!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            checker.ImplementationType.Should().Be<LicenseChecker>();
        }

        [Fact]
        public void AddOptionCheckerShouldCorrectlyInitialize()
        {
            terminalBuilder.AddOptionChecker<MockOptionMapper, MockOptionChecker>();

            var arg = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(IOptionChecker)));
            arg.Should().NotBeNull();
            arg!.Lifetime.Should().Be(ServiceLifetime.Transient);
            arg.ImplementationType.Should().Be<MockOptionChecker>();

            var map = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(IDataTypeMapper<Option>)));
            map.Should().NotBeNull();
            map!.Lifetime.Should().Be(ServiceLifetime.Transient);
            map.ImplementationType.Should().Be<MockOptionMapper>();
        }

        [Fact]
        public void AddProcessor_Adds_TerminalProcessor_As_Singleton()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var terminalBuilderMock = new Mock<ITerminalBuilder>();
            terminalBuilderMock.SetupGet(static tb => tb.Services).Returns(serviceCollection);

            // Act
            terminalBuilderMock.Object.AddProcessor<TerminalProcessor>();

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(static sd => sd.ServiceType == typeof(ITerminalProcessor));

            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddPublisherShouldCorrectlyInitialize()
        {
            terminalBuilder.AddExceptionHandler<MockExceptionPublisher>();

            var exe = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalExceptionHandler)));
            exe.Should().NotBeNull();
            exe!.Lifetime.Should().Be(ServiceLifetime.Transient);
            exe.ImplementationType.Should().Be<MockExceptionPublisher>();
        }

        [Fact]
        public void AddRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddCommandRouter<MockCommandRouter, MockCommandHandler, MockCommandResolver>();

            var router = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandRouter)));
            router.Should().NotBeNull();
            router!.Lifetime.Should().Be(ServiceLifetime.Transient);
            router.ImplementationType.Should().Be<MockCommandRouter>();

            var handler = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandHandler)));
            handler.Should().NotBeNull();
            handler!.Lifetime.Should().Be(ServiceLifetime.Transient);
            handler!.ImplementationType.Should().Be<MockCommandHandler>();

            var runtime = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ICommandResolver)));
            runtime.Should().NotBeNull();
            runtime!.Lifetime.Should().Be(ServiceLifetime.Transient);
            runtime!.ImplementationType.Should().Be<MockCommandResolver>();
        }

        [Fact]
        public void AddStartContextShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalRouterContext(new TerminalConsoleRouterContext(TerminalStartMode.Console));

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(TerminalRouterContext)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationInstance.Should().BeOfType<TerminalConsoleRouterContext>();
        }

        [Fact]
        public void AddTerminalConsoleShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConsole<TerminalSystemConsole>();

            var exe = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalConsole)));
            exe.Should().NotBeNull();
            exe!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            exe.ImplementationType.Should().Be<TerminalSystemConsole>();
        }

        [Fact]
        public void AddTerminalOptionsShouldCorrectlyInitialize()
        {
            terminalBuilder.AddConfigurationOptions();

            var serviceDescriptor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(TerminalOptions)));
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddTerminalRouterShouldCorrectlyInitialize()
        {
            terminalBuilder.AddTerminalRouter<MockTerminalRouter, MockRoutingContext>();

            var router = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalRouter<MockRoutingContext>)));
            router.Should().NotBeNull();
            router!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            router!.ImplementationType.Should().Be<MockTerminalRouter>();
        }

        [Fact]
        public void AddTextHandlerShouldCorrectlyInitialize()
        {
            // AddTextHandler is called within CreateTerminalBuilder
            var comparer = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(ITerminalTextHandler)));
            comparer.Should().NotBeNull();
            comparer!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            comparer!.ImplementationInstance.Should().NotBeNull();
            comparer.ImplementationType.Should().BeNull();
            comparer.ImplementationInstance!.GetType().Should().Be<TerminalTextHandler>();
        }

        [Fact]
        public void DefineCommand_MultipleTimes_DoesNotAdds_Checker_To_Service_Collection()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();

            var sp = terminalBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<MockCommandChecker>();
            cmds.Count().Should().Be(0);
        }

        [Fact]
        public void DefineCommand_MultipleTimes_DoesNotAdds_Runner_To_Service_Collection()
        {
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();
            terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "desc", CommandType.Leaf).Checker<MockCommandChecker>().Add();

            var sp = terminalBuilder.Services.BuildServiceProvider();
            var cmds = sp.GetServices<MockCommandRunner>();
            cmds.Count().Should().Be(0);
        }

        private readonly ITerminalBuilder terminalBuilder;
    }
}