//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Server
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesLocationTest()
        {
            typeof(TerminalHttpMapService).Assembly.Should().HaveTypesInValidLocations();
        }

        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(TerminalHttpMapService).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal.Server");
        }
    }
}