using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Apps.Test.Custom
{
    internal class CustomCommandContextFactory : ICommandContextFactory
    {
        public CommandContext Create(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties)
        {
            var context = new CustomCommandContext(properties);
            context.SetCommandRequest(request);
            context.SetRouterContext(routerContext);
            return context;

        }

        public TContext Create<TContext>(CommandRequest request, TerminalRouterContext routerContext, Dictionary<string, object> properties) where TContext : CommandContext
        {
            var context = new CustomCommandContext(properties);
            context.SetCommandRequest(request);
            context.SetRouterContext(routerContext);
            return (TContext) (CommandContext) context;
        }
    }
}
