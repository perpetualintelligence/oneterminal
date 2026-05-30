//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class CommandContextTests
    {
        [Fact]
        public void Constructor_Sets_Request_RouterContext_And_Properties()
        {
            var request = new CommandRequest("req-1", "test command");
            var routerContext = new MockRoutingContext(TerminalStartMode.Console, CancellationToken.None);
            var properties = new Dictionary<string, object> { ["key"] = "value" };

            ICommandContext context = new CommandContextFactory().Create(request, routerContext, properties);

            context.Properties.Should().BeSameAs(properties);
            context.GetCommandRequest().Should().BeSameAs(request);
            context.GetRouterContext().Should().BeSameAs(routerContext);
        }
    }
}