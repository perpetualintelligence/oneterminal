//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Testing.Mocks
{
    internal class MockListLogger : ILogger
    {
        private readonly List<string> logMessages;

        public MockListLogger(List<string> allLogMessages)
        {
            this.logMessages = allLogMessages;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            logMessages.Add(formatter(state, exception));
        }
    }
}