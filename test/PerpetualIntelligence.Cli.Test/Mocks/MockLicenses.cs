﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Shared.Licensing;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Mocks
{
    public static class MockLicenses
    {
        static MockLicenses()
        {
            TestClaims = LicenseClaimsModel.Create(new Dictionary<string, object>()
            {
                {"name", "test_name" },
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"exp", System.DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
                {"iat", System.DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() },
                {"nbf",  System.DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds() },
            });

            TestLimits = LicenseLimits.Create(LicensePlans.Community);
            TestPrice = LicensePrice.Create(LicensePlans.Community);

            TestLicense = new License("testProviderId1", Handlers.OfflineHandler, LicensePlans.Community, LicenseUsages.RnD, LicenseSources.JsonFile, "testLicKey1", TestClaims, TestLimits, TestPrice);
        }

        public static LicenseClaimsModel TestClaims = null!;
        public static License TestLicense = null!;
        public static LicenseLimits TestLimits = null!;
        public static LicensePrice TestPrice = null!;
    }
}