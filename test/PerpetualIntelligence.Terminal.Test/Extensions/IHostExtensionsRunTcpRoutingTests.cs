﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

#if false

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Runtime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [Collection("Sequential")]
    public class IHostExtensionsRunTcpRoutingTests : IAsyncLifetime
    {
        [Fact]
        public async Task RunAsync_Should_Throw_Exception_If_IPEndPoint_Is_Null()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            Func<Task> act = async () => await host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(null!, startContext));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The network IP endpoint is missing in the TCP server routing request.");
        }

        /// <summary>
        /// Verifies that the server runs indefinitely and stops when the cancellation token is triggered.
        /// </summary>
        /// <remarks>
        /// Test Steps:
        /// <list type="number">
        ///   <item>Starts the TCP routing asynchronously by calling <c>host.RunTcpRoutingAsync</c>.</item>
        ///   <item>Waits for either <c>routingTask</c> or a 10-second delay using <c>Task.WhenAny</c>.</item>
        ///   <item>Checks that <c>routingTask</c> is not completed after the 10-second wait, confirming that it's running indefinitely.</item>
        ///   <item>Issues a cancellation by calling <c>tokenSource.CancelAfter(2000)</c> to stop the server after 2 seconds.</item>
        ///   <item>Waits for 2.5 seconds using <c>await Task.Delay(2500)</c> to ensure that the cancellation is processed and <c>routingTask</c> is completed.</item>
        ///   <item>Verifies that <c>routingTask</c> is completed, confirming that the server stopped after the cancellation.</item>
        /// </list>
        /// </remarks>
        [Fact]
        public async Task RunAsync_Should_Run_Indefinitely()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Wait for 10 seconds and make sure routingTask is still running
            await Task.WhenAny(routingTask, Task.Delay(10000));

            // Server is not yet complete
            routingTask.IsCompletedSuccessfully.Should().BeFalse();

            // Stop the server by issuing a cancellation
            tokenSource.CancelAfter(2000);

            // Wait for routingTask to complete
            await routingTask;

            // Verify that the server runs indefinitely and stops when the cancellation token is triggered
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Should_Start_And_Stop_Server()
        {
            // Create a new host builder and configure the services
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Set the timeout to infinite to avoid cancellation during the test
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously with the cancellation token
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Cancel the server after running for a while
            tokenSource.CancelAfter(2000); // Wait for 2 seconds and then cancel the server

            // Wait for the server task to complete
            await routingTask;

            // Ensure that the server task has completed successfully
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public void Setup_Sets_Client_And_Server()
        {
            var context = new TerminalTcpRoutingContext(serverIpEndPoint, startContext);
            context.Server.Should().BeNull();
            context.Client.Should().BeNull();

            context.Setup(new TcpListener(IPAddress.Loopback, 23), new TcpClient(new IPEndPoint(IPAddress.IPv6Any, 45)));
            context.Server.Should().NotBeNull();
            context.Server!.LocalEndpoint.ToString().Should().Be("127.0.0.1:23");
            context.Server.LocalEndpoint.As<IPEndPoint>().Port.Should().Be(23);

            context.Client.Should().NotBeNull();
            context.Client!.Client!.LocalEndPoint!.ToString().Should().Be("[::]:45");
            context.Client.Client.LocalEndPoint.As<IPEndPoint>().Port.Should().Be(45);
        }

        [Fact]
        public async Task RunAsync_Should_Stop_Server_On_Cancellation()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Wait for 3 seconds and make sure routingTask is still running
            await Task.WhenAny(routingTask, Task.Delay(3000));

            // Server is not yet complete
            routingTask.IsCompletedSuccessfully.Should().BeFalse();

            // Stop the server by issuing a cancellation
            tokenSource.Cancel();

            // Wait for routingTask to complete or timeout after 500 seconds
            routingTask.Wait(500);

            // Verify that the server stops on cancellation
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public void RunAsync_Should_Stop_Server_And_Client_On_Cancellation()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);
            });

            // Cancel both the server and client tasks after a delay
            tokenSource.CancelAfter(3000);

            // Wait for client and server tasks to complete
            Task.WhenAll(routingTask, clientTask).Wait(5000);

            // Verify that both server and client tasks stopped on cancellation
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Should_Throw_InvalidConfiguration_For_Invalid_StartMode()
        {
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Custom), tokenSource.Token);
            var context = new TerminalTcpRoutingContext(serverIpEndPoint, startContext);
            Func<Task> act = async () => await host.RunTcpRoutingAsync(context);
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The requested start mode is not valid for console routing. start_mode=Custom");
        }

        [Fact(Skip = "Fix this test")]
        public async Task HandleClientConnected_Should_Route_Command_To_Router()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            tokenSource.CancelAfter(2000); // Wait for 2 seconds and then cancel the server

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command from the client to the server
                byte[] messageBytes = Encoding.Unicode.GetBytes(GetCliOptions(host).DelimitedCommandString("Test command"));
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandString.Should().Be("Test command");

            // Verify that the server and client tasks have completed
            await routingTask;
            await clientTask;
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task HandleClientConnected_Throw_Exception_If_PackageData_Exceeds_LimitAsync()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                byte[] messageBytes = Encoding.Unicode.GetBytes(GetCliOptions(host).DelimitedCommandString(new string('A', 10000))); // Length exceeds the limit
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Client handling throws an exception
            Task.WaitAny(routingTask, clientTask);
            routingTask.IsCompletedSuccessfully.Should().BeFalse();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();

            // Wait for .5 seconds and make sure tasks are completed
            await Task.Delay(500);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("The command string length is over the configured limit. max_length=1024");

            // Cancel the token source to stop the server and client (if not already completed)
            tokenSource.Cancel();

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact(Skip = "Fix this test")]
        public async Task HandleClientConnected_Handles_Delimited_Messages_CorrectlyAsync()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                string testString = GetCliOptions(host).DelimitedCommandString("rt1 grp1 cmd1", "rt2 grp2 cmd2", "rt3 grp3 cmd3");

                byte[] messageBytes = Encoding.Unicode.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Cancel the token source to stop the server and client
            tokenSource.CancelAfter(2000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(3);
            mockCommandRouter.MultipleRawString[0].Should().Be("rt1 grp1 cmd1");
            mockCommandRouter.MultipleRawString[1].Should().Be("rt2 grp2 cmd2");
            mockCommandRouter.MultipleRawString[2].Should().Be("rt3 grp3 cmd3");

            // Verify that the server and client tasks have completed
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Multiple_Delimited_Messages_CorrectlyAsync()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a command that exceeds the configured command string length limit
                string testString = GetCliOptions(host).DelimitedCommandString("rt1 grp1 cmd1", "rt2 grp2 cmd2", "rt3 grp3 cmd3");
                byte[] messageBytes = Encoding.Unicode.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);

                await Task.Delay(3000);

                // Send a command that exceeds the configured command string length limit
                testString = GetCliOptions(host).DelimitedCommandString("rt4 grp4 cmd4", "rt5 grp5 cmd5", "rt6 grp6 cmd6");
                messageBytes = Encoding.Unicode.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Cancel the token source to stop the server and client
            tokenSource.CancelAfter(6000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(6);
            mockCommandRouter.MultipleRawString[0].Should().Be("rt1 grp1 cmd1");
            mockCommandRouter.MultipleRawString[1].Should().Be("rt2 grp2 cmd2");
            mockCommandRouter.MultipleRawString[2].Should().Be("rt3 grp3 cmd3");
            mockCommandRouter.MultipleRawString[3].Should().Be("rt4 grp4 cmd4");
            mockCommandRouter.MultipleRawString[4].Should().Be("rt5 grp5 cmd5");
            mockCommandRouter.MultipleRawString[5].Should().Be("rt6 grp6 cmd6");

            // Verify that the server and client tasks have completed
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact(Skip = "Fix this test")]
        public async Task HandleClientConnected_Handles_Large_Delimited_Messages_CorrectlyAsync()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Create a large command string with 100 commands
                List<string> commandList = new();
                for (int i = 0; i < 100; i++)
                {
                    commandList.Add($"rt{i} grp{i} cmd{i}");
                }
                string testString = GetCliOptions(host).DelimitedCommandString(commandList.ToArray());

                byte[] messageBytes = Encoding.Unicode.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Cancel the token source to stop the server and client
            tokenSource.CancelAfter(5000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(100);
            for (int idx = 0; idx < 100; idx++)
            {
                mockCommandRouter.MultipleRawString[idx].Should().Be($"rt{idx} grp{idx} cmd{idx}");
            }

            // Verify that the server and client tasks have completed
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task HandleClientConnected_Handles_Very_Large_Delimited_Messages_CorrectlyAsync()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Create a large command string with 1000 commands
                List<string> commandList = new();
                for (int i = 0; i < 1000; i++)
                {
                    commandList.Add($"rt{i} grp{i} cmd{i}");
                }
                string testString = GetCliOptions(host).DelimitedCommandString(commandList.ToArray());

                byte[] messageBytes = Encoding.Unicode.GetBytes(testString);
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Cancel the token source to stop the server and client
            tokenSource.CancelAfter(5000);

            // Wait for both the routing task and the client task to complete
            await Task.WhenAll(routingTask, clientTask);

            // Assert
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.MultipleRawString.Count.Should().Be(1000);
            for (int idx = 0; idx < 1000; idx++)
            {
                mockCommandRouter.MultipleRawString[idx].Should().Be($"rt{idx} grp{idx} cmd{idx}");
            }

            // Verify that the server and client tasks have completed
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Should_Stop_Server_And_Multiple_Clients_On_Cancelation()
        {
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Stop the server by issuing a cancellation
            tokenSource.CancelAfter(8000);

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Number of clients to simulate
            int numClients = 5;

            // List to store client tasks
            var clientTasks = new List<Task>();
            for (int i = 0; i < numClients; i++)
            {
                // Simulate a client connecting to the server and sending a unique message
                var clientTask = Task.Run(async () =>
                {
                    await Task.Delay(1000); // Wait for 1 second to stagger the connections
                    using var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);
                });

                clientTasks.Add(clientTask);
            }

            // Wait for all the client tasks to complete or timeout after 10 seconds
            List<Task> allTasks = new(clientTasks)
            {
                routingTask
            };
            await Task.WhenAll(allTasks);

            // Verify that the server runs indefinitely and stops when the cancellation token is triggered
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            for (int i = 0; i < numClients; i++)
            {
                clientTasks[i].IsCompletedSuccessfully.Should().BeTrue();
            }
        }

        [Fact]
        public async Task HandleClientConnected_Should_Handle_Malformed_Delimited_Messages_Async()
        {
            // Arrange
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Cancel so that the client does not go into an infinite loop during stream.Read
            tokenSource.CancelAfter(2000);

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Simulate a client connecting to the server after a short delay and sending a malformed delimited message
            var connectTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1000 milliseconds before connecting
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a malformed delimited message (without proper delimiters)
                byte[] messageBytes = Encoding.Unicode.GetBytes("Malformed message without delimiters");
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Act
            // Wait for both routingTask and connectTask to complete
            await Task.WhenAll(routingTask, connectTask);

            // Assert
            // Check if the client is connected and the server handles the malformed message correctly
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            connectTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Exception_In_Routing_Process_Async()
        {
            // Arrange
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesErrorExceptionOnRoute);
            host = newHostBuilder.Build();

            // Set the timeout to infinite to avoid cancellation during the test
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start the client task and send a valid delimited message
            var clientTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIpEndPoint.Address, serverIpEndPoint.Port);

                // Send a valid delimited message
                byte[] messageBytes = Encoding.Unicode.GetBytes(GetCliOptions(host).DelimitedCommandString("Test command"));
                await tcpClient.GetStream().WriteAsync(messageBytes);
            });

            // Wait for both the routing task and the client task to complete
            int idx = Task.WaitAny(routingTask, clientTask);
            routingTask.IsCompletedSuccessfully.Should().BeFalse();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();

            // Wait for .5 seconds and make sure tasks are completed
            await Task.Delay(500);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("test_error_description. opt1=test1 opt2=test2");

            tokenSource.Cancel();
            Task.WaitAll(routingTask, clientTask);
            routingTask.IsCompletedSuccessfully.Should().BeTrue();
            clientTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [Fact]
        public async Task RunAsync_Should_Not_Stop_Server_After_Timeout_Async()
        {
            // Arrange
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Start the TCP routing asynchronously with a timeout value (e.g., 2 seconds)
            var timeout = 2000; // 2 seconds
            GetCliOptions(host).Router.Timeout = timeout;
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Wait for the routing task to complete or timeout
            bool success = await Task.WhenAny(routingTask, Task.Delay(timeout + 1000)) == routingTask;

            // Verify that the server stops on cancellation
            success.Should().BeFalse();
        }

        [Fact(Skip = "Fix this test")]
        public async Task Multiple_Clients_Should_Send_Commands_Concurrently()
        {
            // Arrange
            var newHostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // Set the timeout to infinite to avoid cancellation during the test
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start the TCP routing asynchronously
            var routingTask = host.RunTcpRoutingAsync(new TerminalTcpRoutingContext(serverIpEndPoint, startContext));

            // Start multiple client tasks and send a valid delimited message
            const int numClients = 5;
            Task[] _clientTasks = new Task[numClients];
            var multipleClientsTask = Task.Run(async () =>
            {
                await Task.Delay(1000); // Wait for 1 second

                for (int idx = 0; idx < numClients; idx++)
                {
                    _clientTasks[idx] = Task.Run(async () =>
                    {
                        using var tcpClient = new TcpClient();
                        await tcpClient.ConnectAsync(serverIpEndPoint);
                        tcpClient.Connected.Should().BeTrue();

                        var command = $"Client-{idx} sent test command";
                        var commandBytes = Encoding.Unicode.GetBytes(GetCliOptions(host).DelimitedCommandString(command));
                        await tcpClient.GetStream().WriteAsync(commandBytes);
                    });
                }
            });

            tokenSource.CancelAfter(5000); // Stop the server

            await Task.WhenAll(routingTask, multipleClientsTask);

            // Verify that all client tasks are completed
            for (int i = 0; i < numClients; i++)
            {
                await _clientTasks[i];
                _clientTasks[i].IsCompletedSuccessfully.Should().BeTrue();
            }

            await routingTask; // Wait for the routing task to complete
            routingTask.IsCompletedSuccessfully.Should().BeTrue();

            // Assert result
            var mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            //mockCommandRouter.RouteCounter.Should().Be(numClients);
            mockCommandRouter.MultipleRawString.Count.Should().Be(numClients);
            mockCommandRouter.MultipleRawString.Should().OnlyHaveUniqueItems();
        }

        private static TerminalOptions GetCliOptions(IHost host)
        {
            return host.Services.GetRequiredService<TerminalOptions>();
        }

        private void ConfigureServicesDefault(IServiceCollection opt2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Tcp), tokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
            opt2.AddSingleton<TerminalTcpRouting>();
            opt2.AddSingleton<ITextHandler, UnicodeTextHandler>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesErrorExceptionOnRoute(IServiceCollection opt2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Tcp), tokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, null, new ErrorException("test_error_code", "test_error_description. opt1={0} opt2={1}", "test1", "test2")));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
            opt2.AddSingleton<TerminalTcpRouting>();
            opt2.AddSingleton<ITextHandler, UnicodeTextHandler>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private IHost host = null!;
        private CancellationTokenSource tokenSource = null!;
        private TerminalStartContext startContext = null!;
        private IPEndPoint serverIpEndPoint = null!;

        public Task InitializeAsync()
        {
            var hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(ConfigureServicesDefault);
            host = hostBuilder.Build();

            serverIpEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            host?.Dispose();
            return Task.CompletedTask;
        }
    }
}
#endif