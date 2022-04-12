﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing.Models;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// A <c>cli</c> license.
    /// </summary>
    public sealed class License : System.ComponentModel.License
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="providerTenantId">The license provider tenant id.</param>
        /// <param name="checkMode">The license check mode.</param>
        /// <param name="plan">The license plan.</param>
        /// <param name="usage">The license usage.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="claims">The license claims.</param>
        /// <param name="limits">The license limits.</param>
        public License(string providerTenantId, string checkMode, string plan, string usage, string licenseKey, LicenseClaimsModel claims, LicenseLimits limits)
        {
            ProviderTenantId = providerTenantId;
            CheckMode = checkMode;
            Plan = plan;
            Usage = usage;
            this.licenseKey = licenseKey;
            Limits = limits;
            Claims = claims;
        }

        /// <summary>
        /// The license check mode.
        /// </summary>
        public string CheckMode { get; }

        /// <summary>
        /// The license claims.
        /// </summary>
        public LicenseClaimsModel Claims { get; }

        /// <summary>
        /// The license key.
        /// </summary>
        public override string LicenseKey => licenseKey;

        /// <summary>
        /// The license limits.
        /// </summary>
        public LicenseLimits Limits { get; }

        /// <summary>
        /// The license plan.
        /// </summary>
        public string Plan { get; }

        /// <summary>
        /// The license provider tenant id.
        /// </summary>
        public string ProviderTenantId { get; }

        /// <summary>
        /// The license usage.
        /// </summary>
        public string Usage { get; }

        /// <summary>
        /// Disposes the license.
        /// </summary>
        public override void Dispose()
        {
        }

        private readonly string licenseKey;
    }
}