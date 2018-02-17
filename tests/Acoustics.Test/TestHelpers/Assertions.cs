// <copyright file="Assertions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

        public static void AreEqual(
            this CollectionAssert collectionAssert,
            ICollection<double> expected,
            ICollection<double> actual,
            double delta,
            string message = "")
        {
            if (!ReferenceEquals(expected, actual))
            {
                if (expected == null || actual == null)
                {
                    Assert.Fail("Expected or actual is null");
                }

                if (expected.Count != actual.Count)
                {
                    Assert.Fail("The number of items in the collections differs");
                }

                var expectedEnum = expected.GetEnumerator();
                var actualEnum = actual.GetEnumerator();
                int i = 0;
                while (expectedEnum.MoveNext() && actualEnum.MoveNext())
                {
                    var actualDelta = Math.Abs(expectedEnum.Current - actualEnum.Current);
                    bool areEqual = actualDelta < delta;
                    if (!areEqual)
                    {
                        Assert.Fail(
                            $"At index {i}, expected item `{expectedEnum.Current}` does not match `{actualEnum.Current}`. "
                            + $"Actual delta is `{actualDelta}`");
                    }

                    i++;
                }

                expectedEnum.Dispose();
                actualEnum.Dispose();
            }
        }
        public static void AreEqual(
            this CollectionAssert collectionAssert,
            double[,] expected,
            double[,] actual,
            double delta,
            string message = "")
        {
            if (!ReferenceEquals(expected, actual))
            {
                if (expected == null || actual == null)
                {
                    Assert.Fail("Expected or actual is null");
                }

                if (expected.Length != actual.Length)
                {
                    Assert.Fail("The number of items in the collections differs");
                }

                for (int i = 0; i < expected.GetLength(0); i++)
                {
                    for (int j = 0; j < expected.GetLength(1); j++)
                    {
                        var expectedItem = expected[i, j];
                        var actualItem = actual[i, j];
                        var actualDelta = Math.Abs(expectedItem - actualItem);
                        bool areEqual = actualDelta < delta;
                        if (!areEqual)
                        {
                            Assert.Fail(
                                $"At index [{i},{j}], expected item `{expectedItem}` does not match `{actualItem}`. "
                                + $"Actual delta is `{actualDelta}`");
                        }

                    }
                }
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

        public static void FileExists(this Assert assert, FileInfo file)
        {
            FileExists(assert, file.FullName);
        }

        public static void FileExists(this Assert assert, string path)
        {
            Assert.IsTrue(
                File.Exists(Path.GetFullPath(path)),
                $"Expected path {path} to exist but it could not be found");
        }

        public static void FileNotExists(this Assert assert, FileInfo file)
        {
            FileNotExists(assert, file.FullName);
        }

        public static void FileNotExists(this Assert assert, string path)
        {
            Assert.IsFalse(
                File.Exists(Path.GetFullPath(path)),
                $"Expected path {path} to not exist but it does exist");
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
