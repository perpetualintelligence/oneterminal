//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTerminalConsole_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            services.AddTerminalConsole<TerminalInMemoryCommandStore>(
                textHandler,
                static options => { }
            );

            var provider = services.BuildServiceProvider();

            // Text handler is special
            provider.GetService<ITerminalTextHandler>().Should().BeSameAs(textHandler);

            // Type services - hardcoded in AddTerminalConsole
            provider.GetService<ITerminalConsole>().Should().BeOfType<TerminalSystemConsole>();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalConsoleHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalTextHandler>().Should().BeOfType<TerminalTextHandler>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();
            provider.GetService<ITerminalBytesParser>().Should().BeOfType<TerminalBytesParser>();

            // Command Router
            provider.GetService<ICommandRouter>().Should().BeOfType<CommandRouter>();
            provider.GetService<ICommandHandler>().Should().BeOfType<CommandHandler>();
            provider.GetService<ICommandResolver>().Should().BeOfType<CommandResolver>();

            // Command Parser
            provider.GetService<ICommandParser>().Should().BeOfType<CommandParser>();
            provider.GetService<ITerminalRequestParser>().Should().BeOfType<TerminalRequestQueueParser>();

            // Option and Argument Checkers
            provider.GetService<IOptionChecker>().Should().BeOfType<OptionChecker>();
            provider.GetService<IDataTypeMapper<Option>>().Should().BeOfType<DataTypeMapper<Option>>();

            // Argument checker
            provider.GetService<IArgumentChecker>().Should().BeOfType<ArgumentChecker>();
            provider.GetService<IDataTypeMapper<Argument>>().Should().BeOfType<DataTypeMapper<Argument>>();

            // Exception Handler
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();

            // Licensing Services
            provider.GetService<ILicenseChecker>().Should().BeOfType<LicenseChecker>();
            provider.GetService<ILicenseExtractor>().Should().BeOfType<LicenseExtractor>();
            provider.GetService<ILicenseDebugger>().Should().BeOfType<LicenseDebugger>();

            // Terminal router - should be TerminalConsoleRouter
            provider.GetService<ITerminalRouter<TerminalConsoleRouterContext>>().Should().BeOfType<TerminalConsoleRouter>();
        }

        [Fact]
        public void AddTerminalCli_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            services.AddTerminalCli<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole, TerminalConsoleRouter, TerminalConsoleRouterContext>(
                textHandler,
                static options => { }
            );

            var provider = services.BuildServiceProvider();

            // Text handler
            provider.GetService<ITerminalTextHandler>().Should().BeSameAs(textHandler);

            // Type services
            provider.GetService<ITerminalConsole>().Should().BeOfType<TerminalSystemConsole>();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalConsoleHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();
            provider.GetService<ITerminalBytesParser>().Should().BeOfType<TerminalBytesParser>();

            // Core services
            provider.GetService<ICommandRouter>().Should().BeOfType<CommandRouter>();
            provider.GetService<ICommandHandler>().Should().BeOfType<CommandHandler>();
            provider.GetService<ICommandResolver>().Should().BeOfType<CommandResolver>();
            provider.GetService<ICommandParser>().Should().BeOfType<CommandParser>();
            provider.GetService<ITerminalRequestParser>().Should().BeOfType<TerminalRequestQueueParser>();
            provider.GetService<IOptionChecker>().Should().BeOfType<OptionChecker>();
            provider.GetService<IArgumentChecker>().Should().BeOfType<ArgumentChecker>();

            // Licensing Services
            provider.GetService<ILicenseChecker>().Should().BeOfType<LicenseChecker>();
            provider.GetService<ILicenseExtractor>().Should().BeOfType<LicenseExtractor>();
            provider.GetService<ILicenseDebugger>().Should().BeOfType<LicenseDebugger>();

            // Terminal router
            provider.GetService<ITerminalRouter<TerminalConsoleRouterContext>>().Should().BeOfType<TerminalConsoleRouter>();
        }

        [Fact]
        public void AddTerminalClient_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            services.AddTerminalClient<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                static options => { }
            );

            var provider = services.BuildServiceProvider();

            // Text handler
            provider.GetService<ITerminalTextHandler>().Should().BeSameAs(textHandler);

            // Type services
            provider.GetService<ITerminalConsole>().Should().BeOfType<TerminalSystemConsole>();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalConsoleHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();
            provider.GetService<ITerminalBytesParser>().Should().BeOfType<TerminalBytesParser>();

            // Core services
            provider.GetService<ICommandRouter>().Should().BeOfType<CommandRouter>();
            provider.GetService<ICommandHandler>().Should().BeOfType<CommandHandler>();
            provider.GetService<ICommandResolver>().Should().BeOfType<CommandResolver>();
            provider.GetService<ICommandParser>().Should().BeOfType<CommandParser>();
            provider.GetService<ITerminalRequestParser>().Should().BeOfType<TerminalRequestQueueParser>();
            provider.GetService<IOptionChecker>().Should().BeOfType<OptionChecker>();
            provider.GetService<IArgumentChecker>().Should().BeOfType<ArgumentChecker>();

            // Licensing Services
            provider.GetService<ILicenseChecker>().Should().BeOfType<LicenseChecker>();
            provider.GetService<ILicenseExtractor>().Should().BeOfType<LicenseExtractor>();
            provider.GetService<ILicenseDebugger>().Should().BeOfType<LicenseDebugger>();

            // Terminal router should NOT be registered (added separately)
            provider.GetService<ITerminalRouter<TerminalConsoleRouterContext>>().Should().BeNull();
        }

        [Fact]
        public void AddTerminalServer_ShouldInitializeCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            services.AddTerminalServer<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                static options => { }
            );

            var provider = services.BuildServiceProvider();

            // Text handler
            provider.GetService<ITerminalTextHandler>().Should().BeSameAs(textHandler);

            // Type services
            provider.GetService<ITerminalConsole>().Should().BeOfType<TerminalSystemConsole>();
            provider.GetService<ITerminalHelpProvider>().Should().BeOfType<TerminalConsoleHelpProvider>();
            provider.GetService<ITerminalCommandStore>().Should().BeOfType<TerminalInMemoryCommandStore>();
            provider.GetService<ITerminalExceptionHandler>().Should().BeOfType<TerminalConsoleExceptionHandler>();
            provider.GetService<ITerminalBytesParser>().Should().BeOfType<TerminalBytesParser>();

            // Core services
            provider.GetService<ICommandRouter>().Should().BeOfType<CommandRouter>();
            provider.GetService<ICommandHandler>().Should().BeOfType<CommandHandler>();
            provider.GetService<ICommandResolver>().Should().BeOfType<CommandResolver>();
            provider.GetService<ICommandParser>().Should().BeOfType<CommandParser>();
            provider.GetService<ITerminalRequestParser>().Should().BeOfType<TerminalRequestQueueParser>();
            provider.GetService<IOptionChecker>().Should().BeOfType<OptionChecker>();
            provider.GetService<IArgumentChecker>().Should().BeOfType<ArgumentChecker>();

            // Licensing Services
            provider.GetService<ILicenseChecker>().Should().BeOfType<LicenseChecker>();
            provider.GetService<ILicenseExtractor>().Should().BeOfType<LicenseExtractor>();
            provider.GetService<ILicenseDebugger>().Should().BeOfType<LicenseDebugger>();

            // Terminal router should NOT be registered (added separately)
            provider.GetService<ITerminalRouter<TerminalConsoleRouterContext>>().Should().BeNull();
        }

        [Fact]
        public void CreateTerminalBuilder_Only_Adds_TextHandler()
        {
            IServiceCollection? serviceDescriptors = null;
            ITerminalBuilder? terminalBuilder = null;
            TerminalTextHandler textHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            using var host = Host.CreateDefaultBuilder([]).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
                terminalBuilder = serviceDescriptors!.CreateTerminalBuilder(textHandler);
            }).Build();

            serviceDescriptors.Should().NotBeNull();

            terminalBuilder.Should().NotBeNull()
                           .And.BeOfType<TerminalBuilder>()
                           .And.Match<TerminalBuilder>(tb => ReferenceEquals(serviceDescriptors, tb.Services));

            // Ensure text handler added
            ITerminalTextHandler? fromServices = host.Services.GetService<ITerminalTextHandler>();
            fromServices.Should().NotBeNull();
            fromServices.Should().BeSameAs(textHandler);

            // Verify no core services were added (only text handler)
            host.Services.GetService<ICommandRouter>().Should().BeNull();
            host.Services.GetService<ICommandHandler>().Should().BeNull();
            host.Services.GetService<ICommandResolver>().Should().BeNull();
            host.Services.GetService<ILicenseChecker>().Should().BeNull();
            host.Services.GetService<ILicenseExtractor>().Should().BeNull();
            host.Services.GetService<ILicenseDebugger>().Should().BeNull();
        }

        [Fact]
        public void AddTerminalConsole_WithNullServices_ShouldThrow()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => ((IServiceCollection)null!).AddTerminalConsole<TerminalInMemoryCommandStore>(
                textHandler,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddTerminalConsole_WithNullTextHandler_ShouldThrow()
        {
            var services = new ServiceCollection();

            var act = () => services.AddTerminalConsole<TerminalInMemoryCommandStore>(
                null!,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("textHandler");
        }

        [Fact]
        public void AddTerminalConsole_WithNullSetupAction_ShouldThrow()
        {
            var services = new ServiceCollection();
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => services.AddTerminalConsole<TerminalInMemoryCommandStore>(
                textHandler,
                null!
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("setupAction");
        }

        [Fact]
        public void AddTerminalCli_WithNullServices_ShouldThrow()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => ((IServiceCollection)null!).AddTerminalCli<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole, TerminalConsoleRouter, TerminalConsoleRouterContext>(
                textHandler,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddTerminalCli_WithNullTextHandler_ShouldThrow()
        {
            var services = new ServiceCollection();

            var act = () => services.AddTerminalCli<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole, TerminalConsoleRouter, TerminalConsoleRouterContext>(
                null!,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("textHandler");
        }

        [Fact]
        public void AddTerminalCli_WithNullSetupAction_ShouldThrow()
        {
            var services = new ServiceCollection();
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => services.AddTerminalCli<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole, TerminalConsoleRouter, TerminalConsoleRouterContext>(
                textHandler,
                null!
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("setupAction");
        }

        [Fact]
        public void AddTerminalClient_WithNullServices_ShouldThrow()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => ((IServiceCollection)null!).AddTerminalClient<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddTerminalClient_WithNullTextHandler_ShouldThrow()
        {
            var services = new ServiceCollection();

            var act = () => services.AddTerminalClient<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                null!,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("textHandler");
        }

        [Fact]
        public void AddTerminalClient_WithNullSetupAction_ShouldThrow()
        {
            var services = new ServiceCollection();
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => services.AddTerminalClient<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                null!
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("setupAction");
        }

        [Fact]
        public void AddTerminalServer_WithNullServices_ShouldThrow()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => ((IServiceCollection)null!).AddTerminalServer<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void AddTerminalServer_WithNullTextHandler_ShouldThrow()
        {
            var services = new ServiceCollection();

            var act = () => services.AddTerminalServer<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                null!,
                static options => { }
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("textHandler");
        }

        [Fact]
        public void AddTerminalServer_WithNullSetupAction_ShouldThrow()
        {
            var services = new ServiceCollection();
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => services.AddTerminalServer<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                textHandler,
                null!
            );

            act.Should().Throw<ArgumentNullException>().WithParameterName("setupAction");
        }

        [Fact]
        public void CreateTerminalBuilder_WithNullServices_ShouldThrow()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

            var act = () => ((IServiceCollection)null!).CreateTerminalBuilder(textHandler);

            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }
    }
}