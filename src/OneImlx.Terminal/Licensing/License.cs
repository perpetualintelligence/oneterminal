﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// A terminal framework license.
    /// </summary>
    public sealed class License : System.ComponentModel.License
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="plan">The license plan.</param>
        /// <param name="usage">The license usage.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="claims">The license claims.</param>
        /// <param name="quota">The license quota.</param>
        public License(string plan, string usage, string licenseKey, LicenseClaims claims, LicenseQuota quota)
        {
            Plan = plan;
            Usage = usage;
            this.licenseKey = licenseKey;
            Quota = quota;
            Claims = claims;
        }

        /// <summary>
        /// The license claims.
        /// </summary>
        public LicenseClaims Claims { get; }

        /// <summary>
        /// The license key.
        /// </summary>
        public override string LicenseKey => licenseKey;

        /// <summary>
        /// The license quota.
        /// </summary>
        public LicenseQuota Quota { get; }

        /// <summary>
        /// The license plan.
        /// </summary>
        public string Plan { get; }

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
