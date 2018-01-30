using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Minimod.PrettyText
{
    /// <summary>
    /// <h1>Minimod.PrettyText, Version 2.0.0, Copyright © Lars Corneliussen 2011</h1>
    /// <para>A minimod with string extensions, helping whereever you have to shape text to fit into a box.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class PrettyTextMinimod
    {
        /// <summary>
        /// Fits the text into <paramref name="length"/>, appending <value>...</value> if it is too long.
        /// </summary>
        public static string ShortenTo(this string text, int length)
        {
            return ShortenTo(text, length, "...");
        }

        /// <summary>
        /// Fits the text into <paramref name="length"/>, appending <paramref name="hint"/> if it is too long.
        /// </summary>
        public static string ShortenTo(this string text, int length, string hint)
        {
            if (text == null)
                return null;

            return text.Length > length ? text.Substring(0, length - hint.Length) + hint : text;
        }

        /// <summary>
        /// Splits into lines by using <see cref="Environment.NewLine"/>.
        /// </summary>
        public static IEnumerable<string> SplitLines(this string input)
        {
            if (input == null)
                return new string[0];

            return input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        /// <summary>
        /// Join lines by using <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string JoinLines(this IEnumerable<string> lines)
        {
            if (lines == null) throw new ArgumentNullException("lines");

            return string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>
        /// Wraps lines exceeding a certain margin. For each line, the indentation is maintained.
        /// </summary>
        /// <example>
        /// <code>
        /// <pre>
        /// Some long line here
        ///   Another indented long line here withalongerthan15word
        /// </pre>
        /// </code>
        /// wrapped at 15 will produce:
        /// <code>
        /// <pre>
        /// Some long line 
        /// here
        ///   Another 
        ///   indented long 
        ///   line here 
        ///   withalongerth
        ///   an15word
        /// </pre>
        /// </code>
        /// </example>
        /// <remarks>
        /// based on: http://blueonionsoftware.com/blog.aspx?p=6091173d-6bdb-498c-9d57-c0da43319839
        /// </remarks>
        public static IEnumerable<string> WrapAt(this string input, int margin)
        {
            var resultLines = new List<string>();

            foreach (var line in SplitLines(input))
            {
                var lines = new List<string>();
                var leadingSpaces = Regex.Match(line, "^\\s*").Value.Length;

                var sublineMargin = margin - leadingSpaces;
                var text = line.Substring(leadingSpaces).TrimEnd();

                int start = 0, end;

                while ((end = start + sublineMargin) < text.Length)
                {
                    // walk back; is there a space in the line?
                    while (text[end] != ' ' && end > start)
                        end -= 1;

                    // ok, we found a space
                    if (end != start)
                    {
                        lines.Add(text.Substring(start, end - start));
                        start = end + 1; // +1 means jump over the space
                    }
                    // no space? then crop hardly
                    else
                    {
                        end = start + sublineMargin;
                        lines.Add(text.Substring(start, end - start));
                        start = end;
                    }
                }

                if (start < text.Length)
                    lines.Add(text.Substring(start));

                resultLines.AddRange(lines.IndentBy(leadingSpaces));
            }

            return resultLines;
        }

        /// <summary>
        /// Indents each line by specified number of spaces.
        /// </summary>
        public static IEnumerable<string> IndentBy(this IEnumerable<string> lines, int leadingSpaces)
        {
            return IndentBy(lines, leadingSpaces, IndentOptions.None);
        }

        /// <summary>
        /// Indents each line by specified number of spaces.
        /// </summary>
        public static IEnumerable<string> IndentBy(this IEnumerable<string> lines, int leadingSpaces, IndentOptions options)
        {
            if (lines == null) throw new ArgumentNullException("lines");

            bool firstLine = true;
            foreach (var line in lines)
            {
                if (firstLine && options.HasAnyFlag(options))
                {
                    yield return line;
                }

                yield return new string(' ', leadingSpaces) + line;
            }
        }

        /// <summary>
        /// Indents each line by specified number of spaces.
        /// </summary>
        public static string IndentLinesBy(this string lines, int leadingSpaces)
        {
            return IndentLinesBy(lines, leadingSpaces, IndentOptions.None);
        }

        /// <summary>
        /// Indents each line by specified number of spaces.
        /// </summary>
        public static string IndentLinesBy(this string lines, int leadingSpaces, IndentOptions options)
        {
            if (lines == null) throw new ArgumentNullException("lines");

            return lines.SplitLines().IndentBy(leadingSpaces, options).JoinLines();
        }

        [Flags]
        public enum IndentOptions
        {
            None,
            SkipFirst
        }

        /// <remarks>
        /// copied from http://www.mmowned.com/forums/world-of-warcraft/bots-programs/memory-editing/297201-net-4-0-enum-hasflag-not-what-you-might-expect.html
        /// </remarks>>
        private static bool HasAnyFlag(this Enum value, Enum flags)
        {
            var val = ((IConvertible)value).ToUInt64(null);
            var test = ((IConvertible)flags).ToUInt64(null);

            return (val & test) != 0;
        }
    }
}
