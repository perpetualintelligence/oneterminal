﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Mocks;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    [CommandDescriptor("id2", "name2", "test grp cmd2", "description")]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandChecker(typeof(MockCommandChecker))]
    [TextHandler(typeof(AsciiTextHandler))]
    [ArgumentDescriptor("arg1", DataType.Text, "test arg desc1")]
    [ArgumentCustomProperty("arg1", "key1", "value1")]
    [ArgumentCustomProperty("arg1", "key2", "value2")]
    [ArgumentCustomProperty("arg1", "key3", "value3")]
    [ArgumentDescriptor("arg2", DataType.Text, "test arg desc2")]
    [ArgumentValidation("arg2", typeof(RequiredAttribute))]
    [ArgumentCustomProperty("arg2", "key1", "value1")]
    [ArgumentCustomProperty("arg2", "key2", "value2")]
    [ArgumentDescriptor("ar3", DataType.Text, "test arg desc3")]
    [ArgumentValidation("arg3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeTarget2 : IDeclarativeTarget
    {
    }
}