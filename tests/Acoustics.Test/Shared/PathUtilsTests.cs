// <copyright file="PathUtilsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

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

        [TestMethod]
        public void CanGetShortFileNames()
        {
            var path = this.outputDirectory.CombineFile("bad folder", "REC_C_20180616_145526😂.wav").Touch();

            var actual = PathUtils.GetShortFilename(path.FullName);
            string[] expected;
            if (AppConfigHelper.IsWindows)
            {
                expected = new[]
                {
                    "C:\\", "BADFOL~1", "REC_C_~1.WAV",
                };
            }
            else if (AppConfigHelper.IsLinux || AppConfigHelper.IsMacOsX)
            {
                expected = string.Copy(actual).AsArray();
            }
            else
            {
                throw new InvalidOperationException();
            }

            foreach (var fragment in expected)
            {
                StringAssert.Contains(actual, fragment);
            }

            Assert.That.FileExists(actual);
            Assert.IsFalse(PathUtils.HasUnicodeOrUnsafeChars(actual));
        }

        [TestMethod]
        public void ShortFilenameValidatesFileExistence()
        {
            Assert.ThrowsException<FileNotFoundException>(
                () => PathUtils.GetShortFilename("I do not exist"));
        }
    }
}
