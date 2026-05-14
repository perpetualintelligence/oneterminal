//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// An abstraction of runner method builder.
    /// </summary>
    public interface IRunMethodBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Builds a new <see cref="RunMethodBuilder"/> and add it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        ICommandBuilder Add();
    }
}