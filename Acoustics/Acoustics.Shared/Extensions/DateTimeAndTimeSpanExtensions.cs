﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeAndTimeSpanExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using Collections.Generic;
    using Linq;

    /// <summary>
    /// Extensions for the <c>DateTime</c> and <c>TimeSpan</c> structs.
    /// </summary>
    public static class DateTimeAndTimeSpanExtensions
    {
        private const int DaysInYear = 365;

        private const int DaysInMonth = 30;

        private const int DaysInWeek = 7;

        /// <summary>
        /// Get a human readable representation of the time span.
        /// </summary>
        /// <param name="timeSpan">
        /// The time span.
        /// </param>
        /// <param name="addSuffixOrPrefix">
        /// True to add a suffix or prefix as appropriate. False to not add a suffix or prefix.
        /// </param>
        /// <returns>
        /// Human readable representation of the time span.
        /// </returns>
        public static string Humanise(this TimeSpan timeSpan, bool addSuffixOrPrefix = false)
        {
            // was last modified ___ (right now, at an unknown time, 2 hours 3 minutes ago) (addSuffixOrPrefix = true)
            // will be available ___ (right now, at an unknown time, in 2 hours 3 minutes) (addSuffixOrPrefix = true)
            // the difference between these times is ___ (nothing, unknown, 2 hours 3 minutes)  (addSuffixOrPrefix = false)
            if (timeSpan == TimeSpan.MaxValue || timeSpan == TimeSpan.MinValue)
            {
                return addSuffixOrPrefix ? "at an unknown time" : "unknown";
            }

            if (timeSpan == TimeSpan.Zero)
            {
                return addSuffixOrPrefix ? "right now" : "nothing";
            }

            const string Suffix = " ago";
            const string Prefix = "in ";

            var strings = TimeSpanToString(timeSpan).ToList();
            var valueString = string.Join(" ", strings);

            if (addSuffixOrPrefix)
            {
                valueString = timeSpan.TotalMilliseconds < 0 ? valueString + Suffix : Prefix + valueString;
            }

            return valueString.Trim();
        }

        /// <summary>
        /// Get a human-readable string of difference between two DateTimes (eg. 2 hours, 2 hours ago, in 2 hours).
        /// </summary>
        /// <param name="first">
        /// First time (should be earlier).
        /// </param>
        /// <param name="second">
        /// Second time (should be later).
        /// </param>
        /// <param name="addSuffixOrPrefix">
        /// The add Suffix Or Prefix.
        /// </param>
        /// <returns>
        /// Human-readable string of difference between two DateTimes.
        /// </returns>
        public static string Humanise(this DateTimeOffset first, DateTimeOffset second, bool addSuffixOrPrefix = false)
        {
            if (first == DateTimeOffset.MaxValue || first == DateTimeOffset.MinValue
                || second == DateTimeOffset.MaxValue || second == DateTimeOffset.MinValue)
            {
                return TimeSpan.MaxValue.Humanise();
            }

            return (second - first).Humanise();
        }

        /// <summary>
        /// Get a human-readable string of difference between two DateTimes (eg. 2 hours, 2 hours ago, in 2 hours).
        /// </summary>
        /// <param name="first">
        /// First time (should be earlier).
        /// </param>
        /// <param name="second">
        /// Second time (should be later).
        /// </param>
        /// <param name="addSuffixOrPrefix">
        /// The add Suffix Or Prefix.
        /// </param>
        /// <returns>
        /// Human-readable string of difference between two DateTimes.
        /// </returns>
        public static string Humanise(this DateTime first, DateTime second, bool addSuffixOrPrefix = false)
        {
            return Humanise(new DateTimeOffset(first), new DateTimeOffset(second));
        }

        /// <summary>
        /// Gets the minutes and seconds of a time span in the format required for an offset from UTC.
        /// </summary>
        /// <param name="ts">
        /// The time span.
        /// </param>
        /// <returns>
        /// Time zone offset formatted string.
        /// </returns>
        public static string ToTimeZoneString(this TimeSpan ts)
        {
            return (ts < TimeSpan.Zero ? "-" : string.Empty) + ts.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Gets the DateTimeOffset formatted as a javascript timestamp.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// Javascript timestamp.
        /// </returns>
        public static long ToJavascriptTimestamp(this DateTimeOffset value)
        {
            // as milliseconds since January 1, 1970 00:00
            var stamp = new DateTimeOffset(new DateTime(1970, 1, 1));
            var js = value - stamp;

            return Convert.ToInt64(js.TotalMilliseconds);
        }

        /// <summary>
        /// Gets the DateTime formatted as a javascript timestamp.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// Javascript timestamp.
        /// </returns>
        public static long ToJavascriptTimestamp(this DateTime value)
        {
            return ToJavascriptTimestamp(new DateTimeOffset(value));
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
            const int MaxItems = 3;

            if (ts.TotalDays > DaysInYear)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInYear));
                ts = ts.Subtract(new TimeSpan(whole * DaysInYear, 0, 0, 0));
                readable.Add(whole + " year" + Plural(whole));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.TotalDays > DaysInMonth)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInMonth));
                ts = ts.Subtract(new TimeSpan(whole * DaysInMonth, 0, 0, 0));
                readable.Add(whole + " month" + Plural(whole));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.TotalDays > DaysInWeek)
            {
                var whole = Convert.ToInt32(Math.Floor(ts.TotalDays / DaysInWeek));
                ts = ts.Subtract(new TimeSpan(whole * DaysInWeek, 0, 0, 0));
                readable.Add(whole + " week" + Plural(whole));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.Days > 0)
            {
                readable.Add(ts.Days + " day" + Plural(ts.Days));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.Hours > 0)
            {
                readable.Add(ts.Hours + " hour" + Plural(ts.Hours));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.Minutes > 0)
            {
                readable.Add(ts.Minutes + " minute" + Plural(ts.Minutes));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.Seconds > 0)
            {
                readable.Add(ts.Seconds + " second" + Plural(ts.Seconds));
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            if (ts.Milliseconds > 0)
            {
                readable.Add(ts.Milliseconds + " ms");
                if (readable.Count >= MaxItems)
                {
                    return readable;
                }
            }

            return readable;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time"></param>
        /// <param name="roundingInterval"></param>
        /// <param name="roundingType"></param>
        /// <returns></returns>
        /// <remarks>From: http://stackoverflow.com/questions/766626/is-there-a-better-way-in-c-sharp-to-round-a-datetime-to-the-nearest-5-seconds (Stackoverflow).</remarks>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType)
        {
            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType)) * roundingInterval.Ticks);
        }

        /// <remarks>From: http://stackoverflow.com/questions/766626/is-there-a-better-way-in-c-sharp-to-round-a-datetime-to-the-nearest-5-seconds (Stackoverflow).</remarks>
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval)
        {
            return Round(time, roundingInterval, MidpointRounding.ToEven);
        }

        /// <remarks>From: http://stackoverflow.com/questions/766626/is-there-a-better-way-in-c-sharp-to-round-a-datetime-to-the-nearest-5-seconds (Stackoverflow).</remarks>
        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval)
        {
            return new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
        }

        /// <summary>
        /// Multiplies a timespan by a scalar value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        }

        /// <summary>
        /// Divides a timespan by an scalar value
        /// </summary>
        public static TimeSpan Divide(this TimeSpan dividend, int divisor)
        {
            return TimeSpan.FromTicks(dividend.Ticks / divisor);
        }

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }
        
        /// <summary>
        /// Divides a timespan by an scalar value
        /// </summary>
        public static TimeSpan Divide(this TimeSpan dividend, double divisor)
        {
            return TimeSpan.FromTicks((long)(dividend.Ticks / divisor));
        }

        // https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/DateTimeOffsetExtensions.cs#L222
        public static DateTimeOffset Floor(this DateTimeOffset date, TimeSpan interval)
        {
            return date.AddTicks(-(date.Ticks % interval.Ticks));
        }

        // https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/DateTimeOffsetExtensions.cs#L226
        public static DateTimeOffset Ceiling(this DateTimeOffset date, TimeSpan interval)
        {
            return date.AddTicks(interval.Ticks - (date.Ticks % interval.Ticks));
        }

        // https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/src/Exceptionless.DateTimeExtensions/DateTimeOffsetExtensions.cs#L230
        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan roundingInterval)
        {
            var halfIntervalTicks = ((roundingInterval.Ticks + 1) >> 1);
            return date.AddTicks(halfIntervalTicks - ((date.Ticks + halfIntervalTicks) % roundingInterval.Ticks));
        }

        public static TimeSpan Absolute(this TimeSpan span)
        {
            return span < TimeSpan.Zero ? new TimeSpan(span.Ticks * -1) : span;
        }

        public static TimeSpan Min(this TimeSpan t1, TimeSpan t2)
        {
            return t1 <= t2 ? t1 : t2;
        }

        public static TimeSpan Max(this TimeSpan t1, TimeSpan t2)
        {
            return t1 >= t2 ? t1 : t2;
        }

    }
}
