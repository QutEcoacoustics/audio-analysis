// <copyright file="PathHelperTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PathHelperTests
    {
        [TestMethod]
        public void CanDetectUnicodePaths()
        {
            var path = @"Y:\bad folder\REC_C_20180616_145526.wav";

            Assert.IsTrue(PathUtils.HasUnicodeOrUnsafeChars(path));
        }

        [TestMethod]
        public void CanGetShortFileNames()
        {
            var path = @"Y:\bad folder\REC_C_20180616_145526😂.wav";

            var actual = PathUtils.GetShortFilename(path);
            string expected;
            if (AppConfigHelper.IsWindows)
            {
                expected = @"Y:\bad folder\REC_C_?20180616_145526?.wav";
            }
            else if (AppConfigHelper.IsLinux || AppConfigHelper.IsMacOsX)
            {
                expected = string.Copy(actual);
            }
            else
            {
                throw new InvalidOperationException();
            }

            Assert.AreEqual(expected, actual);
        }
    }
}
