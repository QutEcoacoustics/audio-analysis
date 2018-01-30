// <copyright file="LdSpectrogramConfigTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.IO;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class LdSpectrogramConfigTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists("SpectrogramFalseColourConfig.yml"))
            {
                File.Delete("SpectrogramFalseColourConfig.yml");
            }
        }

        [TestMethod]
        public void TestDeserializationOfTimespan()
        {
            // Michael had a comment somewhere that derserialization of TimeSpans
            // does not work properly. This test ensures it does.
            // Michael was correct kind of - so I changed XAxisTicInterval to an int to make serialization simpler
            var file = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var config = LdSpectrogramConfig.ReadYamlToConfig(file);

            Assert.AreEqual<TimeSpan>(TimeSpan.FromHours(1), config.XAxisTicInterval);
            Assert.AreEqual<double>(3600, config.XAxisTicIntervalSeconds);

            var lines = File.ReadAllLines(file.FullName);
            int index = lines.IndexOf(x => x.Contains("XAxisTicIntervalSeconds"));
            lines[index] = "XAxisTicIntervalSeconds: 127347.567";

            File.WriteAllLines("SpectrogramFalseColourConfig.yml", lines);

            var modifiedConfig = LdSpectrogramConfig.ReadYamlToConfig("SpectrogramFalseColourConfig.yml".ToFileInfo());
            Assert.AreEqual<TimeSpan>(TimeSpan.FromSeconds(127347.567), modifiedConfig.XAxisTicInterval);
            Assert.AreEqual<double>(127347.567, modifiedConfig.XAxisTicIntervalSeconds);
        }
    }
}
