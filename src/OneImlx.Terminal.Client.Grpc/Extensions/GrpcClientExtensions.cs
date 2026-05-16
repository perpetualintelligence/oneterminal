//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using OneImlx.Terminal.Shared;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Client.Grpc.Extensions
{
    /// <summary>
    /// Provides extension methods for interacting with terminal server over gRPC.
    /// </summary>
    public static class GrpcClientExtensions
    {
        /// <summary>
        /// Sends a <see cref="TerminalInputOutput"/> object to a terminal server via a gRPC request.
        /// </summary>
        /// <param name="grpcClient">The gRPC client instance used to send the request.</param>
        /// <param name="input">The <see cref="TerminalInputOutput"/> to be sent.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <param name="serializeOptions">
        /// The <see cref="JsonSerializerOptions"/> used to serialize the input. Defaults to <c>null</c>.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the
        /// <see cref="TerminalGrpcRouterProtoOutput"/> from the server.
        /// </returns>
        public static async Task<TerminalGrpcRouterProtoOutput> SendToTerminalAsync(this TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient grpcClient, TerminalInputOutput input, CancellationToken cancellationToken, JsonSerializerOptions? serializeOptions = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "The input cannot be null.");
            }

            serializeOptions ??= JsonSerializerOptions.Default;
            var protoInput = new TerminalGrpcRouterProtoInput { InputJson = JsonSerializer.Serialize(input, serializeOptions) };
            return await grpcClient.RouteCommandAsync(protoInput, cancellationToken: cancellationToken);
        }
    }
}