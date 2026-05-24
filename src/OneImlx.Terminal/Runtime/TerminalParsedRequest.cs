//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;
using System.Collections.Generic;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The raw representation of a parsed <see cref="CommandRequest"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TerminalParsedRequest"/> class.
    /// </remarks>
    /// <param name="tokens">The parsed tokens that represent a root, groups, command, and arguments.</param>
    /// <param name="options">The parsed options.</param>
    public sealed class TerminalParsedRequest(IEnumerable<string> tokens, Dictionary<string, ValueTuple<string, bool>> options)
    {
        /// <summary>
        /// Gets the parsed options.
        /// </summary>
        public Dictionary<string, ValueTuple<string, bool>> Options { get; } = options;

        /// <summary>
        /// Gets the parsed tokens that represent an ordered collection of root, groups, command, and arguments.
        /// </summary>
        public IEnumerable<string> Tokens { get; } = tokens;
    }
}