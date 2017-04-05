namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class FileEqualityHelpers
    {
        /// <summary>
        /// Use to compare two different text files line by line.
        /// </summary>
        /// <param name="expected">The file that has the expected data</param>
        /// <param name="actual">The file that we are testing with actual data</param>
        public static void TextFileEqual(FileInfo expected, FileInfo actual)
        {
            Assert.IsTrue(expected.Exists, $"Expected file does not exist at {expected.FullName}");
            Assert.IsTrue(actual.Exists, $"Expected file does not exist at {actual.FullName}");

            var expectedLines = File.ReadAllLines(expected.FullName);
            var actualLines = File.ReadAllLines(expected.FullName);

            CollectionAssert.AreEqual(expectedLines, actualLines);

            Assert.AreEqual(expected.Length, actual.Length);
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
