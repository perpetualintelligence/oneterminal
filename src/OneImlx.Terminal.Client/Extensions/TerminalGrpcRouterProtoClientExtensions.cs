﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Client.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient"/> to
    /// interact with terminal servers via gRPC and for sending commands.
    /// </summary>
    /// <remarks>
    /// These methods interact with a terminal server using gRPC, supporting both single and batch command execution.
    /// Use these methods to send commands with optional delimiters, depending on the terminal's routing requirements.
    /// </remarks>
    public static class TerminalGrpcRouterProtoClientExtensions
    {
        /// <summary>
        /// Sends multiple command strings to a terminal server via gRPC in a single batch request.
        /// </summary>
        /// <param name="grpcClient">
        /// The <see cref="TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient"/> instance used to send the request.
        /// </param>
        /// <param name="commands">An array of command strings to send as a batch.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands in the batch.</param>
        /// <param name="msgDelimiter">The delimiter used to separate message parts.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>
        /// A <see cref="TerminalGrpcRouterProtoOutput"/> response containing the terminal's response to the batch command.
        /// </returns>
        /// <remarks>
        /// The batch command is created by concatenating the provided commands using the specified delimiters. This
        /// method is useful for reducing the number of individual requests when sending multiple commands.
        /// </remarks>
        public static async Task<TerminalGrpcRouterProtoOutput> SendBatchAsync(this TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient grpcClient, string[] commands, string cmdDelimiter, string msgDelimiter, CancellationToken cancellationToken)
        {
            string batch = TerminalServices.CreateBatch(cmdDelimiter, msgDelimiter, commands);
            TerminalGrpcRouterProtoOutput response = await grpcClient.RouteCommandAsync(new TerminalGrpcRouterProtoInput { Raw = batch }, cancellationToken: cancellationToken);
            return response;
        }

        /// <summary>
        /// Sends a single command string to a terminal server via gRPC with specified delimiters.
        /// </summary>
        /// <param name="grpcClient">
        /// The <see cref="TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient"/> instance used to send the request.
        /// </param>
        /// <param name="command">The command string to send.</param>
        /// <param name="cmdDelimiter">The delimiter used to separate commands.</param>
        /// <param name="msgDelimiter">The delimiter used to separate message parts.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>
        /// A <see cref="TerminalGrpcRouterProtoOutput"/> response representing the server's response to the single command.
        /// </returns>
        /// <remarks>
        /// The command is formatted using the provided delimiters and sent to the terminal server. The response
        /// contains the terminal's reaction to the command, useful for ensuring proper execution of commands.
        /// </remarks>
        public static async Task<TerminalGrpcRouterProtoOutput> SendSingleAsync(this TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient grpcClient, string command, string cmdDelimiter, string msgDelimiter, CancellationToken cancellationToken)
        {
            string batchedCommand = TerminalServices.CreateBatch(cmdDelimiter, msgDelimiter, [command]);
            TerminalGrpcRouterProtoOutput response = await grpcClient.RouteCommandAsync(new TerminalGrpcRouterProtoInput { Raw = batchedCommand }, cancellationToken: cancellationToken);
            return response;
        }

        /// <summary>
        /// Sends a single command string to a terminal server via gRPC without using any delimiters.
        /// </summary>
        /// <param name="grpcClient">
        /// The <see cref="TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient"/> instance used to send the request.
        /// </param>
        /// <param name="command">The command string to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while awaiting completion.</param>
        /// <returns>
        /// A <see cref="TerminalGrpcRouterProtoOutput"/> response representing the server's response to the command.
        /// </returns>
        /// <remarks>
        /// This method sends a single command to the terminal without delimiters. Delimiters are not required for
        /// single commands and are only necessary when sending batch commands.
        /// </remarks>
        public static async Task<TerminalGrpcRouterProtoOutput> SendSingleAsync(this TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient grpcClient, string command, CancellationToken cancellationToken)
        {
            TerminalGrpcRouterProtoOutput response = await grpcClient.RouteCommandAsync(new TerminalGrpcRouterProtoInput { Raw = command }, cancellationToken: cancellationToken);
            return response;
        }
    }
}
