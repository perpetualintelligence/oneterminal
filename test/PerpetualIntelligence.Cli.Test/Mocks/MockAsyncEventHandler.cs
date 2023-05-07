﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Events;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    internal class MockAsyncEventHandler : IAsyncEventHandler
    {
        public bool AfterRouteCalled { get; private set; }
        public bool BeforeRouteCalled { get; private set; }
        public bool AfterRunCalled { get; private set; }
        public bool BeforeRunCalled { get; private set; }
        public bool AfterCheckCalled { get; private set; }
        public bool BeforeCheckCalled { get; private set; }

        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result)
        {
            AfterCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRouteAsync(Command command, CommandRunnerResult result)
        {
            AfterRouteCalled = true;
            return Task.CompletedTask;
        }

        public Task AfterCommandRouteAsync(Command command, CommandRouterResult result)
        {
            throw new System.NotImplementedException();
        }

        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result)
        {
            AfterRunCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandCheckAsync(Command command)
        {
            BeforeCheckCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandRouteAsync(CommandRoute commandRoute)
        {
            BeforeRouteCalled = true;
            return Task.CompletedTask;
        }

        public Task BeforeCommandRunAsync(Command command)
        {
            BeforeRunCalled = true;
            return Task.CompletedTask;
        }
    }
}