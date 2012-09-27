// -----------------------------------------------------------------------
// <copyright file="ExtensionsString.cs" company="QUT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Linq;
    using System.Web;

    public static class ExtensionsString
    {
        /// <summary>
        /// Truncate a string to a desired length, specifiying an ellipsis to add if the text is longer than length.
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
            Guid value;
            return s.TryParseGuidRegex(out value);
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

        /// <summary>Convert comma separated list to List of Guid.
        /// </summary>
        /// <param name="list">
        /// String to parse.
        /// </param>
        /// <returns>List of Guid.
        /// </returns>
        public static List<Guid> ParseGuidCommaSeparatedList(this string list)
        {
            if (string.IsNullOrEmpty(list))
            {
                return new List<Guid>();
            }

            var retVal = new List<Guid>();

            foreach (var s in list.Split(',').Where(s => !string.IsNullOrEmpty(s)))
            {
                Guid outGuid;
                if (s.TryParseGuidRegex(out outGuid))
                {
                    retVal.Add(outGuid);
                }
            }

            return retVal;
        }

        /// <summary>Convert comma separated list to List of int.
        /// </summary>
        /// <param name="list">
        /// String to parse.
        /// </param>
        /// <returns>List of int.
        /// </returns>
        public static List<int> ParseIntCommaSeparatedList(this string list)
        {
            if (string.IsNullOrEmpty(list))
            {
                return new List<int>();
            }

            var retVal = new List<int>();

            foreach (var s in list.Split(',').Where(s => !string.IsNullOrEmpty(s)))
            {
                int sInt;
                if (int.TryParse(s, out sInt))
                {
                    retVal.Add(sInt);
                }
            }

            return retVal;
        }

        /// <summary>Convert comma separated list to List of long.
        /// </summary>
        /// <param name="list">
        /// String to parse.
        /// </param>
        /// <returns>List of long.
        /// </returns>
        public static List<long> ParseLongCommaSeparatedList(this string list)
        {
            if (string.IsNullOrEmpty(list))
            {
                return new List<long>();
            }

            var retVal = new List<long>();

            foreach (var s in list.Split(',').Where(s => !string.IsNullOrEmpty(s)))
            {
                long value;
                if (long.TryParse(s, out value))
                {
                    retVal.Add(value);
                }
            }

            return retVal;
        }

        /// <summary>Convert comma separated list to List of double.
        /// </summary>
        /// <param name="list">
        /// String to parse.
        /// </param>
        /// <returns>List of double.
        /// </returns>
        public static List<double> ParseDoubleCommaSeparatedList(this string list)
        {
            if (string.IsNullOrEmpty(list))
            {
                return new List<double>();
            }

            var retVal = new List<double>();

            foreach (var s in list.Split(',').Where(s => !string.IsNullOrEmpty(s)))
            {
                double value;
                if (double.TryParse(s, out value))
                {
                    retVal.Add(value);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Check if a string can be parsed as a number.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// True if string can be parsed as a number, otherwise false.
        /// </returns>
        public static bool IsNumeric(this string value)
        {
            double d;
            return double.TryParse(value, out d);
        }

        /// <summary>
        /// Check if a string can be parsed as an integer.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// True if string can be parsed as an integer, otherwise false.
        /// </returns>
        public static bool IsInteger(this string value)
        {
            long l;
            return long.TryParse(value, out l);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization.
        /// </summary>
        /// <param name="pXmlString">Serialised object as xml string.</param>
        /// <returns>Xml string as byte array.</returns>
        public static byte[] StringToUtf8ByteArray(this string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
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
        /// <returns>
        /// </returns>
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

        /// <summary>
        /// Parses a query string into a dictionary.
        /// Keys will be in lower case.
        /// </summary>
        /// <param name="value">
        /// Query string to parse.
        /// </param>
        /// <returns>
        /// Dictionary containing key=value pairs.
        /// </returns>
        public static Dictionary<string, string> ParseUrlParameterString(this string value)
        {
            var retVal = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(value))
                return retVal;

            foreach (Match m in Regex.Matches(value, @"\??([^&=]*)=([^&=]*)"))
            {
                retVal.Add(HttpUtility.UrlDecode(m.Groups[1].Value).ToLowerInvariant(), HttpUtility.UrlDecode(m.Groups[2].Value));
            }

            return retVal;
        }

        public static string ToCommaSeparatedList<T>(this IEnumerable<T> items)
        {
            if (items == null || items.Count() == 0)
                return string.Empty;
            return items.Select(a => a.ToString()).Aggregate((a, b) => a + "," + b);
        }

        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
        public static string Format2(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
