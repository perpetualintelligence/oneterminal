//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="IRunMethodBuilder"/>.
    /// </summary>
    public class RunMethodBuilder(ICommandBuilder builder) : IRunMethodBuilder
    {
        private ICommandBuilder builder = builder;

        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// Builds an <see cref="RunMethodBuilder"/> and adds it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public ICommandBuilder Add()
        {
            // Get from local and add to
            ServiceProvider lsp = Services.BuildServiceProvider();
            var runMethod = lsp.GetRequiredService<RunMethod>();
            builder.Services.AddSingleton(runMethod);
            return builder;
        }
    }
}