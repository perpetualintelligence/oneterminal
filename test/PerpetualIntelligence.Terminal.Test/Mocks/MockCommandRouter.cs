﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockCommandRouter : ICommandRouter
    {
        public MockCommandRouter(int? routeDelay = null, CancellationTokenSource? cancelOnRouteCalled = null, Exception? exception = null, Error? explicitError = null)
        {
            this.routeDelay = routeDelay;
            this.cancelOnRouteCalled = cancelOnRouteCalled;
            this.exception = exception;
            this.explicitError = explicitError;
            MultipleRawString = new List<string>();
        }

        public List<string> MultipleRawString { get; set; }

        public string? RawCommandString { get; set; }

        public bool FindCalled { get; set; }

        public bool RouteCalled { get; set; }

        //This is used in the context of singleton Router
        public int RouteCounter { get; set; }

        public CommandRouterResult? ReturnedRouterResult { get; private set; }

        public async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            // Stats
            RouteCalled = true;
            RawCommandString = context.Route.Command.Raw;
            MultipleRawString.Add(context.Route.Command.Raw);
            RouteCounter += 1;

            // Add delay
            if (routeDelay != null)
            {
                await Task.Delay(routeDelay.Value);
            }

            // Cancel on route first
            cancelOnRouteCalled?.Cancel();

            // Raise exception
            if (exception != null)
            {
                throw exception;
            }

            // Raise explicit error
            if (explicitError != null)
            {
                throw new ErrorException(explicitError);
            }

            ReturnedRouterResult = new CommandRouterResult(new CommandHandlerResult(new Commands.Runners.CommandRunnerResult(), new Commands.Checkers.CommandCheckerResult()), context.Route);
            return ReturnedRouterResult;
        }

        private readonly CancellationTokenSource? cancelOnRouteCalled;
        private readonly Exception? exception;
        private readonly Error? explicitError;
        private readonly int? routeDelay;
    }
}