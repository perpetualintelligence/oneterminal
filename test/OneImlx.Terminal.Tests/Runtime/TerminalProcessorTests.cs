﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalProcessorTests
    {
        public TerminalProcessorTests()
        {
            _terminalTokenSource = new CancellationTokenSource();
            terminalStartContext = new TerminalStartContext(TerminalStartMode.Console, _terminalTokenSource.Token, CancellationToken.None, null, null);
            _mockCommandRouter = new Mock<ICommandRouter>();
            _mockExceptionHandler = new Mock<ITerminalExceptionHandler>();
            _mockLogger = new Mock<ILogger<TerminalProcessor>>();
            _mockOptions = new Mock<IOptions<TerminalOptions>>();
            _mockTerminalRouterContext = new Mock<TerminalRouterContext>(terminalStartContext);
            _textHandler = new TerminalAsciiTextHandler();

            _mockOptions.Setup(static o => o.Value).Returns(new TerminalOptions
            {
                Router = new RouterOptions
                {
                    MaxLength = 1000,
                    EnableBatch = true,
                    BatchDelimiter = "|",
                    CommandDelimiter = ",",
                    Timeout = 1000
                }
            });

            _terminalProcessor = new TerminalProcessor(
                _mockCommandRouter.Object,
                _mockExceptionHandler.Object,
                _mockOptions.Object,
                _textHandler,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Add_Without_Processing_And_Background_Throws()
        {
            // Add with sender endpoint and sender id
            Func<Task> act = async () => await _terminalProcessor.AddAsync("command1|", "sender_1", "sender_endpoint_1");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running.");

            // Start but without background
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: false);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running a background queue.");
        }

        [Fact]
        public async Task AddAsync_Batch_Commands_HandlesConcurrentCalls()
        {
            _mockOptions.Object.Value.Router.EnableBatch = true;

            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandRouterResult(new Commands.Handlers.CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), new TerminalRequest("mock_id", "mock_command")));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            int idx = 0;
            var tasks = Enumerable.Range(0, 500).Select<int, Task>(e =>
            {
                ++idx;
                string batch = TerminalServices.CreateBatch(_mockOptions.Object.Value, [$"command_{idx}_0", $"command_{idx}_1", $"command_{idx}_2"]);
                return _terminalProcessor.AddAsync(batch, "sender", "endpoint");
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(1500);
        }

        [Fact]
        public async Task AddAsync_CallsExceptionHandler_WhenRouterFails()
        {
            _mockCommandRouter.Setup(static r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));

            // Act
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            await _terminalProcessor.AddAsync("command1|", "sender", "endpoint");
            await Task.Delay(500);

            // Assert exception handler was called
            _mockExceptionHandler.Verify(static e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Does_Add_When_BatchDelimiter_Missing_In_Non_BatchMode()
        {
            _mockOptions.Object.Value.Router.EnableBatch = false;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw));

            // Act
            await _terminalProcessor.AddAsync("command1", "sender", "endpoint");
            await Task.Delay(500);

            // Assert only a single command was processed
            routedCommands.Should().HaveCount(1);
            routedCommands.Should().Contain("command1");
        }

        [Fact]
        public async Task AddAsync_Processes_BatchCommand_In_Order_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableBatch = true;
            _mockOptions.Object.Value.Router.MaxLength = 1500000;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandRouterResult(new Commands.Handlers.CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), new TerminalRequest("mock_id", "mock_command")));

            // Create sets of commands to simulate batch processing
            List<string> commands1 = new(Enumerable.Range(0, 1000).Select(i => $"command_1_{i}"));
            List<string> commands2 = new(Enumerable.Range(0, 1000).Select(i => $"command_2_{i}"));
            List<string> commands3 = new(Enumerable.Range(0, 1000).Select(i => $"command_3_{i}"));
            List<string> commands4 = new(Enumerable.Range(0, 1000).Select(i => $"command_4_{i}"));
            List<string> commands5 = new(Enumerable.Range(0, 1000).Select(i => $"command_5_{i}"));
            List<string> commands6 = new(Enumerable.Range(0, 1000).Select(i => $"command_6_{i}"));
            List<string> commands7 = new(Enumerable.Range(0, 1000).Select(i => $"command_7_{i}"));
            List<string> commands8 = new(Enumerable.Range(0, 1000).Select(i => $"command_8_{i}"));
            List<string> commands9 = new(Enumerable.Range(0, 1000).Select(i => $"command_9_{i}"));
            List<string> commands10 = new(Enumerable.Range(0, 1000).Select(i => $"command_10_{i}"));

            // Create batches for each command collection
            var batch1 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands1.ToArray());
            var batch2 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands2.ToArray());
            var batch3 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands3.ToArray());
            var batch4 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands4.ToArray());
            var batch5 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands5.ToArray());
            var batch6 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands6.ToArray());
            var batch7 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands7.ToArray());
            var batch8 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands8.ToArray());
            var batch9 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands9.ToArray());
            var batch10 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands10.ToArray());

            // Add all batches asynchronously
            Task addBatch1 = _terminalProcessor.AddAsync(batch1, "sender1", "endpoint1");
            Task addBatch2 = _terminalProcessor.AddAsync(batch2, "sender2", "endpoint2");
            Task addBatch3 = _terminalProcessor.AddAsync(batch3, "sender3", "endpoint3");
            Task addBatch4 = _terminalProcessor.AddAsync(batch4, "sender4", "endpoint4");
            Task addBatch5 = _terminalProcessor.AddAsync(batch5, "sender5", "endpoint5");
            Task addBatch6 = _terminalProcessor.AddAsync(batch6, "sender6", "endpoint6");
            Task addBatch7 = _terminalProcessor.AddAsync(batch7, "sender7", "endpoint7");
            Task addBatch8 = _terminalProcessor.AddAsync(batch8, "sender8", "endpoint8");
            Task addBatch9 = _terminalProcessor.AddAsync(batch9, "sender9", "endpoint9");
            Task addBatch10 = _terminalProcessor.AddAsync(batch10, "sender10", "endpoint10");

            // Wait for all batches to be processed
            await Task.WhenAll(addBatch1, addBatch2, addBatch3, addBatch4, addBatch5, addBatch6, addBatch7, addBatch8, addBatch9, addBatch10);

            // Stop the processing with a timeout of 5000ms
            await _terminalProcessor.StopProcessingAsync(5000);

            // Verify all commands are processed
            routedCommands.Should().HaveCount(10000);
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);

            // Collect the processed commands into groups by their prefixes (e.g., "command_1_", "command_2_", etc.)
            var groupedCommands = routedCommands.GroupBy(r => r.Split('_')[1])
                .ToDictionary(g => g.Key, g => g.Select(r => r).ToList());

            // Verify that each group of commands is in the expected order
            groupedCommands["1"].Should().BeEquivalentTo(commands1, options => options.WithStrictOrdering());
            groupedCommands["2"].Should().BeEquivalentTo(commands2, options => options.WithStrictOrdering());
            groupedCommands["3"].Should().BeEquivalentTo(commands3, options => options.WithStrictOrdering());
            groupedCommands["4"].Should().BeEquivalentTo(commands4, options => options.WithStrictOrdering());
            groupedCommands["5"].Should().BeEquivalentTo(commands5, options => options.WithStrictOrdering());
            groupedCommands["6"].Should().BeEquivalentTo(commands6, options => options.WithStrictOrdering());
            groupedCommands["7"].Should().BeEquivalentTo(commands7, options => options.WithStrictOrdering());
            groupedCommands["8"].Should().BeEquivalentTo(commands8, options => options.WithStrictOrdering());
            groupedCommands["9"].Should().BeEquivalentTo(commands9, options => options.WithStrictOrdering());
            groupedCommands["10"].Should().BeEquivalentTo(commands10, options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("command1,command2,command3,command4,command5,command6|")]
        [InlineData("command1,command2,command3,command4,command5,command6,|")]
        [InlineData("command1,,,command2,command3,command4,command5,command6|")]
        public async Task AddAsync_Processes_BatchCommands_By_Ignoring_Empty_Commands(string message)
        {
            _mockOptions.Object.Value.Router.EnableBatch = true;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandRouterResult(new Commands.Handlers.CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), new TerminalRequest("mock_id", "mock_command")));

            await _terminalProcessor.AddAsync(message, "sender", "endpoint");

            await _terminalProcessor.StopProcessingAsync(5000);
            routedCommands.Should().HaveCount(6);
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);

            routedCommands[0].Should().Be("command1");
            routedCommands[1].Should().Be("command2");
            routedCommands[2].Should().Be("command3");
            routedCommands[3].Should().Be("command4");
            routedCommands[4].Should().Be("command5");
            routedCommands[5].Should().Be("command6");
        }

        [Fact]
        public async Task AddAsync_Processes_VeryLarge_BatchCommand_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableBatch = true;
            _mockOptions.Object.Value.Router.MaxLength = 1500000;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandRouterResult(new Commands.Handlers.CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), new TerminalRequest("mock_id", "mock_command")));

            // Send batch of 100000 commands by using TerminalServices
            HashSet<string> allCommands = new(Enumerable.Range(0, 100000).Select(i => $"command{i}"));
            var longBatch = TerminalServices.CreateBatch(_mockOptions.Object.Value, allCommands.ToArray());
            await _terminalProcessor.AddAsync(longBatch, "sender", "endpoint");

            await _terminalProcessor.StopProcessingAsync(5000);

            // We are iterating over a large number of unprocessed requests, so we need to ensure that the validation
            // code is not too slow. We are also checking that all commands are present in the batch at the same time
            // reducing the batch size so that the test does not take too long to run.
            routedCommands.Should().HaveCount(100000);
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0)
            ;
            foreach (var request in routedCommands)
            {
                if (allCommands.Contains(request))
                {
                    allCommands.Remove(request);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Fact]
        public async Task AddAsync_Single_Commands_HandlesConcurrentCalls()
        {
            _mockOptions.Object.Value.Router.EnableBatch = false;

            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            int idx = 1;
            var tasks = Enumerable.Range(0, 500).Select<int, Task>(e =>
            {
                return _terminalProcessor.AddAsync($"command{idx++}", "sender", "endpoint");
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(500);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddAsync_Throws_On_EmptyBatch(string? batch)
        {
            _mockOptions.Object.Value.Router.EnableBatch = false;
            Func<Task> act = () => _terminalProcessor.AddAsync(batch!, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw command or batch cannot be empty.");

            _mockOptions.Object.Value.Router.EnableBatch = true;
            act = () => _terminalProcessor.AddAsync(batch!, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw command or batch cannot be empty.");
        }

        [Fact]
        public async Task AddAsync_Throws_When_BatchDelimiter_Is_Larger_Than_Batch_In_BatchMode()
        {
            // Make sure delimter is enabled and greater than 1 character
            _mockOptions.Object.Value.Router.EnableBatch = true;
            _mockOptions.Object.Value.Router.BatchDelimiter = "$m$";

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            Func<Task> act = () => _terminalProcessor.AddAsync("c", "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw batch does not end with the batch delimiter.");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("command1")]
        [InlineData("|command1")]
        [InlineData("command1||")]
        [InlineData("com|mand1")]
        [InlineData("|com|mand1")]
        [InlineData("|command1|")]
        [InlineData("command1|command2|")]
        [InlineData("|command1|command2|")]
        [InlineData("|command1|command2")]
        public async Task AddAsync_Throws_When_BatchDelimiter_Is_Misplaced_In_BatchMode(string batch)
        {
            // Make sure delimter is enabled and greater than 1 character
            _mockOptions.Object.Value.Router.EnableBatch = true;

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            Func<Task> act = () => _terminalProcessor.AddAsync(batch, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw batch must have a single delimiter at the end, not missing or placed elsewhere.");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenBatchTooLong()
        {
            _mockOptions.Object.Value.Router.MaxLength = 1000;

            var longBatch = new string('A', 1001);
            Func<Task> act = () => _terminalProcessor.AddAsync(longBatch, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The raw command or batch length exceeds configured maximum. max_length=1000");
        }

        [Fact]
        public void ByDefault_Processor_Is_NotProcessing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();
        }

        [Fact]
        public void NewUniqueId_Generates_Unique_Id()
        {
            // Test default case
            string testId = _terminalProcessor.NewUniqueId();
            Guid.TryParse(testId, out _).Should().BeTrue();

            // Test case with null hint
            testId = _terminalProcessor.NewUniqueId(null);
            Guid.TryParse(testId, out _).Should().BeTrue();

            // Test case with whitespace hint
            testId = _terminalProcessor.NewUniqueId("   ");
            Guid.TryParse(testId, out _).Should().BeTrue();

            // Test case with an unrelated string as a hint
            testId = _terminalProcessor.NewUniqueId("blah");
            Guid.TryParse(testId, out _).Should().BeTrue();

            // Test case with the "short" hint
            for (int idx = 0; idx < 100; ++idx)
            {
                testId = _terminalProcessor.NewUniqueId("short");
                testId.Length.Should().Be(12);
                testId.Should().MatchRegex("^[a-fA-F0-9]{12}$", "Short ID should be 12 hexadecimal characters");
            }

            // Ensure uniqueness for short IDs
            HashSet<string> shortIds = [];
            for (int idx = 0; idx < 1000; ++idx)
            {
                testId = _terminalProcessor.NewUniqueId("short");
                shortIds.Add(testId).Should().BeTrue($"Short ID '{testId}' should be unique");
            }
        }

        [Fact]
        public async Task ProcessAsync_Routes_Batched_Commands_And_Processes_In_Order()
        {
            TerminalRequest testRequest = new("id1", "command1,command2,command3|");

            // Create mock command router results for each command
            CommandRouterResult routerResult1 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result1")),
                new TerminalRequest("id1", "command1|")
                                                   );

            CommandRouterResult routerResult2 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result2")),
                new TerminalRequest("id2", "command2|")
                                                   );

            CommandRouterResult routerResult3 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result3")),
                new TerminalRequest("id3", "command3|")
                                                   );

            // Arrange
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routeContext = c)
                .ReturnsAsync((CommandRouterContext context) =>
                {
                    return context.Request.Raw switch
                    {
                        "command1" => routerResult1,
                        "command2" => routerResult2,
                        "command3" => routerResult3,
                        _ => throw new InvalidOperationException("Unexpected command")
                    };
                });

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            var response = await _terminalProcessor.ProcessAsync("command1,command2,command3|", "sender_1", "sender_endpoint_1");

            // Assert route context and response
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            response.Should().NotBeNull();
            response.Commands.Should().HaveCount(3);

            // Assert first request and result
            response.Commands[0].Raw.Should().Be("command1");
            response.Results[0].Should().Be("sender_result1");

            // Assert second request and result
            response.Commands[1].Raw.Should().Be("command2");
            response.Results[1].Should().Be("sender_result2");

            // Assert third request and result
            response.Commands[2].Raw.Should().Be("command3");
            response.Results[2].Should().Be("sender_result3");
        }

        [Fact]
        public async Task ProcessAsync_Routes_Batched_Commands_With_Null_Value_Result()
        {
            TerminalRequest testRequest = new("id1", "command1,command2,command3,command4,command5|");

            // Create mock command router results: 4 valid and 1 null
            CommandRouterResult routerResult1 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result1")),
                new TerminalRequest("id1", "command1|")
                                                   );

            CommandRouterResult routerResult2 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result2")),
                new TerminalRequest("id2", "command2|")
                                                   );

            CommandRouterResult routerResult3 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result3")),
                new TerminalRequest("id3", "command3|")
                                                   );

            CommandRouterResult routerResult4 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult()), // Null Value
                new TerminalRequest("id4", "command4|")
                                                   );

            CommandRouterResult routerResult5 = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                new Commands.Runners.CommandRunnerResult("sender_result5")),
                new TerminalRequest("id5", "command5|")
                                                   );

            // Arrange
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routeContext = c)
                .ReturnsAsync((CommandRouterContext context) =>
                {
                    return context.Request.Raw switch
                    {
                        "command1" => routerResult1,
                        "command2" => routerResult2,
                        "command3" => routerResult3,
                        "command4" => routerResult4,
                        "command5" => routerResult5,
                        _ => throw new InvalidOperationException("Unexpected command")
                    };
                });

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            var response = await _terminalProcessor.ProcessRequestAsync("command1,command2,command3,command4,command5|", "sender_1", "sender_endpoint_1");

            // Assert route context and response
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            response.Should().NotBeNull();
            response.Commands.Should().HaveCount(5);

            // Assert requests and results
            response.Commands[0].Raw.Should().Be("command1");
            response.Results[0].Should().Be("sender_result1");

            response.Commands[1].Raw.Should().Be("command2");
            response.Results[1].Should().Be("sender_result2");

            response.Commands[2].Raw.Should().Be("command3");
            response.Results[2].Should().Be("sender_result3");

            response.Commands[3].Raw.Should().Be("command4");
            response.Results[3].Should().BeNull(); // Command4 returns null

            response.Commands[4].Raw.Should().Be("command5");
            response.Results[4].Should().Be("sender_result5");
        }

        [Fact]
        public async Task ProcessAsync_Routes_Command_And_Returns_Result()
        {
            TerminalRequest testRequest = new("id1", "command1|");

            // Create a mock command router result with
            CommandRouterResult routerResult = new(new Commands.Handlers.CommandHandlerResult(
                new Commands.Checkers.CommandCheckerResult(),
                    new Commands.Runners.CommandRunnerResult("sender_result")),
                testRequest
                                                  );

            // Arrange
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routeContext = c)
                .ReturnsAsync(routerResult);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            var response = await _terminalProcessor.ProcessRequestAsync("command1|", "sender_1", "sender_endpoint_1");

            // Make sure context is correctly populated
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Make sure response is correct
            response.Should().NotBeNull();
            response.SenderId.Should().Be("sender_1");
            response.SenderEndpoint.Should().Be("sender_endpoint_1");

            response.Commands.Should().HaveCount(1);
            response.Commands[0].Id.Should().NotBeNullOrWhiteSpace();
            response.Commands[0].Raw.Should().Be("command1");

            response.Results.Should().HaveCount(1);
            response.Results[0].Should().Be("sender_result");
        }

        [Fact]
        public async Task ProcessAsync_Without_Processing_Throws()
        {
            // Act
            Func<Task> act = async () => await _terminalProcessor.ProcessRequestAsync("command1", "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal processor is not running.");
        }

        [Fact]
        public async Task StartProcessing_HandlesRouterException()
        {
            Exception? handeledException = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));
            _mockExceptionHandler.Setup(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>())).Callback<TerminalExceptionHandlerContext>(c => handeledException = c.Exception);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            await _terminalProcessor.AddAsync("command1|", "sender", "endpoint");
            await Task.Delay(500);

            handeledException.Should().NotBeNull();
            handeledException!.Message.Should().Be("Router error");
        }

        [Fact]
        public async Task StartProcessing_With_Add_Routes_Command_To_Router()
        {
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).Callback<CommandRouterContext>(c => routeContext = c);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Add with sender endpoint and sender id
            await _terminalProcessor.AddAsync("command1|", "sender_1", "sender_endpoint_1");
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");

            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command1");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Add without sender endpoint and sender id
            routeContext = null;
            await _terminalProcessor.AddAsync("command2|", null, null);
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(1);
            routeContext.Properties!["sender_endpoint"].Should().Be("$unknown$");

            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command2");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);
        }

        [Fact]
        public void StartProcessing_With_Background_Sets_Fields()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalProcessor.IsBackground.Should().BeTrue();
            _terminalTokenSource.Cancel();
        }

        [Fact]
        public void StartProcessing_Without_Background_Sets_Fields()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: false);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalProcessor.IsBackground.Should().BeFalse();
            _terminalTokenSource.Cancel();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(Timeout.Infinite)]
        public async Task StopProcessingAsync_Any_Timeout_And_Completed_Processing_Sets_IsProcessing_ToFalse(int timeout)
        {
            var context = new Mock<TerminalRouterContext>(terminalStartContext).Object;

            _terminalProcessor.StartProcessing(context, background: true);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalTokenSource.Cancel();
            await Task.Delay(500);
            var timedOut = await _terminalProcessor.StopProcessingAsync(timeout);
            timedOut.Should().BeFalse();
            _terminalProcessor.IsProcessing.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Sets_IsProcessing_ToFalse()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            _terminalTokenSource.CancelAfter(300);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(Timeout.Infinite);
            _terminalProcessor.IsProcessing.Should().BeFalse();
            timeout.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Throws_If_Not_Processing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            Func<Task> act = async () => await _terminalProcessor.StopProcessingAsync(100);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running.");
        }

        [Fact]
        public async Task StopProcessingAsync_TimesOut_Return_True_Sets_IsProcessing_ToTrue()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            _terminalTokenSource.CancelAfter(400);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(100);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            timeout.Should().BeTrue();

            // give time for the processor to stop
            await Task.Delay(500);
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Incomplete_Batch_correctly()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            string? processedCommand = null;
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(ctx =>
                {
                    processedCommand = ctx.Request.Raw;
                });

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            byte[] bytes = _textHandler.Encoding.GetBytes("command1");
            await _terminalProcessor.StreamAsync(bytes, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify command was not processed since the batch is incomplete
            processedCommand.Should().BeNull();

            // Ensure there are not unprocessed requests since the batch is incomplete
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Large_Data_Received_In_Chunks_Maintaining_Order()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            var processedCommands = new ConcurrentQueue<string>();
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(ctx =>
                {
                    processedCommands.Enqueue(ctx.Request.Raw);
                });

            // Create a large batch of commands to simulate streaming
            var commands = Enumerable.Range(1, 1500).Select(i => $"command{i}").ToArray();
            var completeBatch = string.Join("|", commands) + "|"; // Ensure the batch ends with the delimiter
            var completeBatchBytes = Encoding.UTF8.GetBytes(completeBatch);

            // Split the data into sizable chunks to simulate streaming
            int chunkSize = 30;
            var chunks = completeBatchBytes.Chunk(chunkSize).ToArray();

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            foreach (var chunk in chunks)
            {
                await _terminalProcessor.StreamAsync(chunk, senderId, senderEndpoint);
            }

            // Allow time for processing to complete
            await Task.Delay(3000);

            // Assert: Verify all commands were processed in the correct order
            processedCommands.ToArray().Should().BeEquivalentTo(commands);

            // Ensure no unprocessed requests are left
            _terminalProcessor.UnprocessedRequests.Should().BeEmpty();
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Partial_Batch_correctly()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            List<string> processedCommands = [];
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(ctx =>
                {
                    processedCommands.Add(ctx.Request.Raw);
                });

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            byte[] bytes = _textHandler.Encoding.GetBytes("command1|command2|command3");
            await _terminalProcessor.StreamAsync(bytes, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify command was are processed since the batch is partial
            processedCommands.Should().HaveCount(2);
            processedCommands.Should().BeEquivalentTo(["command1", "command2"]);

            // Ensure there are not unprocessed requests since the batch is partial
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Partial_Batch_With_Command_Delimiter_Correctly()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            List<string> processedCommands = [];
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(ctx =>
                {
                    processedCommands.Add(ctx.Request.Raw);
                })
                .ReturnsAsync(new CommandRouterResult(new Commands.Handlers.CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), new TerminalRequest("mock_id", "mock_command")));

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            byte[] bytes = _textHandler.Encoding.GetBytes("command1,command2,command3|command4,command5,command6|command7");
            await _terminalProcessor.StreamAsync(bytes, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify command was are processed since the batch is partial
            processedCommands.Should().HaveCount(6);
            processedCommands.Should().BeEquivalentTo(["command1", "command2", "command3", "command4", "command5", "command6"]);

            // Ensure there are not unprocessed requests since the batch is partial
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Single_Batch_correctly()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            var processedCommand = "";
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(ctx =>
                {
                    processedCommand = ctx.Request.Raw;
                });

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            byte[] bytes = _textHandler.Encoding.GetBytes("command1|");
            await _terminalProcessor.StreamAsync(bytes, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify all commands were processed in the correct order
            processedCommand.Should().Be("command1");

            // Ensure no unprocessed requests are left
            _terminalProcessor.UnprocessedRequests.Should().BeEmpty();
        }

        [Fact]
        public async Task WaitAsync_CancelsIndefiniteProcessing()
        {
            // Start the WaitAsync task
            Task waiting = _terminalProcessor.WaitUntilCanceledAsync(_terminalTokenSource.Token);

            // Check periodically to ensure the task is not yet completed
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                waiting.Status.Should().NotBe(TaskStatus.RanToCompletion);
            }

            // Trigger cancellation and verify the task completes
            _terminalTokenSource.Cancel();
            await Task.Delay(200);
            waiting.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        private readonly Mock<ICommandRouter> _mockCommandRouter;
        private readonly Mock<ITerminalExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<TerminalProcessor>> _mockLogger;
        private readonly Mock<IOptions<TerminalOptions>> _mockOptions;
        private readonly Mock<TerminalRouterContext> _mockTerminalRouterContext;
        private readonly TerminalProcessor _terminalProcessor;
        private readonly CancellationTokenSource _terminalTokenSource;
        private readonly ITerminalTextHandler _textHandler;
        private readonly TerminalStartContext terminalStartContext;
    }
}
