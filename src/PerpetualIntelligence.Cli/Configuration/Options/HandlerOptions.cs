﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Handlers;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> handler options.
    /// </summary>
    public class HandlerOptions
    {
        /// <summary>
        /// The data type handler. Its value can be <c>null</c>, <c>default</c> or <c>custom</c>. The <c>default</c> and
        /// <c>custom</c> are reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para><c>null</c> indicates no data type handler.</para>
        /// <para><c>default</c> to be defined.</para>
        /// <para><c>custom</c> to be defined.</para>
        /// </remarks>
        public string? DataTypeHandler { get; set; }

        /// <summary>
        /// The hosting and routing error handler. Its value can be <c>default</c> or <c>custom</c>. The command router
        /// receives an error or exception during the command routing, extraction, checker, or execution. On error, it
        /// forwards it to the <see cref="IErrorHandler"/> or <see cref="IExceptionHandler"/>.
        /// </summary>
        /// <remarks>
        /// <para><c>default</c> handler prints the error information in the CLI terminal.</para>
        /// <para>
        /// <c>custom</c> handler allows application authors to define a custom error handler to process and publish the
        /// error according to their needs. E.g., app authors can publish the errors to a central log or on their cloud
        /// backend for audit purposes.
        /// </para>
        /// </remarks>
        /// <seealso cref="IErrorHandler"/>
        /// <seealso cref="Commands.Handlers.ErrorHandler"/>
        public string ErrorHandler { get; set; } = "default";

        /// <summary>
        /// The license handler. Its value can be <c>online-license</c>, <c>offline-license</c>, or <c>dev-license</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>online-license</c> handler checks your license key online. Your CLI terminal needs network access during startup.
        /// </para>
        /// <para>
        /// <c>offline-license</c> handler checks your license key offline. Your CLI terminal does not need network access
        /// during startup.
        /// </para>
        /// <para>
        /// <c>dev-license</c> handler allows you to bring you own license certificate to check the license key. Your CLI
        /// terminal may need network access during startup
        /// </para>
        /// </remarks>
        public string LicenseHandler { get; set; } = "online-license";

        /// <summary>
        /// The hosting and routing dependency injection services. Its value can be <c>default</c> or <c>custom</c>. The
        /// <c>custom</c> is reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>default</c> handler provides default DI service implementations for command router, extractor, handler,
        /// mapper, checker and invoking runner.
        /// </para>
        /// <para><c>custom</c> handler allows application authors to define a custom DI services implementations.</para>
        /// </remarks>
        public string ServiceHandler { get; set; } = "default";

        /// <summary>
        /// The command and option store handler. Its value can be <c>in-memory</c>, <c>json</c> or <c>custom</c>. The
        /// <c>json</c> or <c>custom</c> are reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para><c>in-memory</c> handler provides in memory command and option descriptions.</para>
        /// <para><c>json</c> handler provides command and option descriptions in a JSON file.</para>
        /// <para>
        /// <c>custom</c> handler allows application authors to provide command and option descriptions from a custom
        /// store such as Entity framework or cloud databases REST API.
        /// </para>
        /// </remarks>
        public string StoreHandler { get; set; } = "in-memory";

        /// <summary>
        /// The hosting and routing command string text handler. Its value can be <c>unicode</c> or <c>ascii</c>. The
        /// <c>ascii</c> is reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para><c>unicode</c> handler supports Unicode command strings.</para>
        /// <para><c>ascii</c> handler supports ASCII command strings.</para>
        /// By default the value is set to <c>unicode</c>. Currently we only support <c>left-to-right</c> languages.
        /// </remarks>
        public string TextHandler { get; set; } = "unicode";
    }
}