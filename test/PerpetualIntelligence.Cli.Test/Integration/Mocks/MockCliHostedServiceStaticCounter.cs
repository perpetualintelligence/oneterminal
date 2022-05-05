﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Integration.Mocks
{
    public static class MockCliHostedServiceStaticCounter
    {
        public static int Increment()
        {
            Counter++;
            return Counter;
        }

        public static void Restart()
        {
            Counter = 0;
        }

        private static int Counter { get; set; }
    }
}