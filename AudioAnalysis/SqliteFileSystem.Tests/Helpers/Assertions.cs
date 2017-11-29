﻿namespace SqliteFileSystem.Tests.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class Assertions
    {
        [DebuggerHidden]
        public static void AreClose(this Assert assert, long expected, long actual, long delta, string message = null)
        {
            var actualDelta = Math.Abs(expected - actual);
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected}) and actual value ({actual}) was not less than {delta}");
            }
        }

        public static void AreClose(this Assert assert, DateTime expected, DateTime actual, TimeSpan delta, string message = null)
        {
            var actualDelta = TimeSpan.FromTicks(Math.Abs((expected - actual).Ticks));
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected:O}) and actual value ({actual:O}) was not less than {delta}");
            }
        }

        public static void DirectoryExists(this Assert assert, DirectoryInfo directory)
        {
            DirectoryExists(assert, directory.FullName);
        }

        public static void DirectoryExists(this Assert assert, string path)
        {
            Assert.IsTrue(
                Directory.Exists(Path.GetFullPath(path)),
                $"Expected path {path} to exist but it could not be found");
        }

        public static void PathExists(this Assert assert, string path, string message = "")
        {
            path = Path.GetFullPath(path);

            Assert.IsTrue(
                Directory.Exists(path) || File.Exists(path),
                $"Expected path {path} to exist but it could not be found. {message}");
        }

        public static void PathNotExists(this Assert assert, string path, string message = "")
        {
            path = Path.GetFullPath(path);

            Assert.IsFalse(
                Directory.Exists(path) || File.Exists(path),
                $"Expected path {path} NOT to exist but it was found. {message}");
        }

        public enum DiffStyle
        {
            Full,
            Minimal,
        }

        public static void StringEqualWithDiff(this Assert assert, string expectedValue, string actualValue)
        {
            ShouldEqualWithDiff(actualValue, expectedValue, DiffStyle.Full, Console.Out);
        }

        public static void StringEqualWithDiff(this Assert assert, string expectedValue, string actualValue, DiffStyle diffStyle)
        {
            ShouldEqualWithDiff(actualValue, expectedValue, diffStyle, Console.Out);
        }

        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue)
        {
            ShouldEqualWithDiff(actualValue, expectedValue, DiffStyle.Full, Console.Out);
        }

        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue, DiffStyle diffStyle)
        {
            ShouldEqualWithDiff(actualValue, expectedValue, diffStyle, Console.Out);
        }

        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue, DiffStyle diffStyle, TextWriter output)
        {
            if (actualValue == null || expectedValue == null)
            {
                Assert.AreEqual(expectedValue, actualValue);
                return;
            }

            if (actualValue.Equals(expectedValue, StringComparison.Ordinal))
            {
                return;
            }

            output.WriteLine("  Idx Expected  Actual");
            output.WriteLine("-------------------------");
            int maxLen = Math.Max(actualValue.Length, expectedValue.Length);
            int minLen = Math.Min(actualValue.Length, expectedValue.Length);
            for (int i = 0; i < maxLen; i++)
            {
                if (diffStyle != DiffStyle.Minimal || i >= minLen || actualValue[i] != expectedValue[i])
                {
                    output.WriteLine(
                        "{0} {1,-3} {2,-4} {3,-3}  {4,-4} {5,-3}",
                        i < minLen && actualValue[i] == expectedValue[i] ? " " : "*", // put a mark beside a differing row
                        i, // the index
                        i < expectedValue.Length ? ((int)expectedValue[i]).ToString() : string.Empty, // character decimal value
                        i < expectedValue.Length ? expectedValue[i].ToSafeString() : string.Empty, // character safe string
                        i < actualValue.Length ? ((int)actualValue[i]).ToString() : string.Empty, // character decimal value
                        i < actualValue.Length ? actualValue[i].ToSafeString() : string.Empty // character safe string
                    );
                }
            }

            output.WriteLine();

            Assert.AreEqual(expectedValue, actualValue);
        }

        private static string ToSafeString(this char c)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return $"\\u{(int)c:X};";
                }
            }

            return c.ToString(CultureInfo.InvariantCulture);
        }
    }
}
