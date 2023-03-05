﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check a <see cref="Command"/>.
    /// </summary>
    public interface ICommandChecker : IChecker<CommandCheckerContext, CommandCheckerResult>
    {
    }
}
