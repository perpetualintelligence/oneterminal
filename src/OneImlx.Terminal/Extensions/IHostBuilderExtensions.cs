//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IHostBuilder"/> extension methods.
    /// </summary>
    public static class IHostBuilderExtensions
    {
        /// <summary>
        /// Starts the <see cref="IHost"/> and runs the <see cref="ITerminalRouter{TContext}"/> synchronously, blocking
        /// the calling thread until a cancellation request.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        public static void RunTerminalRouter<TRouter, TContext>(this IHostBuilder hostBuilder, TContext context) where TRouter : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            // Start the host.
            using (IHost host = hostBuilder.Start())
            {
                // Run terminal router indefinitely till canceled.
                host.RunTerminalRouterBlockingAsync<TRouter, TContext>(context).Wait();
            }
        }
    }
}