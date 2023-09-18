﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalLogger"/> scope.
    /// </summary>
    public sealed class TerminalConsoleLoggerScope<TState> : IDisposable
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="state">The state identifier.</param>
        /// <param name="terminalConsoleLogger">The terminal console logger.</param>
        public TerminalConsoleLoggerScope(TState state, TerminalConsoleLogger terminalConsoleLogger)
        {
            State = state;
            TerminalConsoleLogger = terminalConsoleLogger;
            TerminalConsoleLogger.Scopes.Add(this);
        }

        /// <summary>
        /// The state identifier.
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// The console logger.
        /// </summary>
        public TerminalConsoleLogger TerminalConsoleLogger { get; }

        /// <summary>
        /// Disposes the console scope.
        /// </summary>
        public void Dispose()
        {
            TerminalConsoleLogger.Scopes.Remove(this);
        }
    }
}