//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandHandlerInner : ICommandHandler
    {
        public MockCommandHandlerInner()
        {
        }

        public bool Called { get; set; }

        public ICommandContext PassedContext { get; internal set; }

        public bool IsExplicitError { get; internal set; }

        public Task HandleCommandAsync(ICommandContext context)
        {
            Called = true;

            PassedContext = context;

            if (IsExplicitError)
            {
                throw new TerminalException("test_handler_error", "test_handler_error_desc");
            }

            context.SetCommandResult(new CommandResult());
            return Task.CompletedTask;
        }
    }
}