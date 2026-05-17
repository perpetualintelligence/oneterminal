//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id5", "name5", "description", CommandType.Leaf)]
    [CommandChecker(typeof(MockCommandChecker))]
    public class MockDeclarativeRunner5 : IDeclarativeRunner
    {
    }
}