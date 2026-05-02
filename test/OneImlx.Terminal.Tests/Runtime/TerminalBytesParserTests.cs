//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using FluentAssertions;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalBytesParserTests
    {
        private readonly ITestOutputHelper output;
        private readonly TerminalBytesParser parser;

        public TerminalBytesParserTests(ITestOutputHelper output)
        {
            this.output = output;
            this.parser = new TerminalBytesParser();
        }

        [Fact]
        public void Split_HandlesConsecutiveDelimiters()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 2, 3];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEmpty();
            result[2].Should().BeEquivalentTo(new byte[] { 2, 3 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesConsecutiveDelimiters_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 2, 3];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEquivalentTo(new byte[] { 2, 3 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesDelimiterAtStartAndEnd()
        {
            // Arrange
            byte[] source = [0x1F, 1, 2, 3, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEmpty();
            result[1].Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
            result[2].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_HandlesDelimiterAtStartAndEnd_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [0x1F, 1, 2, 3, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsEmptyLastSegment_WhenSourceEndsWithDelimiter()
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
            result[1].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsEmptyLastSegment_WhenSourceEndsWithDelimiter_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5 });
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsNoSegments_WhenOnlyDelimiterExists_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsSingleEmptySegment_WhenOnlyDelimiterExists()
        {
            // Arrange
            byte[] source = [0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Split_ReturnsSingleSegment_WhenDelimiterIsAbsent(bool ignoreEmpty)
        {
            // Arrange
            byte[] source = [1, 2, 3, 4, 5];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, ignoreEmpty, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(source);
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesMultipleConsecutiveDelimiters()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 0x1F, 2];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(4);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEmpty();
            result[2].Should().BeEmpty();
            result[3].Should().BeEquivalentTo(new byte[] { 2 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesMultipleConsecutiveDelimiters_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [1, 0x1F, 0x1F, 0x1F, 2];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEquivalentTo(new byte[] { 2 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesAllDelimiters()
        {
            // Arrange
            byte[] source = [0x1F, 0x1F, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(4);
            result[0].Should().BeEmpty();
            result[1].Should().BeEmpty();
            result[2].Should().BeEmpty();
            result[3].Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_HandlesAllDelimiters_IgnoreEmpty()
        {
            // Arrange
            byte[] source = [0x1F, 0x1F, 0x1F];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().BeEmpty();
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_HandlesLargeSegments()
        {
            // Arrange
            // Use delimiter 0xFF and ensure test data uses values 0x00-0xFE to avoid accidental splits
            byte[] segment1 = Enumerable.Range(0, 1000).Select(x => (byte)(x % 255)).ToArray();
            byte[] segment2 = Enumerable.Range(0, 2000).Select(x => (byte)(x % 255)).ToArray();
            byte[] source = segment1.Concat(new byte[] { 0xFF }).Concat(segment2).ToArray();
            byte delimiter = 0xFF;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(segment1);
            result[1].Should().BeEquivalentTo(segment2);
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesManySmallSegments()
        {
            // Arrange
            // Create 100 segments of 10 bytes each
            var segments = Enumerable.Range(0, 100)
                .Select(i => Enumerable.Range(0, 10).Select(j => (byte)(i + j)).ToArray())
                .ToArray();

            byte[] source = segments
                .SelectMany(s => s.Concat(new byte[] { 0xFF }))
                .ToArray();
            byte delimiter = 0xFF;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(101); // 100 segments + 1 empty at end
            result[^1].Should().BeEmpty();
            for (int i = 0; i < 100; i++)
            {
                result[i].Should().BeEquivalentTo(segments[i]);
            }
            endsWithDelimiter.Should().BeTrue();
        }

        [Fact]
        public void Split_HandlesManySmallSegments_IgnoreEmpty()
        {
            // Arrange
            var segments = Enumerable.Range(0, 100)
                .Select(i => Enumerable.Range(0, 10).Select(j => (byte)(i + j)).ToArray())
                .ToArray();

            byte[] source = segments
                .SelectMany(s => s.Concat(new byte[] { 0xFF }))
                .ToArray();
            byte delimiter = 0xFF;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(100);
            for (int i = 0; i < 100; i++)
            {
                result[i].Should().BeEquivalentTo(segments[i]);
            }
            endsWithDelimiter.Should().BeTrue();
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x1E)]
        [InlineData(0x1F)]
        [InlineData(0xFF)]
        public void Split_HandlesVariousDelimiterValues(byte delimiter)
        {
            // Arrange
            byte[] source = [1, delimiter, 2, 3, delimiter, 4, 5];

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(new byte[] { 1 });
            result[1].Should().BeEquivalentTo(new byte[] { 2, 3 });
            result[2].Should().BeEquivalentTo(new byte[] { 4, 5 });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesBinaryDataWithZeroBytes()
        {
            // Arrange
            byte[] source = [0x00, 0x01, 0x1F, 0x00, 0x00, 0x1F, 0xFF];
            byte delimiter = 0x1F;

            // Act
            var result = parser.Split(source, delimiter, false, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(new byte[] { 0x00, 0x01 });
            result[1].Should().BeEquivalentTo(new byte[] { 0x00, 0x00 });
            result[2].Should().BeEquivalentTo(new byte[] { 0xFF });
            endsWithDelimiter.Should().BeFalse();
        }

        [Fact]
        public void Split_HandlesStreamingScenario_PartialMessage()
        {
            // Arrange - Simulates TCP streaming where last message is incomplete
            byte[] jsonMessage1 = System.Text.Encoding.UTF8.GetBytes("{\"id\":1}");
            byte[] jsonMessage2 = System.Text.Encoding.UTF8.GetBytes("{\"id\":2}");
            byte[] jsonMessage3Partial = System.Text.Encoding.UTF8.GetBytes("{\"id\":3,\"da"); // Incomplete

            byte[] source = jsonMessage1
                .Concat(new byte[] { 0x1E })
                .Concat(jsonMessage2)
                .Concat(new byte[] { 0x1E })
                .Concat(jsonMessage3Partial)
                .ToArray();
            byte delimiter = 0x1E;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(3);
            result[0].Should().BeEquivalentTo(jsonMessage1);
            result[1].Should().BeEquivalentTo(jsonMessage2);
            result[2].Should().BeEquivalentTo(jsonMessage3Partial); // Partial message should be returned
            endsWithDelimiter.Should().BeFalse(); // Critical: indicates incomplete message
        }

        [Fact]
        public void Split_HandlesStreamingScenario_CompleteMessages()
        {
            // Arrange - All messages complete (end with delimiter)
            byte[] jsonMessage1 = System.Text.Encoding.UTF8.GetBytes("{\"id\":1}");
            byte[] jsonMessage2 = System.Text.Encoding.UTF8.GetBytes("{\"id\":2}");

            byte[] source = jsonMessage1
                .Concat(new byte[] { 0x1E })
                .Concat(jsonMessage2)
                .Concat(new byte[] { 0x1E })
                .ToArray();
            byte delimiter = 0x1E;

            // Act
            var result = parser.Split(source, delimiter, true, out var endsWithDelimiter);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().BeEquivalentTo(jsonMessage1);
            result[1].Should().BeEquivalentTo(jsonMessage2);
            endsWithDelimiter.Should().BeTrue(); // Critical: all messages complete
        }

        [Fact]
        public void Split_ThrowsArgumentException_WhenSourceIsEmpty()
        {
            // Arrange
            byte[] source = [];
            byte delimiter = 0x1F;

            // Act
            Action act = () => parser.Split(source, delimiter, false, out _);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Source array cannot be null or empty.*");
        }

        [Fact]
        public void Split_ThrowsArgumentException_WhenSourceIsNull()
        {
            // Arrange
            byte[] source = null!;
            byte delimiter = 0x1F;

            // Act
            Action act = () => parser.Split(source, delimiter, false, out _);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Source array cannot be null or empty.*");
        }

        [Fact]
        public void Split_Performance_SmallBuffer_10KB()
        {
            // Arrange
            byte[] buffer = CreateTestBuffer(10240, 100);
            byte delimiter = 0x1E;
            const int iterations = 10000;

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }

            // Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw.Stop();

            // Assert
            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            output.WriteLine($"Small buffer (10KB, 100 delimiters): {avgMs:F6} ms per operation");
            output.WriteLine($"Total time for {iterations} iterations: {sw.Elapsed.TotalMilliseconds:F2} ms");

            avgMs.Should().BeLessThan(0.1, "Split should process 10KB in less than 0.1ms");
        }

        [Fact]
        public void Split_Performance_MediumBuffer_100KB()
        {
            // Arrange
            byte[] buffer = CreateTestBuffer(102400, 1000);
            byte delimiter = 0x1E;
            const int iterations = 1000;

            // Warmup
            for (int i = 0; i < 10; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }

            // Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw.Stop();

            // Assert
            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            output.WriteLine($"Medium buffer (100KB, 1000 delimiters): {avgMs:F6} ms per operation");
            output.WriteLine($"Total time for {iterations} iterations: {sw.Elapsed.TotalMilliseconds:F2} ms");

            avgMs.Should().BeLessThan(1.0, "Split should process 100KB in less than 1ms");
        }

        [Fact]
        public void Split_Performance_LargeBuffer_1MB()
        {
            // Arrange
            byte[] buffer = CreateTestBuffer(1048576, 10000);
            byte delimiter = 0x1E;
            const int iterations = 100;

            // Warmup
            for (int i = 0; i < 5; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }

            // Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw.Stop();

            // Assert
            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            output.WriteLine($"Large buffer (1MB, 10000 delimiters): {avgMs:F6} ms per operation");
            output.WriteLine($"Total time for {iterations} iterations: {sw.Elapsed.TotalMilliseconds:F2} ms");

            avgMs.Should().BeLessThan(15.0, "Split should process 1MB in less than 15ms");
        }

        [Fact]
        public void Split_Performance_ManySmallSegments()
        {
            // Arrange
            byte[] buffer = CreateTestBuffer(10240, 5000);
            byte delimiter = 0x1E;
            const int iterations = 1000;

            // Warmup
            for (int i = 0; i < 10; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }

            // Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw.Stop();

            // Assert
            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            output.WriteLine($"Many small segments (10KB, 5000 delimiters): {avgMs:F6} ms per operation");
            output.WriteLine($"Total time for {iterations} iterations: {sw.Elapsed.TotalMilliseconds:F2} ms");

            avgMs.Should().BeLessThan(1.0, "Split should handle many small segments efficiently");
        }

        [Fact]
        public void Split_Performance_NoDelimiters()
        {
            // Arrange
            byte[] buffer = new byte[102400];
            Array.Fill(buffer, (byte)0x42);
            byte delimiter = 0x1E;
            const int iterations = 10000;

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }

            // Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw.Stop();

            // Assert
            double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
            output.WriteLine($"No delimiters (100KB, single segment): {avgMs:F6} ms per operation");
            output.WriteLine($"Total time for {iterations} iterations: {sw.Elapsed.TotalMilliseconds:F2} ms");

            avgMs.Should().BeLessThan(0.5, "Split should be extremely fast with no delimiters");
        }

        [Fact]
        public void Split_Performance_IgnoreEmptyVsKeepEmpty()
        {
            // Arrange
            byte[] buffer = CreateTestBuffer(10240, 1000);
            byte delimiter = 0x1E;
            const int iterations = 5000;

            // Warmup
            for (int i = 0; i < 50; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
                _ = parser.Split(buffer, delimiter, false, out _);
            }

            // Act
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, true, out _);
            }
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _ = parser.Split(buffer, delimiter, false, out _);
            }
            sw2.Stop();

            // Assert
            double avgIgnoreEmpty = sw1.Elapsed.TotalMilliseconds / iterations;
            double avgKeepEmpty = sw2.Elapsed.TotalMilliseconds / iterations;

            output.WriteLine($"IgnoreEmpty=true:  {avgIgnoreEmpty:F6} ms per operation");
            output.WriteLine($"IgnoreEmpty=false: {avgKeepEmpty:F6} ms per operation");
            output.WriteLine($"Difference: {Math.Abs(avgIgnoreEmpty - avgKeepEmpty):F6} ms");

            avgIgnoreEmpty.Should().BeLessThan(0.5);
            avgKeepEmpty.Should().BeLessThan(0.5);
        }

        private byte[] CreateTestBuffer(int totalSize, int delimiterCount)
        {
            byte[] buffer = new byte[totalSize];

            if (delimiterCount == 0)
            {
                var random = new Random(42);
                random.NextBytes(buffer);
                return buffer;
            }

            int segmentSize = totalSize / (delimiterCount + 1);
            var random2 = new Random(42);

            int position = 0;
            for (int i = 0; i < delimiterCount; i++)
            {
                for (int j = 0; j < segmentSize && position < totalSize - 1; j++)
                {
                    buffer[position++] = (byte)random2.Next(0, 256);
                }

                if (position < totalSize)
                {
                    buffer[position++] = 0x1E;
                }
            }

            while (position < totalSize)
            {
                buffer[position++] = (byte)random2.Next(0, 256);
            }

            return buffer;
        }
    }
}