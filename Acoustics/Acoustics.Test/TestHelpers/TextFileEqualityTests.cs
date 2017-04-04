namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class TextFileEqualityTests
    {

        public static void TextFileEqual(FileInfo expected, FileInfo actual)
        {
            Assert.IsTrue(expected.Exists);
            Assert.IsTrue(actual.Exists);

            var expectedLines = File.ReadAllLines(expected.FullName);
            var actualLines = File.ReadAllLines(expected.FullName);

            CollectionAssert.AreEqual(expectedLines, actualLines);

            Assert.AreEqual(expected.Length, actual.Length);
        }

    }
}
