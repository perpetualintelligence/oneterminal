//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("oid1, oid2")]
    [CommandDescriptor("id4", "name4", "description", CommandTypes.Leaf)]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("tag1", "tag2", "tag3")]
    [OptionDescriptor("opt1", nameof(String), "test arg desc1", ReservedFlags.None)]
    [OptionDescriptor("opt2", nameof(String), "test arg desc2", ReservedFlags.None)]
    [OptionDescriptor("opt3", nameof(String), "test arg desc3", ReservedFlags.None)]
    [ArgumentDescriptor(1, "arg1", nameof(String), "test arg desc1", ReservedFlags.None)]
    [ArgumentDescriptor(2, "arg2", nameof(String), "test arg desc2", ReservedFlags.None)]
    [ArgumentDescriptor(3, "arg3", nameof(Double), "test arg desc3", ReservedFlags.None)]
    public class MockDeclarativeRunner4 : IDeclarativeRunner
    {
    }
}