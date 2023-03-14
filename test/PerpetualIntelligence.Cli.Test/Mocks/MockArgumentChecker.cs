﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentChecker : IOptionChecker
    {
        public bool Called { get; set; }

        public Task<OptionCheckerResult> CheckAsync(OptionCheckerContext context)
        {
            Called = true;
            return Task.FromResult(new OptionCheckerResult(typeof(string)));
        }
    }
}
