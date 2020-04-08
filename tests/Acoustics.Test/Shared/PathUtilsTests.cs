// <copyright file="PathUtilsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Acoustics.Test.TestHelpers.PlatformSpecificTestMethod;

    [TestClass]
    public class PathUtilsTests : OutputDirectoryTest
    {
        [TestMethod]
        public void CanDetectUnicodePaths()
        {
            var path = @"Y:\bad folder\REC_C_20180616_145526.wav";

            Assert.IsTrue(PathUtils.HasUnicodeOrUnsafeChars(path));

            path = @"Y:\bad folder\REC_C_20180616_145526.wav";

            Assert.IsFalse(PathUtils.HasUnicodeOrUnsafeChars(path));
        }

        [PlatformSpecificTestMethod(Windows)]
        public void CanGetShortFileNamesWindows()
        {
            var path = this.TestOutputDirectory.CombineFile("bad folder", "REC_C_20180616_145526😂.wav").Touch();

            var actual = PathUtils.GetShortFilename(path.FullName);

            // make sure each segment of the path is what we expect
            string[] expected = new[] { this.TestOutputDirectory.Root.Name, "BADFOL~1", "REC_C_~1.WAV" };
            foreach (var fragment in expected)
            {
                StringAssert.Contains(actual, fragment);
            }

            Assert.That.FileExists(actual);
            Assert.IsFalse(PathUtils.HasUnicodeOrUnsafeChars(actual));
        }

        [PlatformSpecificTestMethod(NotWindows)]
        public void CanGetShortFileNamesOther()
        {
            var path = this.TestOutputDirectory.CombineFile("bad folder", "REC_C_20180616_145526😂.wav").Touch();

            var actual = PathUtils.GetShortFilename(path.FullName);

            Assert.That.FileExists(actual);

            // path should be unchanged
            Assert.IsTrue(PathUtils.HasUnicodeOrUnsafeChars(actual));
            Assert.That.StringEqualWithDiff(path.FullName, actual);
        }

        [PlatformSpecificTestMethod(Windows)]
        public void ShortFilenameValidatesFileExistence()
        {
            Assert.ThrowsException<FileNotFoundException>(
                () => PathUtils.GetShortFilename("I do not exist"));
        }
    }
}