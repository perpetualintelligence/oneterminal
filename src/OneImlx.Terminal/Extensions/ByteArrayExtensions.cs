//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="byte"/> arrays.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Splits a byte array into segments based on a delimiter.
        /// </summary>
        /// <param name="buffer">The source buffer array to split.</param>
        /// <param name="delimiter">The delimiter byte.</param>
        /// <param name="ignoreEmpty">Indicates whether to ignore empty segments.</param>
        /// <param name="endsWithDelimiter">Indicates whether the source array ends with the delimiter.</param>
        /// <returns>An array of byte arrays representing the segments.</returns>
        public static byte[][] Split(this byte[] buffer, byte delimiter, bool ignoreEmpty, out bool endsWithDelimiter)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("Source array cannot be null or empty.", nameof(buffer));
            }

            // Check if buffer ends with delimiter
            endsWithDelimiter = buffer[buffer.Length - 1] == delimiter;

            // Special case: buffer is only the delimiter
            if (buffer.Length == 1 && buffer[0] == delimiter)
            {
                return ignoreEmpty ? [] : [[]];
            }

            // Count delimiters in a single pass
            int delimiterCount = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == delimiter)
                {
                    delimiterCount++;
                }
            }

            // Fast path: no delimiters - return original buffer (zero-copy)
            if (delimiterCount == 0)
            {
                return [buffer];
            }

            // Pre-allocate result array for maximum possible segments
            byte[][] segments = new byte[delimiterCount + 1][];
            int segmentCount = 0;
            int segmentStart = 0;

            // Extract segments
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == delimiter)
                {
                    int length = i - segmentStart;

                    if (!ignoreEmpty || length > 0)
                    {
                        byte[] segment = new byte[length];
                        if (length > 0)
                        {
                            Buffer.BlockCopy(buffer, segmentStart, segment, 0, length);
                        }
                        segments[segmentCount++] = segment;
                    }

                    segmentStart = i + 1;
                }
            }

            // Handle final segment after last delimiter
            int finalLength = buffer.Length - segmentStart;
            if (!ignoreEmpty || finalLength > 0)
            {
                byte[] finalSegment = new byte[finalLength];
                if (finalLength > 0)
                {
                    Buffer.BlockCopy(buffer, segmentStart, finalSegment, 0, finalLength);
                }
                segments[segmentCount++] = finalSegment;
            }

            // Trim array if we skipped empty segments
            if (segmentCount < segments.Length)
            {
                byte[][] result = new byte[segmentCount][];
                Array.Copy(segments, result, segmentCount);
                return result;
            }

            return segments;
        }
    }
}