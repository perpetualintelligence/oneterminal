﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using System.Collections.Generic;

namespace OneImlx.Terminal.Mocks
{
    public static class MockLicenses
    {
        static MockLicenses()
        {
            TestClaims = LicenseClaims.Create(new Dictionary<string, object>()
            {
                {"name", "test_name" },
                {"oid", "test_id" },
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"mode", "test_mode" },
                { "deployment", "test_deployment" },
                {"exp", System.DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
                {"iat", System.DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() },
                {"nbf",  System.DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds() },
            });

            TestLimits = LicenseLimits.Create(TerminalLicensePlans.Demo);
            TestPrice = LicensePrice.Create(TerminalLicensePlans.Demo);

            TestLicense = new License(TerminalLicensePlans.Demo, LicenseUsage.RnD, "testLicKey1", TestClaims, TestLimits, TestPrice);
        }

        public static LicenseClaims TestClaims = null!;
        public static License TestLicense = null!;
        public static LicenseLimits TestLimits = null!;
        public static LicensePrice TestPrice = null!;
    }
}