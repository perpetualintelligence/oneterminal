//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class CommandContextFactoryTests
    {
        [Fact]
        public void Create_SetsRequestContextAndProperties()
        {
            var factory = new CommandContextFactory();
            var request = new CommandRequest("req-1", "run cmd");
            var routerContext = new MockRoutingContext(TerminalStartMode.Console, CancellationToken.None);
            var properties = new Dictionary<string, object> { ["key"] = "value" };

            var result = factory.Create(request, routerContext, properties);

            result.Request.Should().BeSameAs(request);
            result.RouterContext.Should().BeSameAs(routerContext);
            result.Properties.Should().BeSameAs(properties);
        }
    }
}