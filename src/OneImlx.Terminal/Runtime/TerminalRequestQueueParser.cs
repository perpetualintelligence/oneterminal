//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRequestParser"/> that uses a queue to parse the terminal request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The parser implements a three-phase queue-based algorithm to convert raw terminal input into structured tokens and options:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Phase 1: Segmentation</term>
    /// <description>
    /// Segments the raw input string by replacing separators (space, option-value separator) with a runtime separator character,
    /// while respecting value delimiters (e.g., quotes). This ensures that values within delimiters are treated as single segments.
    /// The result is a queue of string segments ready for classification.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Phase 2: Token Extraction</term>
    /// <description>
    /// Processes segments from the front of the queue until an option prefix is encountered. Each segment is validated for
    /// delimiter balance, cleaned of delimiters if present, and added to the tokens collection. Tokens represent command
    /// arguments that precede all options.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Phase 3: Option Extraction</term>
    /// <description>
    /// Processes remaining segments as options with optional values. Distinguishes between alias options (single dash) and
    /// full options (double dash). Determines whether an option is unary (boolean flag) or requires a value by peeking at
    /// the next segment. Validates delimiter balance for option values and stores options with their values and alias flags.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The algorithm uses local option variables passed through method parameters to enable efficient parsing.
    /// Collections are pre-sized to minimize memory allocations. All parsing is performed synchronously with the result
    /// wrapped in a completed task.
    /// </para>
    /// </remarks>
    public class TerminalRequestQueueParser : ITerminalRequestParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalRequestQueueParser"/> class.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalRequestQueueParser(
            ITerminalTextHandler textHandler,
            IOptions<TerminalOptions> terminalOptions,
            ILogger<TerminalRequestQueueParser> logger)
        {
            this.textHandler = textHandler;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Parses the terminal request asynchronously into tokens and options.
        /// </summary>
        /// <param name="request">The terminal request containing the raw command string to parse.</param>
        /// <returns>
        /// A task that represents the asynchronous parse operation. The task result contains the parsed request
        /// with extracted tokens (command arguments) and options (key-value pairs with alias information).
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method executes synchronously and returns a completed task for API compatibility. The parsing
        /// involves three sequential phases: segmentation of the raw input, extraction of positional tokens,
        /// and extraction of named options.
        /// </para>
        /// <para>
        /// The terminal options are retrieved once at the start and passed to all extraction methods to avoid
        /// repeated property access overhead. Collections are pre-sized based on the segment queue count for
        /// optimal memory usage.
        /// </para>
        /// </remarks>
        public Task<TerminalParsedRequest> ParseRequestAsync(CommandRequest request)
        {
            TerminalOptions options = terminalOptions.Value;

            // Parse the queue of segments from the raw command based on `separator` and `optionValueSeparator`
            Queue<string> segmentsQueue = ExtractQueue(request, options);

            // Extract tokens and options from the segments queue
            IEnumerable<string> tokens = ExtractTokens(segmentsQueue, options);
            Dictionary<string, ValueTuple<string, bool>> parsedOptions = ExtractOptions(segmentsQueue, options);

            return Task.FromResult(new TerminalParsedRequest(tokens, parsedOptions));
        }

        /// <summary>
        /// Extracts options and their values from the remaining segments in the queue.
        /// </summary>
        /// <param name="segmentsQueue">
        /// The queue of string segments to process. This queue is modified as segments are consumed.
        /// After token extraction, this queue contains only option-related segments.
        /// </param>
        /// <param name="options">The terminal configuration options used to control parsing behavior.</param>
        /// <returns>
        /// A dictionary where keys are option names (without prefix characters), and values are tuples containing
        /// the option value (as string) and a boolean indicating whether the option was specified using alias syntax.
        /// For unary boolean options without explicit values, the value will be "True".
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method processes segments as option-value pairs with the following logic:
        /// </para>
        /// <list type="bullet">
        /// <item>Dequeues each segment and identifies it as an option based on the configured prefix character(s).</item>
        /// <item>Strips the option prefix (single dash for alias, double dash for full name) unless alias support is disabled.</item>
        /// <item>Peeks at the next segment to determine if it's a value or another option.</item>
        /// <item>Validates delimiter balance for option values (opening and closing delimiters must match).</item>
        /// <item>Treats options without values as unary boolean flags with a value of "True".</item>
        /// </list>
        /// <para>
        /// When alias support is disabled via <see cref="ParserOptions.DisableOptionAlias"/>, all options
        /// are treated as non-aliased regardless of the number of prefix characters used.
        /// </para>
        /// <para>
        /// The dictionary is pre-sized with the queue count to minimize internal array resizing operations during
        /// option addition.
        /// </para>
        /// </remarks>
        /// <exception cref="TerminalException">
        /// Thrown when an option value starts with a delimiter but does not end with the matching delimiter.
        /// </exception>
        private Dictionary<string, ValueTuple<string, bool>> ExtractOptions(Queue<string> segmentsQueue, TerminalOptions options)
        {
            // Pre-size dictionary to avoid internal array resizing operations
            Dictionary<string, ValueTuple<string, bool>> parsedOptions = new(segmentsQueue.Count);

            while (segmentsQueue.Count > 0)
            {
                // Always dequeue a segment because we're expecting it to be an option.
                string option = segmentsQueue.Dequeue();
                _ = TerminalServices.IsOption(option, options.Parser.OptionPrefix, out bool isAlias);

                // If the option alias is disabled then we do not support short, long and hybrid options.
                // - With alias: -a, --all, --long-option,
                // - Without alias: -a, -all, -longoption
                if (options.Parser.DisableOptionAlias)
                {
                    // Override the alias flag if the option alias is disabled.
                    isAlias = false;
                    option = option.Substring(1);
                }
                else
                {
                    // Remove the first character if it is an alias prefix otherwise remove 2 characters if it is an
                    // option prefix.
                    option = isAlias ? option.Substring(1) : option.Substring(2);
                }

                // Check whether we have an option value and if the option value is an option itself then the previous
                // option is a unary boolean option.
                if (segmentsQueue.Count > 0)
                {
                    string optionValue = segmentsQueue.Peek();
                    if (!TerminalServices.IsOption(optionValue, options.Parser.OptionPrefix, out _))
                    {
                        // Ensure token ends with a delimiter (use indexer instead of LINQ for performance)
                        if (optionValue.Length > 0 && optionValue[0] == options.Parser.ValueDelimiter)
                        {
                            if (optionValue[optionValue.Length - 1] != options.Parser.ValueDelimiter)
                            {
                                throw new TerminalException(TerminalErrors.InvalidOption, "The option value is missing the closing delimiter. option={0}", option);
                            }

                            // Remove the delimiter and return the raw value.
                            optionValue = optionValue.Substring(1, optionValue.Length - 2);
                        }

                        // The option value is processed to remove it from the queue, so we can process the next option.
                        segmentsQueue.Dequeue();
                        parsedOptions.Add(option, new ValueTuple<string, bool>(optionValue, isAlias));
                        continue;
                    }
                }

                // If we are here that means the option is a unary boolean option.
                parsedOptions.Add(option, new ValueTuple<string, bool>(true.ToString(), isAlias));
            }

            return parsedOptions;
        }

        /// <summary>
        /// Extracts and processes the raw terminal request into a queue of string segments.
        /// </summary>
        /// <param name="request">The terminal request containing the raw command string to segment.</param>
        /// <param name="options">The terminal configuration options that define separators and delimiters.</param>
        /// <returns>
        /// A queue of string segments where each segment represents either a token, option, or option value.
        /// Segments are ordered as they appeared in the original raw input.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method implements the segmentation phase of the parsing algorithm with the following steps:
        /// </para>
        /// <list type="number">
        /// <item>
        /// Copies the raw input string into a <see cref="StringBuilder"/> with exact capacity to avoid resizing operations.
        /// </item>
        /// <item>
        /// Iterates through each character, tracking whether the current position is within a value delimiter
        /// (e.g., inside quotes). This ensures that separators within delimited values are not treated as segment boundaries.
        /// </item>
        /// <item>
        /// Replaces separator characters (space and option-value separator) with the runtime separator character,
        /// but only when outside of delimited regions.
        /// </item>
        /// <item>
        /// Splits the processed string on the runtime separator character, filtering out empty segments.
        /// </item>
        /// <item>
        /// Constructs a queue directly from the array of segments for efficient sequential access during subsequent
        /// token and option extraction phases.
        /// </item>
        /// </list>
        /// <para>
        /// The value delimiter character specified in <see cref="ParserOptions.ValueDelimiter"/> allows
        /// values containing separator characters to be treated as atomic segments. For example, with quotes as
        /// delimiters: <c>command "value with spaces" --option</c> produces three segments.
        /// </para>
        /// <para>
        /// This approach preserves the integrity of delimited values while enabling simple split-based tokenization
        /// after delimiter-aware separator replacement.
        /// </para>
        /// </remarks>
        private Queue<string> ExtractQueue(CommandRequest request, TerminalOptions options)
        {
            string raw = request.Raw;
            char valueDelimiter = options.Parser.ValueDelimiter;
            char separator = options.Parser.Separator;
            char valueSeparator = options.Parser.OptionValueSeparator;
            char runtimeSeparator = options.Parser.RuntimeSeparator;

            // Use StringBuilder with exact capacity to avoid resizing
            StringBuilder rawBuilder = new(raw.Length);
            rawBuilder.Append(raw);

            bool withinDelimiter = false;
            for (int idx = 0; idx < rawBuilder.Length; ++idx)
            {
                char currentChar = rawBuilder[idx];

                // If we are within a value delimiter then no parsing logic is applied. The value delimiter are for
                // arguments and options values. So a value delimiter will always have a preceding separator.
                if (currentChar == valueDelimiter)
                {
                    withinDelimiter = !withinDelimiter;
                    continue;
                }

                // Replace the separator with the runtime separator if it is not within a delimiter.
                if ((currentChar == separator || currentChar == valueSeparator) && !withinDelimiter)
                {
                    rawBuilder[idx] = runtimeSeparator;
                }
            }

            // Split the raw command based on the runtime separator character.
            string[] segments = rawBuilder.ToString().Split([runtimeSeparator], StringSplitOptions.RemoveEmptyEntries);

            // Populate queue directly from array for better performance
            return new Queue<string>(segments);
        }

        /// <summary>
        /// Extracts positional tokens (command arguments) from the beginning of the segments queue.
        /// </summary>
        /// <param name="segmentQueue">
        /// The queue of string segments to process. This queue is modified as segments are consumed.
        /// After this method completes, the queue contains only option-related segments.
        /// </param>
        /// <param name="options">The terminal configuration options used to identify options and validate delimiters.</param>
        /// <returns>
        /// A collection of token strings representing command arguments in the order they appeared.
        /// Tokens are all segments that appear before the first option (identified by option prefix).
        /// Delimiter characters are removed from token values if present.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method processes segments sequentially from the front of the queue with the following logic:
        /// </para>
        /// <list type="bullet">
        /// <item>Peeks at the next segment without removing it to check if it's an option (starts with option prefix).</item>
        /// <item>If an option is detected, stops processing and leaves remaining segments in the queue for option extraction.</item>
        /// <item>For non-option segments, validates delimiter balance (opening and closing delimiters must match).</item>
        /// <item>Strips delimiter characters from token values while preserving the inner content.</item>
        /// <item>Dequeues the processed segment and adds the cleaned token to the result collection.</item>
        /// </list>
        /// <para>
        /// Tokens represent positional arguments that must appear before any options in the terminal request.
        /// For example, in <c>command arg1 arg2 --option value</c>, "arg1" and "arg2" are extracted as tokens.
        /// </para>
        /// <para>
        /// The list is pre-sized with the queue count to minimize internal array resizing operations. While this
        /// may slightly over-allocate when options are present, it provides consistent performance characteristics.
        /// </para>
        /// </remarks>
        /// <exception cref="TerminalException">
        /// Thrown when a token value starts with a delimiter but does not end with the matching delimiter.
        /// </exception>
        private IEnumerable<string> ExtractTokens(Queue<string> segmentQueue, TerminalOptions options)
        {
            // Pre-size list to avoid internal array resizing operations
            List<string> tokens = new(segmentQueue.Count);

            while (segmentQueue.Count > 0)
            {
                // Break loop if segment represents an option.
                string token = segmentQueue.Peek();
                if (TerminalServices.IsOption(token, options.Parser.OptionPrefix, out _))
                {
                    break;
                }

                // Ensure token ends with a delimiter (use indexer instead of LINQ for performance)
                if (token.Length > 0 && token[0] == options.Parser.ValueDelimiter)
                {
                    if (token[token.Length - 1] != options.Parser.ValueDelimiter)
                    {
                        throw new TerminalException(TerminalErrors.InvalidArgument, "The argument value is missing the closing delimiter. argument={0}", token);
                    }

                    token = token.Substring(1, token.Length - 2);
                }

                // Not an option so now dequeue and process the token.
                segmentQueue.Dequeue();
                tokens.Add(token);
            }
            return tokens;
        }

        private readonly ILogger<TerminalRequestQueueParser> logger;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}