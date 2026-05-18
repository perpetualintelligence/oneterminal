//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Declarative
{
    [CommandOwners("owner1", "owner2")]
    [CommandDescriptor("composite1", "composite_name1", "composite description", ReservedCommandTypes.CompositeGroup)]
    [CommandChecker(typeof(MockCommandChecker))]
    [CommandTags("comp_tag1", "comp_tag2")]
    [CommandCustomProperty("comp_key1", "comp_value1")]
    [CommandCustomProperty("comp_key2", "comp_value2")]
    internal class MockDeclarativeRunMethodsRunner : IDeclarativeRunner
    {
        public bool Method1Called { get; private set; }

        public bool Method2Called { get; private set; }

        public bool Method3Called { get; private set; }

        public string? LastMethodCalled { get; private set; }

        [CommandDescriptor("method1", "method1_name", "method1 description", ReservedCommandTypes.Leaf)]
        [CommandChecker(typeof(MockCommandChecker))]
        [CommandTags("m1_tag1", "m1_tag2", "m1_tag3")]
        [CommandCustomProperty("m1_key1", "m1_value1")]
        [CommandCustomProperty("m1_key2", "m1_value2")]
        [OptionDescriptor("m1_opt1", nameof(String), "method1 option1 desc", ReservedFlags.None)]
        [OptionDescriptor("m1_opt2", nameof(String), "method1 option2 desc", ReservedFlags.Required, "m1_opt2_alias")]
        [OptionValidation("m1_opt2", typeof(RequiredAttribute))]
        [OptionValidation("m1_opt2", typeof(OneOfAttribute), "m1val1", "m1val2", "m1val3")]
        [ArgumentDescriptor(1, "m1_arg1", nameof(String), "method1 arg1 desc", ReservedFlags.None)]
        [ArgumentDescriptor(2, "m1_arg2", nameof(Int32), "method1 arg2 desc", ReservedFlags.Required)]
        [ArgumentValidation("m1_arg2", typeof(RangeAttribute), 10, 100)]
        public Task<CommandRunnerResult> TestMethod1Async(CommandContext context)
        {
            Method1Called = true;
            LastMethodCalled = nameof(TestMethod1Async);
            return Task.FromResult(new CommandRunnerResult());
        }

        [CommandDescriptor("method2", "method2_name", "method2 description", ReservedCommandTypes.Leaf)]
        [CommandChecker(typeof(MockCommandCheckerInner))]
        [CommandTags("m2_tag1")]
        [CommandCustomProperty("m2_key1", "m2_value1")]
        [OptionDescriptor("m2_opt1", nameof(Double), "method2 option1 desc", ReservedFlags.Disabled)]
        [OptionValidation("m2_opt1", typeof(RangeAttribute), 0.5, 99.9)]
        [ArgumentDescriptor(1, "m2_arg1", nameof(String), "method2 arg1 desc", ReservedFlags.Required | ReservedFlags.Obsolete)]
        [ArgumentValidation("m2_arg1", typeof(RequiredAttribute))]
        [ArgumentValidation("m2_arg1", typeof(OneOfAttribute), "m2val1", "m2val2")]
        [ArgumentDescriptor(2, "m2_arg2", nameof(Boolean), "method2 arg2 desc", ReservedFlags.None)]
        public Task<CommandRunnerResult> TestMethod2Async(CommandContext context)
        {
            Method2Called = true;
            LastMethodCalled = nameof(TestMethod2Async);
            return Task.FromResult(new CommandRunnerResult());
        }

        [CommandDescriptor("method3", "method3_name", "method3 description", ReservedCommandTypes.Leaf)]
        [CommandTags("m3_tag1", "m3_tag2")]
        [CommandCustomProperty("m3_key1", "m3_value1")]
        [CommandCustomProperty("m3_key2", "m3_value2")]
        [CommandCustomProperty("m3_key3", "m3_value3")]
        [OptionDescriptor("m3_opt1", nameof(String), "method3 option1 desc", ReservedFlags.None, "m3_opt1_alias")]
        [OptionDescriptor("m3_opt2", nameof(Int32), "method3 option2 desc", ReservedFlags.Required | ReservedFlags.Obsolete)]
        [OptionValidation("m3_opt2", typeof(RequiredAttribute))]
        [ArgumentDescriptor(1, "m3_arg1", nameof(Double), "method3 arg1 desc", ReservedFlags.Required)]
        [ArgumentValidation("m3_arg1", typeof(RangeAttribute), 25.5, 75.5)]
        [ArgumentDescriptor(2, "m3_arg2", nameof(String), "method3 arg2 desc", ReservedFlags.None)]
        [ArgumentDescriptor(3, "m3_arg3", nameof(Int32), "method3 arg3 desc", ReservedFlags.Required | ReservedFlags.Disabled)]
        [ArgumentValidation("m3_arg3", typeof(RangeAttribute), 1, 10)]
        public Task<CommandRunnerResult> TestMethod3Async(CommandContext context)
        {
            Method3Called = true;
            LastMethodCalled = nameof(TestMethod3Async);
            return Task.FromResult(new CommandRunnerResult());
        }

        public void Reset()
        {
            Method1Called = false;
            Method2Called = false;
            Method3Called = false;
            LastMethodCalled = null;
        }
    }
}