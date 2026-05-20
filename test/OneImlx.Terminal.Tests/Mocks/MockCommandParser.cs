//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandParser : ICommandParser
    {
        public Task ParseCommandAsync(ICommandContext context)
        {
            Called = true;

            var cIdt = new CommandDescriptor("testid", "testname", "desc", CommandTypes.Leaf);
            Command command = new(cIdt);
            ParsedCommand extractedCommand = new(command, null);
            context.SetParsedCommand(extractedCommand);
            return Task.CompletedTask;
        }

        public bool Called { get; set; }
    }
}