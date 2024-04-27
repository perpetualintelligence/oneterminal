﻿/*
    Copyright (c) 2024 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The command router options.
    /// </summary>
    public sealed class RouterOptions
    {
        /// <summary>
        /// The terminal caret to show in the console. The default value is <c>&gt;</c>.
        /// </summary>
        public string Caret { get; set; } = ">";

        /// <summary>
        /// The maximum number of active remote client connections the router can accept. The default value is <c>5</c>.
        /// </summary>
        public int MaxRemoteClients { get; set; } = 5;

        /// <summary>
        /// The maximum length of a single unprocessed remote message. The default value is <c>1024</c> characters.
        /// </summary>
        /// <remarks>
        /// This is not the actual command string length, but the length of the message that is being streamed from a
        /// remote source.
        /// </remarks>
        public int MaxRemoteMessageLength { get; set; } = 1024;

        /// <summary>
        /// The delimiter to identify a complete message. The default value is <c>$EOM$</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="RemoteMessageDelimiter"/> is used while streaming a long command string from a remote source
        /// such as a network stream.
        /// </remarks>
        public string RemoteMessageDelimiter { get; set; } = "$EOM$";

        /// <summary>
        /// The command router timeout in milliseconds. The default value is <c>25</c> seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command route starts at a request to execute the command and ends when the command run is complete or at
        /// an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;
    }
}
