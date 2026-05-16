//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public partial class RunMethodTests
    {
        public RunMethodTests()
        {
            Command command = new(new CommandDescriptor("cmd-id", "cmd-name", "cmd-desc", CommandType.Leaf, CommandFlags.None));
            parsedCommand = new ParsedCommand(command);
            commandTokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRouterContext(TerminalStartMode.Custom, commandTokenSource.Token);
            routerContext = new CommandContext(new(Guid.NewGuid().ToString(), "test"), routingContext, null) { ParsedCommand = parsedCommand };
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Func<RunMethod> act = () => new RunMethod(null!, "test-method");
            act.Should().Throw<ArgumentNullException>().WithParameterName("id");

            var methodInfo = typeof(MockRunnerWithBaseResult).GetMethod(nameof(MockRunnerWithBaseResult.TestMethodBase))!;
            act = () => new RunMethod(null!, methodInfo);
            act.Should().Throw<ArgumentNullException>().WithParameterName("id");

            act = () => new RunMethod("test-id", methodName: null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("methodName");

            act = () => new RunMethod("test-id", methodInfo: null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("methodInfo");
        }

        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var runMethod = new RunMethod("test-id", "test-method");
            runMethod.Id.Should().Be("test-id");
            runMethod.MethodName.Should().Be("test-method");
            runMethod.MethodInfo.Should().BeNull();

            var methodInfo = typeof(MockRunnerWithBaseResult).GetMethod(nameof(MockRunnerWithBaseResult.TestMethodBase))!;
            runMethod = new RunMethod("test-id", methodInfo);
            runMethod.Id.Should().Be("test-id");
            runMethod.MethodInfo.Should().BeSameAs(methodInfo);
            runMethod.MethodName.Should().Be(nameof(MockRunnerWithBaseResult.TestMethodBase));
        }

        [Fact]
        public async Task RunAsync_InvokesCorrectMethod_WhenMethodNameProvided_BaseResult()
        {
            MockRunnerWithBaseResult runner = new();
            runner.MethodCalled.Should().BeFalse();

            var runMethod = new RunMethod("cmd-id", nameof(MockRunnerWithBaseResult.TestMethodBase));
            var result = await runMethod.DelegateRunAsync(runner, routerContext);

            runner.MethodCalled.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandRunnerResult>();
        }

        [Fact]
        public async Task RunAsync_InvokesCorrectMethod_WhenMethodNameProvided_DerivedResult()
        {
            MockRunnerWithDerivedResult runner = new();
            runner.MethodCalled.Should().BeFalse();

            var runMethod = new RunMethod("cmd-id", nameof(MockRunnerWithDerivedResult.TestMethodDerived));
            var result = await runMethod.DelegateRunAsync(runner, routerContext);

            runner.MethodCalled.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<MockRunnerDerviedResult>();
        }

        [Fact]
        public async Task RunAsync_InvokesCorrectMethod_WhenMethodInfoProvided_BaseResult()
        {
            MockRunnerWithBaseResult runner = new();
            runner.MethodCalled.Should().BeFalse();

            var methodInfo = typeof(MockRunnerWithBaseResult).GetMethod(nameof(MockRunnerWithBaseResult.TestMethodBase))!;
            var runMethod = new RunMethod("cmd-id", methodInfo);
            var result = await runMethod.DelegateRunAsync(runner, routerContext);

            runner.MethodCalled.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandRunnerResult>();
        }

        [Fact]
        public async Task RunAsync_InvokesCorrectMethod_WhenMethodInfoProvided_DerivedResult()
        {
            MockRunnerWithDerivedResult runner = new();
            runner.MethodCalled.Should().BeFalse();

            var methodInfo = typeof(MockRunnerWithDerivedResult).GetMethod(nameof(MockRunnerWithDerivedResult.TestMethodDerived))!;
            var runMethod = new RunMethod("cmd-id", methodInfo);
            var result = await runMethod.DelegateRunAsync(runner, routerContext);

            runner.MethodCalled.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<MockRunnerDerviedResult>();
        }

        [Fact]
        public async Task RunAsync_Throws_WhenParsedCommandIsNull()
        {
            MockRunnerWithBaseResult runner = new();
            var runMethod = new RunMethod("cmd-id", nameof(MockRunnerWithBaseResult.TestMethodBase));
            var contextWithoutParsedCommand = new CommandContext(new(Guid.NewGuid().ToString(), "test"), routingContext, null);
            Func<Task> act = async () => await runMethod.DelegateRunAsync(runner, contextWithoutParsedCommand);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The parsed command is missing in the context.");
        }

        [Fact]
        public async Task RunAsync_Throws_WhenCommandIdDoesNotMatch()
        {
            MockRunnerWithBaseResult runner = new();
            var runMethod = new RunMethod("wrong-id", nameof(MockRunnerWithBaseResult.TestMethodBase));
            Func<Task> act = async () => await runMethod.DelegateRunAsync(runner, routerContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The method's command is invalid. command=wrong-id");
        }

        [Fact]
        public async Task RunAsync_Throws_WhenMethodNameNotFound()
        {
            MockRunnerWithBaseResult runner = new();
            var runMethod = new RunMethod("cmd-id", "NonExistentMethod");
            Func<Task> act = async () => await runMethod.DelegateRunAsync(runner, routerContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("No public run method found on the command. command=cmd-id, name=NonExistentMethod");
        }

        [Fact]
        public async Task RunAsync_Throws_WhenBothMethodInfoAndMethodNameAreNull()
        {
            MockRunnerWithBaseResult runner = new();
            var runMethod = new RunMethod("cmd-id", "SomeMethod");
            var runMethodType = runMethod.GetType();
            var methodInfoField = runMethodType.GetField("<MethodInfo>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var methodNameField = runMethodType.GetField("<MethodName>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfoField!.SetValue(runMethod, null);
            methodNameField!.SetValue(runMethod, null);
            Func<Task> act = async () => await runMethod.DelegateRunAsync(runner, routerContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The method name or method info is not registered. command=cmd-id");
        }

        private readonly ParsedCommand parsedCommand = null!;
        private readonly CancellationTokenSource commandTokenSource = null!;
        private readonly CommandContext routerContext = null!;
        private readonly TerminalRouterContext routingContext = null!;
    }
}