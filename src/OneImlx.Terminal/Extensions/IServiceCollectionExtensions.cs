//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using System;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods to register <c>OneImlx.Terminal</c> services with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <c>OneTerminal</c> framework for interactive console-based applications.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// Configures the terminal for user-interactive applications using console I/O. Registers <see cref="TerminalConsoleRouter"/>
        /// with <see cref="TerminalConsoleRouterContext"/> for processing console input and output with <see cref="TerminalSystemConsole"/>.
        /// </remarks>
        public static ITerminalBuilder AddTerminalConsole<TStore>(
            this IServiceCollection services,
            ITerminalTextHandler textHandler,
            Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler>(textHandler, setupAction)
                           .AddConsole<TerminalSystemConsole>()
                           .AddTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>();
        }

        /// <summary>
        /// Adds the <c>OneTerminal</c> framework for interactive applications with custom UI routing.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <typeparam name="TRouter">The type implementing <see cref="ITerminalRouter{TContext}"/>.</typeparam>
        /// <typeparam name="TContext">The type of the router context derived from <see cref="TerminalRouterContext"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// Configures the terminal for user-interactive applications with custom UI routing. Registers <typeparamref name="TRouter"/>
        /// with <typeparamref name="TContext"/> for custom input routing and <typeparamref name="TConsole"/> for logging.
        /// </remarks>
        public static ITerminalBuilder AddTerminalCli<TStore, THelp, TException, TConsole, TRouter, TContext>(
            this IServiceCollection services,
            ITerminalTextHandler textHandler,
            Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
            where TConsole : class, ITerminalConsole
            where TRouter : class, ITerminalRouter<TContext>
            where TContext : TerminalRouterContext
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, THelp, TException>(textHandler, setupAction)
                           .AddConsole<TConsole>()
                           .AddTerminalRouter<TRouter, TContext>();
        }

        /// <summary>
        /// Adds the <c>OneTerminal</c> framework for non-interactive client applications.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// Configures the terminal for automated client applications. The router must be added separately using
        /// <see cref="ITerminalBuilderExtensions.AddTerminalRouter{TRouter, TContext}(ITerminalBuilder)"/> based on
        /// the client protocol (e.g., TCP, HTTP, gRPC).
        /// </remarks>
        public static ITerminalBuilder AddTerminalClient<TStore, THelp, TException, TConsole>(
            this IServiceCollection services,
            ITerminalTextHandler textHandler,
            Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
            where TConsole : class, ITerminalConsole
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, THelp, TException>(textHandler, setupAction)
                           .AddConsole<TConsole>();
        }

        /// <summary>
        /// Adds the <c>OneTerminal</c> framework for non-interactive server applications.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// Configures the terminal for automated server applications. The router must be added separately using
        /// <see cref="ITerminalBuilderExtensions.AddTerminalRouter{TRouter, TContext}(ITerminalBuilder)"/> based on
        /// the server protocol (e.g., TCP, HTTP, gRPC, UDP, Pulsar).
        /// </remarks>
        public static ITerminalBuilder AddTerminalServer<TStore, THelp, TException, TConsole>(
            this IServiceCollection services,
            ITerminalTextHandler textHandler,
            Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
            where TConsole : class, ITerminalConsole
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, THelp, TException>(textHandler, setupAction)
                           .AddConsole<TConsole>();
        }

        /// <summary>
        /// Adds the default terminal services.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        private static ITerminalBuilder AddTerminalDefault<TStore, THelp, TException>(
            this IServiceCollection services,
            ITerminalTextHandler textHandler,
            Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);

            return services.CreateTerminalBuilder(textHandler)
                           .AddConfigurationOptions()
                           .AddCommandStore<TStore>()
                           .AddProcessor<TerminalProcessor>()
                           .AddLicensing()
                           .AddCommandRouter<CommandRouter, CommandHandler, CommandResolver>()
                           .AddCommandParser<CommandParser, TerminalRequestQueueParser>()
                           .AddOptionChecker<DataTypeMapper<Option>, OptionChecker>()
                           .AddArgumentChecker<DataTypeMapper<Argument>, ArgumentChecker>()
                           .AddExceptionHandler<TException>()
                           .AddHelpProvider<THelp>()
                           .AddBytesParser<TerminalBytesParser>();
        }

        /// <summary>
        /// Creates a <see cref="ITerminalBuilder"/> for configuring terminal services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        internal static ITerminalBuilder CreateTerminalBuilder(this IServiceCollection services, ITerminalTextHandler textHandler)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            TerminalBuilder tb = new(services, textHandler);
            tb.AddTextHandler(textHandler);
            return tb;
        }
    }
}