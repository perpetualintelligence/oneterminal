﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    [CommandDescriptor("id1", "name", "description", CommandType.SubCommand, CommandFlags.None)]
    [CommandRunner(typeof(MockCommandRunner))]
    [CommandTags("tag1", "tag2", "tag3")]
    [CommandCustomProperty("key1", "value1")]
    [CommandCustomProperty("key2", "value2")]
    [CommandCustomProperty("key3", "value3")]
    [OptionDescriptor("arg1", nameof(String), "test arg desc1", OptionFlags.None)]
    [OptionCustomProperty("arg1", "key1", "value1")]
    [OptionCustomProperty("arg1", "key2", "value2")]
    [OptionCustomProperty("arg1", "key3", "value3")]
    [OptionDescriptor("arg2", nameof(String), "test arg desc2", OptionFlags.None)]
    [OptionValidation("arg2", typeof(RequiredAttribute))]
    [OptionCustomProperty("arg2", "key1", "value1")]
    [OptionCustomProperty("arg2", "key2", "value2")]
    [OptionDescriptor("ar3", nameof(String), "test arg desc3", OptionFlags.None)]
    [OptionValidation("arg3", typeof(RangeAttribute), 25, 40)]
    public class MockDeclarativeTargetNoCommandChecker : IDeclarativeTarget
    {
    }
}