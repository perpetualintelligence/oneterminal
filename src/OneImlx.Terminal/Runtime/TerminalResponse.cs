﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// A response generated by a <see cref="TerminalRequest"/>.
    /// </summary>
    public sealed class TerminalResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalResponse"/> class.
        /// </summary>
        /// <param name="count">The number of requests per batch.</param>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="senderId">The sender identifier.</param>
        /// <param name="senderEndpoint">The sender endpoint.</param>
        public TerminalResponse(int count, string? batchId, string? senderId, string? senderEndpoint)
        {
            if (count == 0)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "At least one request must be provided.");
            }

            if (count > 1 && string.IsNullOrWhiteSpace(batchId))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "Batch id must be provided for multiple requests.");
            }

            Requests = new TerminalRequest[count];
            Results = new object?[count];

            BatchId = batchId;
            SenderId = senderId;
            SenderEndpoint = senderEndpoint;
        }

        /// <summary>
        /// The batch identifier if the response represents a batch response for multiple requests.
        /// </summary>
        public string? BatchId { get; }

        /// <summary>
        /// The terminal requests.
        /// </summary>
        public TerminalRequest[] Requests { get; }

        /// <summary>
        /// The command router results.
        /// </summary>
        public object?[] Results { get; }

        /// <summary>
        /// The sender endpoint.
        /// </summary>
        public string? SenderEndpoint { get; set; }

        /// <summary>
        /// The sender identifier.
        /// </summary>
        public string? SenderId { get; set; }
    }
}
