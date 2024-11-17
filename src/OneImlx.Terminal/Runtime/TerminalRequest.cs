﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// A <see cref="ITerminalProcessor"/> request that is equatable over its identifier.
    /// </summary>
    public sealed class TerminalRequest : IEquatable<TerminalRequest?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalRequest"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the command item.</param>
        /// <param name="raw">The raw command string to be processed.</param>
        public TerminalRequest(string id, string raw)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
            Raw = raw;
        }

        /// <summary>
        /// Gets the unique identifier for the command item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The raw command or a batch that needs to be processed.
        /// </summary>
        public string Raw { get; }

        /// <inheritdoc/>
        public static bool operator !=(TerminalRequest? left, TerminalRequest? right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public static bool operator ==(TerminalRequest? left, TerminalRequest? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as TerminalRequest);
        }

        /// <inheritdoc/>
        public bool Equals(TerminalRequest? other)
        {
            return other is not null &&
                   Id == other.Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Raw}";
        }
    }
}
