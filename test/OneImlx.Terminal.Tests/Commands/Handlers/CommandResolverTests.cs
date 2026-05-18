//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal.Commands.Handlers
{
    public class CommandResolverTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<CommandResolver> _logger;
        private readonly CommandResolver _commandRuntime;

        public CommandResolverTests()
        {
            var services = new ServiceCollection();
            services.AddTransient<ICommandChecker, MockCommandCheckerInner>();
            services.AddTransient<IDelegateCommandRunner, MockCommandRunnerInner>();

            RunMethods runMethods = [];
            runMethods.Add("test", new RunMethod("test", "test_method1"));
            runMethods.Add("test2", new RunMethod("test2", "test_method2"));
            services.AddSingleton<RunMethods>(runMethods);

            _serviceProvider = services.BuildServiceProvider();
            _logger = new NullLogger<CommandResolver>();
            _commandRuntime = new CommandResolver(_serviceProvider, _logger);
        }

        [Fact]
        public void NullChecker_ResolveChecker_Throws()
        {
            CommandDescriptor commandDescriptor = new("test", "test_name", "test_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Checker = null
            };

            Action act = () => _commandRuntime.ResolveCommandChecker(commandDescriptor);
            act.Should().Throw<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The command checker is not configured. command=test");
        }

        [Fact]
        public void NullRunner_ResolveRunner_Throws()
        {
            CommandDescriptor commandDescriptor = new("test", "test_name", "test_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Runner = null
            };

            Action act = () => _commandRuntime.ResolveCommandRunner(commandDescriptor);
            act.Should().Throw<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The command runner delegate is not configured. command=test");
        }

        [Fact]
        public void ValidChecker_ResolveChecker_Success()
        {
            CommandDescriptor commandDescriptor = new("validChecker", "valid_name", "valid_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Checker = typeof(MockCommandCheckerInner)
            };

            var result = _commandRuntime.ResolveCommandChecker(commandDescriptor);

            result.Should().NotBeNull();
            result.Should().BeOfType<MockCommandCheckerInner>();
        }

        [Fact]
        public void ValidRunner_ResolveRunner_Success()
        {
            CommandDescriptor commandDescriptor = new("validRunner", "valid_name", "valid_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Runner = typeof(MockCommandRunnerInner)
            };

            var result = _commandRuntime.ResolveCommandRunner(commandDescriptor);

            result.Should().NotBeNull();
            result.Should().BeOfType<MockCommandRunnerInner>();
        }

        [Fact]
        public void InvalidTypeAsChecker_ResolveChecker_Throws()
        {
            CommandDescriptor commandDescriptor = new("invalidTypeChecker", "invalid_name", "invalid_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Checker = typeof(MockCommandRunnerInner) // Set to a type that does not implement ICommandChecker
            };

            Action act = () => _commandRuntime.ResolveCommandChecker(commandDescriptor);

            act.Should().Throw<TerminalException>()
               .WithErrorCode("server_error")
               .WithErrorDescription($"The command checker is not valid. command={commandDescriptor.Id} checker={commandDescriptor.Checker.Name}");
        }

        [Fact]
        public void InvalidTypeAsRunner_ResolveRunner_Throws()
        {
            CommandDescriptor commandDescriptor = new("invalidTypeRunner", "invalid_name", "invalid_desc", ReservedCommandTypes.IsolatedGroup)
            {
                Runner = typeof(MockCommandCheckerInner) // Set to a type that does not implement IDelegateCommandRunner
            };

            Action act = () => _commandRuntime.ResolveCommandRunner(commandDescriptor);

            act.Should().Throw<TerminalException>()
               .WithErrorCode("server_error")
               .WithErrorDescription("The command runner delegate is not valid. command=invalidTypeRunner runner=MockCommandCheckerInner");
        }

        [Fact]
        public void NonLeafCommand_ResolveCommandRunMethod_Throws()
        {
            CommandDescriptor commandDescriptor = new("test", "test_name", "test_desc", ReservedCommandTypes.IsolatedGroup);
            Action act = () => _commandRuntime.ResolveCommandRunMethod(commandDescriptor);
            act.Should().Throw<TerminalException>().WithErrorCode("server_error").WithErrorDescription("The command runner method is only supported for leaf commands. command=test type=2");
        }

        [Fact]
        public void NotFoundRunMethod_ResolveCommandRunMethod_Throws()
        {
            CommandDescriptor commandDescriptor = new("not_found", "test_name", "test_desc", ReservedCommandTypes.Leaf);
            Action act = () => _commandRuntime.ResolveCommandRunMethod(commandDescriptor);
            act.Should().Throw<TerminalException>().WithErrorCode("server_error").WithErrorDescription("The command runner method is not configured. command=not_found");
        }

        [Fact]
        public void ValidCommandId_ResolveCommandRunMethod_Finds()
        {
            CommandDescriptor commandDescriptor = new("test2", "test_name2", "test_desc2", ReservedCommandTypes.Leaf);
            var runMethod = _commandRuntime.ResolveCommandRunMethod(commandDescriptor);
            runMethod.Id.Should().Be("test2");
            runMethod.MethodName.Should().Be("test_method2");
        }
    }
}