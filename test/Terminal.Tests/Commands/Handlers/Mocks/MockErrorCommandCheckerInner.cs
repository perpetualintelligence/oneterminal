﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandCheckerInner : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckCommandAsync(CommandCheckerContext context)
        {
            throw new TerminalException("test_checker_error", "test_checker_error_desc");
        }
    }
}