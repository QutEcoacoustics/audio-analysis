// -----------------------------------------------------------------------
// <copyright file="ExtMethodsDateTime.cs" company="QUT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace System
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ExtensionsDateTime
    {
        private const int DaysInYear = 364;
        private const int DaysInMonth = 30;
        private const int DaysInWeek = 7;
        private const int Limit = 1;

        /// <summary>
        /// Get a human-readable string of difference between two DateTimes (eg. 2 hours ago).
        /// </summary>
        /// <param name="first">
        /// First time (should be earlier).
        /// </param>
        /// <param name="second">
        /// Second time (should be later).
        /// </param>
        /// <returns>
        /// Human-readable string of difference between two DateTimes.
        /// </returns>
        public static string ToDifferenceString(this DateTime first, DateTime second)
        {
            if (first == DateTime.MaxValue || first == DateTime.MinValue || second == DateTime.MaxValue ||
                second == DateTime.MinValue)
            {
                return TimeSpan.MaxValue.ToDifferenceString();
            }

            return (second - first).ToDifferenceString();
        }

        /// <summary>
        /// Get a human-readable difference string representation of a TimeSpan (eg. 2 hours ago).
        /// </summary>
        /// <param name="ts">
        /// TimeSpan to convert to human-readable string.
        /// </param>
        /// <returns>
        /// TimeSpan as a human-readable string.
        /// </returns>
        public static string ToDifferenceString(this TimeSpan ts)
        {
            if (ts == TimeSpan.MaxValue || ts == TimeSpan.MinValue)
            {
                return "unknown time";
            }

            if (ts == TimeSpan.Zero)
            {
                return "right now";
            }

            var append = " ago";
            if (ts.TotalMilliseconds < 0) append = " to go";

            var strings = TimeSpanToString(ts);
            return (string.Join(" ", strings.ToArray()) + append).Trim();
        }

        /// <summary>
        /// Get a human-readable string representation of a TimeSpan (eg. 1 hour).
        /// </summary>
        /// <param name="ts">
        /// TimeSpan to convert to human-readable string.
        /// </param>
        /// <returns>
        /// TimeSpan as a human-readable string.
        /// </returns>
        public static string ToReadableString(this TimeSpan ts)
        {
            if (ts == TimeSpan.MaxValue || ts == TimeSpan.MinValue)
            {
                return "unknown duration";
            }

            if (ts == TimeSpan.Zero)
            {
                return "0";
            }

            var strings = TimeSpanToString(ts);
            return string.Join(" ", strings.ToArray()).Trim();
        }

        /// <summary>
        /// Format a TimeSpan.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <returns>
        /// The to string.
        /// </returns>
        public static string TsToString(this TimeSpan value, string format)
        {
            // This approach works for most values.
            return DateTime.MinValue.Add(value).ToString(format);
        }

        public static long ToJavascriptTimestamp(this DateTime value)
        {
            // as milliseconds since January 1, 1970 00:00
            var stamp = new DateTime(1970, 1, 1);
            var js = value - stamp;

            return Convert.ToInt64(js.TotalMilliseconds);
        }

        private static string Plural(double number)
        {
            if (number <= 1.5 && number >= 0.5)
            {
                return string.Empty;
            }

            return "s";
        }

        private static IEnumerable<string> TimeSpanToString(TimeSpan ts)
        {
            var readable = new List<string>();
            if (ts == TimeSpan.MaxValue || ts == TimeSpan.MinValue || ts == TimeSpan.Zero)
            {
                return readable;
            }

            // get absolute timespan
            ts = ts.Duration();
            const int MaxItems = 2;

            if (ts.TotalDays > DaysInYear)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInYear));
                ts = ts.Subtract(new TimeSpan(whole * DaysInYear, 0, 0, 0));
                readable.Add(whole + " year" + Plural(whole));
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.TotalDays > DaysInMonth)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInMonth));
                ts = ts.Subtract(new TimeSpan(whole * DaysInMonth, 0, 0, 0));
                readable.Add(whole + " month" + Plural(whole));
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.TotalDays > DaysInWeek)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInWeek));
                ts = ts.Subtract(new TimeSpan(whole * DaysInWeek, 0, 0, 0));
                readable.Add(whole + " week" + Plural(whole));
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.Days > 0)
            {
                readable.Add(ts.Days + " day" + Plural(ts.Days));
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.Hours > 0)
            {
                readable.Add(ts.Hours + " hr" + Plural(ts.Hours));
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.Minutes > 0)
            {
                readable.Add(ts.Minutes + " min");
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.Seconds > 0)
            {
                readable.Add(ts.Seconds + " sec");
                if (readable.Count >= MaxItems) return readable;
            }

            if (ts.Milliseconds > 0)
            {
                readable.Add(ts.Seconds + " ms");
                if (readable.Count >= MaxItems) return readable;
            }

            return readable;
        }
    }
}
