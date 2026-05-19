//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandParserInner : ICommandParser
    {
        public bool Called { get; set; }

        public bool DoNotSetCommandDescriptor { get; set; }

        public bool DoNotSetParsedCommand { get; set; }

        public ICommandContext? PassedContext { get; internal set; }

        public bool SetExplicitError { get; set; }

        public Task ParseCommandAsync(ICommandContext context)
        {
            Called = true;
            PassedContext = context;

            if (SetExplicitError)
            {
                throw new TerminalException("test_parser_error", "test_parser_error_desc");
            }
            else
            {
                if (DoNotSetParsedCommand)
                {
                    context.SetParsedCommand(null!);
                }
                else if (DoNotSetCommandDescriptor)
                {
                    context.SetParsedCommand(new ParsedCommand(new Command(null!), null));
                }
                else
                {
                    // all ok
                    context.SetParsedCommand(new ParsedCommand(new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandTypes.Leaf)), null));
                }
            }

            return Task.CompletedTask;
        }
    }
}