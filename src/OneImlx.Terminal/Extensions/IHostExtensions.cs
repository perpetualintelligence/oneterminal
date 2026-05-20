//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
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

            // Ensure we have the license extracted before routing
            ILicenseExtractor licenseExtractor = host.Services.GetRequiredService<ILicenseExtractor>();
            License license = await licenseExtractor.GetLicenseAsync().ConfigureAwait(false) ?? throw new TerminalException(TerminalErrors.InvalidLicense, "Failed to extract a valid license. Please configure the hosted service correctly.");
            if (license.Failed != null)
            {
                throw new TerminalException(license.Failed);
            }
            logger.LogDebug("Get license. id={0} tenant={1} plan={2}", license.Claims.Id, license.Claims.TenantId, license.Plan);

            // Now run the router in a  blocking loop till canceled.
            ITerminalRouter<TContext> routingService = host.Services.GetRequiredService<ITerminalRouter<TContext>>();
            await routingService.RunAsync(context);
            logger.LogDebug("End blocking terminal router.");
        }

        /// <summary>Starts the <see cref="ITerminalRouter{TContext}"/> in a background task without blocking or allowing await. This allows the router to run alongside other services like ASP.NET Core web servers. The license is validated before the router is started.</summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context.</param>
        /// <typeparam name="TRouter">The terminal router type.</typeparam>
        /// <typeparam name="TContext">The terminal router context type.</typeparam>
        public static void RunTerminalRouterBackground<TRouter, TContext>(this IHost host, TContext context)
            where TRouter : class, ITerminalRouter<TContext>
            where TContext : TerminalRouterContext
        {
            // Get logger and log start
            ILogger<ITerminalRouter<TContext>> logger = host.Services.GetRequiredService<ILogger<ITerminalRouter<TContext>>>();
            logger.LogDebug("Start background terminal router. type={0} context={1}", typeof(TRouter).Name, typeof(TContext).Name);

            // Link the application lifetime token with the terminal router token to sync the closure
            var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            context.TerminalCancellationToken = applicationLifetime.ApplicationStopping;

            // Extract license synchronously before starting router
            ILicenseExtractor licenseExtractor = host.Services.GetRequiredService<ILicenseExtractor>();
            License license = licenseExtractor.GetLicenseAsync().GetAwaiter().GetResult() ?? throw new TerminalException(TerminalErrors.InvalidLicense, "Failed to extract a valid license. Please configure the hosted service correctly.");
            if (license.Failed != null)
            {
                throw new TerminalException(license.Failed);
            }
            logger.LogDebug("Get license. id={0} tenant={1} plan={2}", license.Claims.Id, license.Claims.TenantId, license.Plan);

            // Start the router in the background (fire-and-forget)
            ITerminalRouter<TContext> router = host.Services.GetRequiredService<ITerminalRouter<TContext>>();
            _ = router.RunAsync(context);
        }
    }
}