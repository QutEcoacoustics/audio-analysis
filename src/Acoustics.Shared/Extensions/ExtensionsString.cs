// -----------------------------------------------------------------------
// <copyright file="ExtensionsString.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Path = System.IO.Path;

    public static class ExtensionsString
    {
        private static readonly char[] Delimiters = { ' ', '-', '_' };

        public static string ToPascalCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SplitWorkOnTokens(
                source,
                '\0',
                (s, i) => new[] { char.ToUpperInvariant(s) });
        }

        public static string ToCamelCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SplitWorkOnTokens(
                source,
                '\0',
                (s, disableFrontDelimiter) =>
                {
                    if (disableFrontDelimiter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { char.ToUpperInvariant(s) };
                });
        }

        public static string ToKebabCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SplitWorkOnTokens(
                source,
                '-',
                (s, disableFrontDelimiter) =>
                {
                    if (disableFrontDelimiter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { '-', char.ToLowerInvariant(s) };
                });
        }

        public static string ToSnakeCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SplitWorkOnTokens(
                source,
                '_',
                (s, disableFrontDelimiter) =>
                {
                    if (disableFrontDelimiter)
                    {
                        return new[] { char.ToLowerInvariant(s) };
                    }

                    return new[] { '_', char.ToLowerInvariant(s) };
                });
        }

        public static string ToTrainCase(this string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SplitWorkOnTokens(
                source,
                '-',
                (s, disableFrontDelimiter) =>
                {
                    if (disableFrontDelimiter)
                    {
                        return new[] { char.ToUpperInvariant(s) };
                    }

                    return new[] { '-', char.ToUpperInvariant(s) };
                });
        }

        private static string SplitWorkOnTokens(
            string source,
            char mainDelimiter,
            Func<char, bool, char[]> newWordSymbolHandler)
        {
            var builder = new StringBuilder();

            bool nextSymbolStartsNewWord = true;
            bool disableFrontDelimiter = true;
            for (var i = 0; i < source.Length; i++)
            {
                var symbol = source[i];
                if (Delimiters.Contains(symbol))
                {
                    if (symbol == mainDelimiter)
                    {
                        builder.Append(symbol);
                        disableFrontDelimiter = true;
                    }

                    nextSymbolStartsNewWord = true;
                }
                else if (!char.IsLetterOrDigit(symbol))
                {
                    builder.Append(symbol);
                    disableFrontDelimiter = true;
                    nextSymbolStartsNewWord = true;
                }
                else
                {
                    if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                    {
                        builder.Append(newWordSymbolHandler(symbol, disableFrontDelimiter));
                        disableFrontDelimiter = false;
                        nextSymbolStartsNewWord = false;
                    }
                    else
                    {
                        builder.Append(symbol);
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Truncate a string to a desired length, specifying an ellipsis to add if the text is longer than length.
        /// </summary>
        /// <param name="text">
        /// String to truncate.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="ellipsis">
        /// The ellipsis.
        /// </param>
        /// <param name="keepFullWordAtEnd">
        /// The keep full word at end.
        /// </param>
        /// <returns>
        /// Truncated string.
        /// </returns>
        public static string Truncate(this string text, int length, string ellipsis, bool keepFullWordAtEnd)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (text.Length < length)
            {
                return text;
            }

            text = text.Substring(0, length - ellipsis.Length);

            if (keepFullWordAtEnd && text.Contains(' '))
            {
                text = text.Substring(0, text.LastIndexOf(' '));
            }

            return text + ellipsis;
        }

        public static string WordWrap(this string text, int wrapThreshold = 120, int leftPadding = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string leftPad = string.Empty.PadRight(leftPadding);

            // wrap lines
            var lines = text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var result = new StringBuilder();

            foreach (string paragraph in lines)
            {
                if (paragraph.Length <= wrapThreshold)
                {
                    result.AppendLine(leftPad + paragraph);
                    continue;
                }

                var currentLine = paragraph;

                while (currentLine.Length > wrapThreshold)
                {
                    int splitPoint = currentLine.Substring(0, wrapThreshold).LastIndexOf(' ');

                    if (splitPoint < 0)
                    {
                        splitPoint = wrapThreshold; // cuts through a word
                    }

                    result.AppendLine(leftPad + currentLine.Substring(0, splitPoint));

                    currentLine = currentLine.Substring(splitPoint + 1);
                }

                if (currentLine.IsNotWhitespace())
                {
                    result.AppendLine(leftPad + currentLine);
                }
            }

            return result.ToString().Remove(0, leftPad.Length).TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Attempts to convert a string to a guid.
        /// </summary>
        /// <param name="s">
        /// String to convert.
        /// </param>
        /// <returns>Returns true if successful, otherwise false.
        /// </returns>
        public static bool IsGuid(this string s)
        {
            return s.TryParseGuidRegex(out _);
        }

        /// <summary>Convert comma separated list to List of string.
        /// </summary>
        /// <param name="value">
        /// String to parse.
        /// </param>
        /// <returns>List of strings.
        /// </returns>
        public static List<string> ParseCommaSeparatedList(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new List<string>();
            }

            var strings = from s in value.Split(',') where !string.IsNullOrEmpty(s) select s.Trim();

            return strings.ToList();
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array.
        /// </summary>
        /// <param name="string">The string to encode.</param>
        /// <returns>Xml string as byte array.</returns>
        public static byte[] StringToUtf8ByteArray(this string @string)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(@string);
            return byteArray;
        }

        /// <summary>
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="encoding">
        /// The encoding.
        /// </param>
        public static string ByteArrayToString(this byte[] bytes, Encoding encoding)
        {
            bytes = Encoding.Convert(encoding, Encoding.UTF8, bytes);
            var tempString = Encoding.UTF8.GetString(bytes);
            return tempString;
        }

        /// <summary>
        /// Contains overload allowing a StringComparison to be specified (easier case-insensitive string compare).
        /// </summary>
        /// <param name="source">
        /// The source string.
        /// </param>
        /// <param name="toCheck">
        /// The string to check.
        /// </param>
        /// <param name="comp">
        /// The comparison.
        /// </param>
        /// <returns>
        /// True if <paramref name="source"/> contains <paramref name="toCheck"/> using <paramref name="comp"/>, otherwise false.
        /// </returns>
        /// <remarks>
        /// from: http://stackoverflow.com/questions/444798/case-insensitive-containsstring
        /// </remarks>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            //// string.Compare(a.Tag, request.TagName, StringComparison.OrdinalIgnoreCase)
            return source.IndexOf(toCheck, comp) >= 0;
        }

        // http://stackoverflow.com/questions/2109756/how-to-get-color-from-hexadecimal-color-code-using-net
        public static Color ParseAsColor(this string str)
        {
            var hex = str.Replace("#", string.Empty);

            Color color;

            switch (hex.Length)
            {
                case 3:
                    color = Color.FromRgb(
                        byte.Parse(hex.Substring(0, 1), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(1, 1), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(2, 1), NumberStyles.HexNumber));
                    break;
                case 4:
                    color = Color.FromRgba(
                        byte.Parse(hex.Substring(1, 1), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(2, 1), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(3, 1), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(0, 1), NumberStyles.HexNumber));
                    break;
                case 6:
                    color = Color.FromRgb(
                        byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
                    break;
                case 8:
                    color = Color.FromRgba(
                        byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber));
                    break;
                default:
                    throw new FormatException("The color format was not recognized");
            }

            return color;
        }

        public static string ToCommaSeparatedList<T>(this IEnumerable<T> items)
        {
            return items == null ? string.Empty : items.Aggregate(string.Empty, (seed, b) => seed + b.ToString() + ",");
        }

        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string Format2(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool IsNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static bool IsNotWhitespace(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static string NormalizeToCrLf(this string str)
        {
            string normalized = Regex.Replace(str, @"\r\n|\n\r|\n|\r", "\r\n");
            return normalized;
        }

        public static string[] SplitOnAnyNewLine(this string str)
        {
            string[] newLines = { "\r\n", "\n" };
            return str.Split(newLines, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string FormatList(this IEnumerable<string> strings)
        {
            var builder = new StringBuilder("\n", 1000);
            foreach (var value in strings)
            {
                builder.AppendFormat("\t- {0}\n", value);
            }

            return builder.ToString();
        }

        public static string NormalizeDirectorySeparators(this string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}