﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// An abstraction of a routing service.
    /// </summary>
    /// <remarks>
    /// A routing service allows <see cref="ICommandRouter"/> to route the request to a standard or custom implementation.
    /// </remarks>
    public interface IRoutingService
    {
        /// <summary>
        /// Routes to a service implementation.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The routing service result object <see cref="RoutingServiceResult"/>.</returns>
        public Task<RoutingServiceResult> RouteAsync(RoutingServiceContext context);
    }
}