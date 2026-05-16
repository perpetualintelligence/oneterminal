//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Runners;
using System.Reflection;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandResolver : ICommandResolver
    {
        public bool ResolveAuthenticatorCalled { get; private set; }

        public bool ResolveCheckerCalled { get; private set; }

        public bool ResolveRunnerCalled { get; private set; }

        public bool ResolveRunnerMethodCalled { get; private set; }

        public ICommandChecker? ReturnedChecker { get; private set; }

        public IDelegateCommandRunner? ReturnedRunner { get; private set; }

        public RunMethod? ReturnedRunMethod { get; private set; }

        public ICommandChecker? ReturnThisChecker { get; set; }

        public IDelegateCommandRunner? ReturnThisRunner { get; set; }

        public RunMethod? ReturnThisRunMethod { get; set; } = null;

        public ICommandChecker ResolveCommandChecker(CommandDescriptor commandDescriptor)
        {
            ResolveCheckerCalled = true;

            if (ReturnThisChecker != null)
            {
                ReturnedChecker = ReturnThisChecker;
            }
            else
            {
                ReturnedChecker = new MockCommandCheckerInner();
            }
            return ReturnedChecker;
        }

        public IDelegateCommandRunner ResolveCommandRunner(CommandDescriptor commandDescriptor)
        {
            ResolveRunnerCalled = true;

            if (ReturnThisRunner != null)
            {
                ReturnedRunner = ReturnThisRunner;
            }
            else
            {
                ReturnedRunner = new MockCommandRunnerInner();
            }

            return ReturnedRunner;
        }

        public RunMethod ResolveCommandRunMethod(CommandDescriptor commandDescriptor)
        {
            ResolveRunnerMethodCalled = true;

            if (ReturnThisRunMethod != null)
            {
                ReturnedRunMethod = ReturnThisRunMethod;
            }
            else
            {
                MethodInfo methodInfo = typeof(MockCommandRunnerInner).GetMethod(nameof(MockCommandRunnerInner.RunCommandAsync))!;
                ReturnedRunMethod = new RunMethod(commandDescriptor.Id, methodInfo);
            }

            return ReturnedRunMethod;
        }
    }
}