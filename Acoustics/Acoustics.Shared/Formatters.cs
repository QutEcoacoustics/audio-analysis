namespace Acoustics.Shared
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// See: http://stackoverflow.com/questions/3627922/format-timespan-in-datagridview-column
    /// </summary>
    public class TimeSpanFormatter : IFormatProvider, ICustomFormatter
    {
        public static string FormatString = "hh:mm:ss.ff";

        private readonly Regex _formatParser;

        public TimeSpanFormatter()
        {
            _formatParser = new Regex("d{1,2}|h{1,2}|m{1,2}|s{1,2}|f{1,7}", RegexOptions.Compiled);
        }

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is TimeSpan)
            {
                var timeSpan = (TimeSpan)arg;
                return timeSpan.Humanise();
            }

            var formattable = arg as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            return arg != null ? arg.ToString() : string.Empty;
        }

        private static MatchEvaluator GetMatchEvaluator(TimeSpan timeSpan)
        {
            return m => EvaluateMatch(m, timeSpan);
        }

        private static string EvaluateMatch(Match match, TimeSpan timeSpan)
        {
            switch (match.Value)
            {
                case "dd":
                    return timeSpan.Days.ToString("00");
                case "d":
                    return timeSpan.Days.ToString("0");
                case "hh":
                    return timeSpan.Hours.ToString("00");
                case "h":
                    return timeSpan.Hours.ToString("0");
                case "mm":
                    return timeSpan.Minutes.ToString("00");
                case "m":
                    return timeSpan.Minutes.ToString("0");
                case "ss":
                    return timeSpan.Seconds.ToString("00");
                case "s":
                    return timeSpan.Seconds.ToString("0");
                case "fffffff":
                    return (timeSpan.Milliseconds * 10000).ToString("0000000");
                case "ffffff":
                    return (timeSpan.Milliseconds * 1000).ToString("000000");
                case "fffff":
                    return (timeSpan.Milliseconds * 100).ToString("00000");
                case "ffff":
                    return (timeSpan.Milliseconds * 10).ToString("0000");
                case "fff":
                    return (timeSpan.Milliseconds).ToString("000");
                case "ff":
                    return (timeSpan.Milliseconds / 10).ToString("00");
                case "f":
                    return (timeSpan.Milliseconds / 100).ToString("0");
                default:
                    return match.Value;
            }
        }
    }

    public class DateTimeFormatter : IFormatProvider, ICustomFormatter
    {
        public static string FormatString = "yyyy/MM/dd hh:mm:ss tt";

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is DateTime)
            {
                var dateTime = (DateTime)arg;
                return dateTime.ToString(format);
            }

            var formattable = arg as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            return arg != null ? arg.ToString() : string.Empty;
        }
    }

    public class ByteCountFormatter : IFormatProvider, ICustomFormatter
    {
        public static string FormatString = "approx-bytes";

        public object GetFormat(Type formatType)
        {
            if (typeof(ICustomFormatter).Equals(formatType))
            {
                return this;
            }

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is long)
            {
                var bytes = (long)arg;
                return bytes.ToByteDisplay();
            }

            var formattable = arg as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            return arg != null ? arg.ToString() : string.Empty;
        }
    }
}
