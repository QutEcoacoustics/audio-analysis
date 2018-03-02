namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class FileEqualityHelpers
    {
        /// <summary>
        /// Use to compare two different text files line by line.
        /// </summary>
        /// <param name="expected">The file that has the expected data</param>
        /// <param name="actual">The file that we are testing with actual data</param>
        [Obsolete("Please don't use this - testing of values based on text is inherently error prone (e.g. encoding, line endings, ...)")]
        public static void TextFileEqual(FileInfo expected, FileInfo actual)
        {
            expected.Refresh();
            actual.Refresh();

            Assert.IsTrue(expected.Exists, $"Expected file does not exist at {expected.FullName}");
            Assert.IsTrue(actual.Exists, $"Expected file does not exist at {actual.FullName}");

            var expectedLines = File.ReadAllLines(expected.FullName);
            var actualLines = File.ReadAllLines(expected.FullName);

            Assert.AreEqual(
                expectedLines.Length, 
                actualLines.Length,
                $"Line length ({expectedLines.Length}) of expected ({expected}) did not match length of ({actualLines.Length}) actual ({actual})");

            for (int i = 0; i < expectedLines.Length; i++)
            {
                // normalize line endings - on CI servers or Unix machines they may be inconsistent
                var e = expectedLines[i].NormalizeToCrLf();
                var a = actualLines[i].NormalizeToCrLf();

                Assert.AreEqual(e, a, $"Text file not equal at line {i}. Expected: `{e}`, Actual: `{a}`");
            }

            // we used to check file size bytes here but for text especially this is an unreliable test
        }

        /// <summary>
        /// Use to compare two different files byte by byte.
        /// Useful for comparing binary files, for example: PNGs, JPEGs, HDF5, etc...
        /// </summary>
        /// <param name="expected">The file that has the expected data</param>
        /// <param name="actual">The file that we are testing with actual data</param>
        public static void FileEqual(FileInfo expected, FileInfo actual)
        {
            Assert.IsTrue(expected.Exists, $"Expected file does not exist at {expected.FullName}");
            Assert.IsTrue(actual.Exists, $"Expected file does not exist at {actual.FullName}");

            Assert.AreEqual(expected.Length, actual.Length, "File lengths were not the same!");

            using (var expectedStream = File.Open(expected.FullName, FileMode.Open))
            using (var actualStream = File.Open(expected.FullName, FileMode.Open))
            using (var md5Hash = MD5.Create())
            {
                byte[] expectedHash = md5Hash.ComputeHash(expectedStream);
                byte[] actualHash = md5Hash.ComputeHash(actualStream);

                CollectionAssert.AreEqual(expectedHash, actualHash);
            }
        }
    }
}
