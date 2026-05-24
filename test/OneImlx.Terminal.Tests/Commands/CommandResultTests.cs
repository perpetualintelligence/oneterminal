//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class CommandResultTests
    {
        [Fact]
        public void Constructor_SetsCheckerAndRunnerResult()
        {
            var checker = new CommandCheckerResult();
            var runner = new CommandRunnerResult();

            var result = new CommandResult(checker, runner);

            result.CheckerResult.Should().BeSameAs(checker);
            result.RunnerResult.Should().BeSameAs(runner);
        }

        [Fact]
        public void EnsureCheckerResult_WhenNull_Throws()
        {
            Action act = () => new CommandResult().EnsureCheckerResult();

            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command checker result is not set.");
        }

        [Fact]
        public void EnsureRunnerResult_WhenNull_Throws()
        {
            Action act = () => new CommandResult().EnsureRunnerResult();

            act.Should().Throw<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command runner result is not set.");
        }
    }
}