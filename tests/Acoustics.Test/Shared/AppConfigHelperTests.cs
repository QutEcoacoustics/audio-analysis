// <copyright file="AppConfigHelperTests.cs" company="QutEcoacoustics">
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
    using Fasterflect;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using static Acoustics.Shared.AppConfigHelper;

    [TestClass]
    public class AppConfigHelperTests
    {
        [TestMethod]
        public void GetString()
        {
            var actual = AppConfigHelper.GetString(DefaultTargetSampleRateKey);

            Assert.AreEqual("22050", actual);
        }

        [TestMethod]
        public void GetInteger()
        {
            var actual = GetInt(DefaultTargetSampleRateKey);

            Assert.AreEqual(22050, actual);
        }

        [DataTestMethod]
        [DataRow(nameof(FfmpegExe), "ffmpeg")]
        [DataRow(nameof(FfprobeExe), "ffprobe")]
        [DataRow(nameof(SoxExe), "sox")]
        [DataRow(nameof(WvunpackExe), "wvunpac")]
        public void GetExecutable(string name, string expected)
        {
            var actual = (string)typeof(AppConfigHelper).GetPropertyValue(name);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void ExecutingAssemblyDirectoryIsSet()
        {
            var actual = ExecutingAssemblyDirectory;

            Assert.That.DirectoryExists(actual);
        }
    }
}
