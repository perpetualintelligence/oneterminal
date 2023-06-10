﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Services;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Services
{
    /// <summary>
    /// The <see cref="Console"/> helper methods.
    /// </summary>
    [WriteUnitTest]
    public static class TerminalHelper
    {
        /// <summary>
        /// Extracts the options from the raw option string.
        /// </summary>
        /// <param name="raw">The raw option string.</param>
        /// <param name="terminalOptions">The terminal options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <returns></returns>
        public static OptionStrings ExtractOptionStrings(string raw, TerminalOptions terminalOptions, ITextHandler textHandler)
        {
            // Our search pattern is {separator}{prefix}, so we make sure we have just 1 separator at the beginning.
            string trimmedRaw = raw.TrimStart(terminalOptions.Extractor.Separator, textHandler.Comparison);
            trimmedRaw = string.Concat(terminalOptions.Extractor.Separator, trimmedRaw);

            // Define the separator split for both id and alias
            string optSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionPrefix);
            string optAliasSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionAliasPrefix);

            // Populate the range
            SortedDictionary<int, int> withInRanges = new();
            if (terminalOptions.Extractor.OptionValueWithIn != null)
            {
                int withInIdx = 0;
                while (true)
                {
                    int startIdx = trimmedRaw.IndexOf(terminalOptions.Extractor.OptionValueWithIn, withInIdx, textHandler.Comparison);
                    if (startIdx < 0)
                    {
                        break;
                    }

                    int endIdx = trimmedRaw.IndexOf(terminalOptions.Extractor.OptionValueWithIn, startIdx + 1, textHandler.Comparison);
                    if (endIdx < 0)
                    {
                        throw new ErrorException(Errors.InvalidRequest, "The option value within end token is not specified.");
                    }

                    withInRanges.Add(startIdx, endIdx);

                    // Look for next with in range
                    withInIdx = endIdx + 1;
                }
            }

            int counter = 1;
            int currentOptionPos = 0;
            int currentIterationPos = 0;
            OptionStrings locations = new();
            while (true)
            {
                if (counter > 50)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, $"Too many iteration while extracting options. max={50} current={counter}");
                }

                // Increment the nextPos to get the next option spit
                int nextPos = currentIterationPos + 1;

                // Iterate to next split positions.
                int nextOptIdPos = trimmedRaw.IndexOf(optSplit, nextPos, textHandler.Comparison);
                int nextOptAliasPos = trimmedRaw.IndexOf(optAliasSplit, nextPos, textHandler.Comparison);

                // No more matches so break. When the currentPos reaches the end then we have traversed the entire argString.
                int splitPos = 0;
                bool reachedEnd = false;
                if (nextOptIdPos == -1 && nextOptAliasPos == -1)
                {
                    // If we reach at the end then take the entire remaining string.
                    splitPos = trimmedRaw.Length;
                    reachedEnd = true;
                }
                else
                {
                    splitPos = InfraHelper.MinPositiveOrZero(nextOptIdPos, nextOptAliasPos);
                }

                // The currentIterationPos tracks each iteration so that we keep moving forward.
                currentIterationPos = splitPos;

                // Determine if split position is in between the with-in ranges
                if (!withInRanges.Any(e => splitPos > e.Key && splitPos < e.Value))
                {
                    // Get the arg substring and record its position and alias
                    // NOTE: This is the current pos and current alias not the next.
                    string kvp = trimmedRaw.Substring(currentOptionPos, splitPos - currentOptionPos);
                    bool isAlias = !kvp.StartsWith(optSplit, textHandler.Comparison);
                    locations.Add(new OptionString(kvp, isAlias, currentOptionPos));

                    // Update the currentPos only if we have processed the entire within range
                    currentOptionPos = currentIterationPos;
                }

                if (reachedEnd)
                {
                    break;
                }

                counter += 1;
            }

            return locations;
        }

        /// <summary>
        /// Determines if we are in a <c>dev-mode</c>. We assume <c>dev-mode</c> during debugging if the consumer deploys <c>pi-cli</c> on-premise,
        /// use any source code editor or an IDE such as Visual Studio, Visual Studio Code, NotePad, Eclipse etc., use DEBUG or any other custom configuration.
        /// It is a violation of licensing terms to disable <c>dev-mode</c> with IL Weaving, Reflection or any other methods.
        /// </summary>
        /// <returns></returns>
        public static bool IsDevMode()
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

#if RELEASE
            return false;
#else
            return true;
#endif
        }

        /// <summary>
        /// Clears the written output for the specified position.
        /// </summary>
        /// <param name="clearPosition">The console clear position.</param>
        public static void ClearOutput(ConsoleClearPosition clearPosition)
        {
            if (clearPosition.Left < 5)
            {
                throw new ArgumentException("The cursor left position cannot be less than 5.");
            }

            if (clearPosition.Top < 0)
            {
                throw new ArgumentException("The cursor top position cannot be negative.");
            }

            if (clearPosition.Length <= 0)
            {
                return;
            }

            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console/8946847
            Console.SetCursorPosition(clearPosition.Left, clearPosition.Top);
            Console.Write(new string(' ', clearPosition.Length));
            Console.SetCursorPosition(clearPosition.Left, clearPosition.Top);
        }

        /// <summary>
        /// Gets the <see cref="ConsoleClearPosition"/> for the specified output length.
        /// </summary>
        /// <param name="length">The output length to clear.</param>
        /// <returns>
        /// <see cref="ConsoleClearPosition"/> object with <see cref="Console.CursorLeft"/>,
        /// <see cref="Console.CursorTop"/> and specified length.
        /// </returns>
        public static ConsoleClearPosition GetClearPosition(int length)
        {
            return new ConsoleClearPosition() { Left = Console.CursorLeft, Top = Console.CursorTop, Length = length };
        }

        /// <summary>
        /// Writes the specified string with specified color to the standard output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteColor(ConsoleColor, string, object[])"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteColor(ConsoleColor color, string value, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(value, args);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes the specified string with specified color followed by the current line terminator to the standard
        /// output stream.
        /// </summary>
        /// <remarks>
        /// <see cref="WriteLineColor(ConsoleColor, string, object[])"/> resets the color after writing the string using
        /// <see cref="Console.ResetColor"/> method.
        /// </remarks>
        public static void WriteLineColor(ConsoleColor color, string value, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value, args);
            Console.ResetColor();
        }
    }
}