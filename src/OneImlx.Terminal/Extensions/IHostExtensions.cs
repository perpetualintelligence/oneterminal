//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IHost"/> extension methods.
    /// </summary>
    public static class IHostExtensions
    {
        /// <summary>
        /// Runs the <see cref="ITerminalRouter{TContext}"/> asynchronously, blocking the calling thread until a
        /// cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        public static async Task RunTerminalRouterBlockingAsync<TRouting, TContext>(this IHost host, TContext context) where TRouting : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            ILogger<ITerminalRouter<TContext>> logger = host.Services.GetRequiredService<ILogger<ITerminalRouter<TContext>>>();
            logger.LogDebug("Start blocking terminal router. type={0} context={1}", typeof(TRouting).Name, typeof(TContext).Name);

            // Link the application lifetime token with the terminal router token to sync the closure.
            logger.LogDebug("Setup terminal router cancellation with application stopping token.");
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            context.TerminalCancellationToken = applicationLifetime.ApplicationStopping;

            // Now run the router in a  blocking loop till canceled.
            ITerminalRouter<TContext> routingService = host.Services.GetRequiredService<ITerminalRouter<TContext>>();
            await routingService.RunAsync(context);
            logger.LogDebug("End blocking terminal router.");
        }

        /// <summary>
        /// Starts the <see cref="ITerminalRouter{TContext}"/> in a background task without blocking the calling thread.
        /// This allows the router to run alongside other services like ASP.NET Core web servers.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context.</param>
        /// <typeparam name="TRouter">The terminal router type.</typeparam>
        /// <typeparam name="TContext">The terminal router context type.</typeparam>
        public static Task RunTerminalRouterBackgroundAsync<TRouter, TContext>(this IHost host, TContext context)
            where TRouter : class, ITerminalRouter<TContext>
            where TContext : TerminalRouterContext
        {
            ILogger<ITerminalRouter<TContext>> logger = host.Services.GetRequiredService<ILogger<ITerminalRouter<TContext>>>();
            logger.LogDebug("Start background terminal router. type={0} context={1}", typeof(TRouter).Name, typeof(TContext).Name);

            // Link the application lifetime token with the terminal router token to sync the closure.
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            context.TerminalCancellationToken = applicationLifetime.ApplicationStopping;

            // Now run the router in a background task without blocking
            ITerminalRouter<TContext> router = host.Services.GetRequiredService<ITerminalRouter<TContext>>();
            return router.RunAsync(context);
        }
    }
}