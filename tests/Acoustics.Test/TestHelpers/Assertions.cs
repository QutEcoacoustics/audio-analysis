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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class Assertions
    {
        [DebuggerHidden]
        public static void AreEqual(this Assert assert, long expected, long actual, long delta, string message = null)
        {
            var actualDelta = Math.Abs(expected - actual);
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected}) and actual value ({actual}) was not less than {delta}");
            }
        }

        public static void AreEqual(this Assert assert, DateTime expected, DateTime actual, TimeSpan delta, string message = null)
        {
            var actualDelta = TimeSpan.FromTicks(Math.Abs((expected - actual).Ticks));
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected:O}) and actual value ({actual:O}) was not less than {delta}");
            }
        }

        public static void AreEqual(this Assert assert, DateTimeOffset expected, DateTimeOffset actual, TimeSpan delta, string message = null)
        {
            var actualDelta = TimeSpan.FromTicks(Math.Abs((expected - actual).Ticks));
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected:O}) and actual value ({actual:O}) was not less than {delta}");
            }
        }

        public static void AreEqual(this Assert assert, TimeSpan expected, TimeSpan actual, TimeSpan delta, string message = null)
        {
            var actualDelta = TimeSpan.FromTicks(Math.Abs((expected - actual).Ticks));
            if (actualDelta > delta)
            {
                message = message == null ? string.Empty : message + "\n";
                Assert.Fail(
                    $"{message}Actual delta ({actualDelta}) between expected value ({expected:O}) and actual value ({actual:O}) was not less than {delta}");
            }
        }

        public static void MatricesAreEqual<T>(this Assert assert, T[,] expected, T[,] actual, Func<T, T, bool> comparer = null, string message = null)
            where T : IEquatable<T>
        {
            string Error(string reason)
            {
                return $"2D arrays are not equal because {reason}."
                       + (message == null ? string.Empty : "\n" + message);
            }

            if (expected == actual)
            {
                return;
            }

            Assert.IsNotNull(expected, Error("expected should not be null"));
            Assert.IsNotNull(actual, Error("actual should not be null"));
            Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), $"Dimension 0 length of actual <{actual.GetLength(0)}> does not equal expected <{expected.GetLength(0)}>");
            Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), $"Dimension 1 length of actual <{actual.GetLength(1)}> does not equal expected <{expected.GetLength(1)}>");

            var mismatches = new List<(int I, int J, T Expected, T Actual)>(expected.Length);
            for (int i = 0; i < expected.RowLength(); i++)
            {
                for (int j = 0; j < expected.ColumnLength(); j++)
                {
                    var expectedItem = expected[i, j];
                    var actualItem = actual[i, j];
                    bool equal = comparer?.Invoke(expectedItem, actualItem)
                                 ?? expectedItem.Equals(actualItem);
                    if (!equal)
                    {
                        mismatches.Add((i, j, expectedItem, actualItem));
                    }
                }
            }

            if (mismatches.Count == 0)
            {
                return;
            }

            var report = $" the elements are different.\nThere are {mismatches.Count} mismatches at: "
                         + mismatches.Select(x => $"[{x.I},{x.J}] <{x.Expected}> vs <{x.Actual}>")
                             .FormatList();

            Assert.Fail(Error(report));
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

        public static void AreEqual(this Assert assert, FileSystemInfo expected, FileSystemInfo actual)
        {
            Assert.AreEqual(expected.FullName, actual.FullName);
        }

        public static void AreEquivalent(this CollectionAssert assert, ICollection<FileSystemInfo> expected, ICollection<FileSystemInfo> actual)
        {
            CollectionAssert.AreEquivalent(expected.Select(FullName).ToList(), actual.Select(FullName).ToList());

            string FullName(FileSystemInfo info)
            {
                return info.FullName;
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

        public static void Contains<TSequence, TResult>(
            this CollectionAssert collectionAssert,
            IEnumerable<TSequence> collection,
            TResult expected,
            Func<TSequence, TResult> mapper)
        {
            foreach (var item in collection)
            {
                var actual = mapper(item);
                if (actual.Equals(expected))
                {
                    return;
                }
            }

            Assert.Fail($"Expected '{expected}' was not found in collection");
        }

        public static void NotContains(this StringAssert assert, string value, string substring, string message = "")
        {
            Assert.IsFalse(
                value.Contains(substring),
                $"String\n{value}\n should not contain `{substring}`. {message}");
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
                        i < actualValue.Length ? actualValue[i].ToSafeString() : string.Empty); // character safe string
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
