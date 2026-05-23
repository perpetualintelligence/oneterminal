//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

// This is used to unit test the behavior of the terminal framework's internal types.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OneImlx.Terminal.Tests")]

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Terminals, also known as command lines, consoles, or CLI applications, allow organizations and users to
    /// accomplish and automate tasks on a computer without using a graphical user interface. If a CLI terminal supports
    /// user interaction, the UX is the terminal.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance.
    /// </remarks>
    /// <param name="id">The terminal identifier.</param>
    public sealed class Terminal(string id)
    {
        /// <summary>
        /// The terminal identifier.
        /// </summary>
        public string Id { get; } = id;
    }
}