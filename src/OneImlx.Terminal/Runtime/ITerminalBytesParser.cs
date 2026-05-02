//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction for parsing byte sequences into structured data or objects.
    /// </summary>
    public interface ITerminalBytesParser
    {
        /// <summary>
        /// Splits the specified byte array into segments separated by the given delimiter.
        /// </summary>
        /// <param name="buffer">The byte array to split. Cannot be null.</param>
        /// <param name="delimiter">The byte value that delimits each segment in the buffer.</param>
        /// <param name="ignoreEmpty">true to omit empty segments from the result; otherwise, false.</param>
        /// <param name="endsWithDelimiter">When this method returns, contains true if the buffer ends with the delimiter; otherwise, false.</param>
        /// <returns>
        /// An array of byte arrays, each representing a segment of the original buffer separated by the delimiter. The
        /// array is empty if the buffer is empty or contains only delimiters and ignoreEmpty is true.
        /// </returns>
        public byte[][] Split(byte[] buffer, byte delimiter, bool ignoreEmpty, out bool endsWithDelimiter);
    }
}