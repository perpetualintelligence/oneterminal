//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Shared;
using System;
using System.Text;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class TerminalBuilderTests
    {
        public TerminalBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void TerminalBuilder_ShouldReturn_Same_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            terminalBuilder.Services.Should().BeSameAs(serviceCollection);
        }

        [Fact]
        public void TerminalBuilder_Constructor_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            Action act = () => new TerminalBuilder(null!, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            act.Should().Throw<ArgumentNullException>().WithParameterName("services");
        }

        [Fact]
        public void TerminalBuilder_Constructor_ThrowsArgumentNullException_WhenTextHandlerIsNull()
        {
            Action act = () => new TerminalBuilder(serviceCollection, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("textHandler");
        }

        [Fact]
        public void TerminalBuilder_TextHandler_ReturnsCorrectInstance()
        {
            var textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);
            TerminalBuilder terminalBuilder = new(serviceCollection, textHandler);
            terminalBuilder.TextHandler.Should().BeSameAs(textHandler);
        }

        ~TerminalBuilderTests()
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