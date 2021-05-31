// <copyright file="MultiRecogniserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using AudioAnalysisTools.Events;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MoreLinq.Extensions;

    [TestClass]
    public class MultiRecogniserTests : OutputDirectoryTest
    {
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "Powerful3AndBoobook0_ksh3_1773_510819_20171109_174311_30_0.wav");
        private static readonly FileInfo ConfigFile1 = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.NinoxStrenua.yml");
        private static readonly FileInfo ConfigFile2 = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.NinoxBoobook.yml");
        private static readonly AudioRecording Recording = new AudioRecording(TestAsset);
        private static readonly MultiRecognizer Recognizer = new MultiRecognizer();

        [TestMethod]
        public void MultiRecogniserDeserializationAcceptsEitherFilenameOrAnalysisName()
        {
            var thisTestBoobookConfig = ConfigFile2.CopyTo(this.TestOutputDirectory);
            var configFileFromBuildDir = Path.Combine(PathHelper.TestBuild, "ConfigFiles", "RecognizerConfigFiles", "Towsey.NinoxStrenua.yml");

            var literalConfig = $@"
AnalysisNames:
    - Towsey.NinoxStrenua.yml
    - Towsey.NinoxStrenua
    - {ConfigFile1.FullName}
    - Towsey.NinoxBoobook.yml
    - ./Towsey.NinoxBoobook.yml
";
            var configFile = this.TestOutputDirectory.CombineFile("QUT.MultiRecognizer.yml");

            File.WriteAllText(configFile.FullName, literalConfig);

            var config = ConfigFile.Deserialize<MultiRecognizer.MultiRecognizerConfig>(configFile);

            Assert.AreEqual(5, config.Analyses.Length);

            // first three should resolve to "default" configs from config file folder
            // first two, resolve to copy with built files, last, is an absolute path reference to source config file
            var first3Recognisers = config.Analyses.Take(3).Select(x => x.Recognizer).ToList();
            CollectionAssert.AllItemsAreInstancesOfType(first3Recognisers, typeof(NinoxStrenua));
            var first3Configs = config.Analyses.Take(3).Select(x => x.Configuration).ToList();
            CollectionAssert.AllItemsAreInstancesOfType(first3Configs, typeof(NinoxStrenua.NinoxStrenuaConfig));
            CollectionAssert.AreEqual(
                first3Configs.Select(x => x.ConfigPath).ToArray(),
                new[]
                {
                    configFileFromBuildDir,
                    configFileFromBuildDir,
                    ConfigFile1.FullName,
                });

            // last two should be resolved relative to the config file
            var last2Recognisers = config.Analyses.Skip(3).Select(x => x.Recognizer).ToList();
            CollectionAssert.AllItemsAreInstancesOfType(last2Recognisers, typeof(NinoxBoobook));
            var last2Configs = config.Analyses.Skip(3).Select(x => x.Configuration).ToList();
            CollectionAssert.AllItemsAreInstancesOfType(last2Configs, typeof(NinoxBoobook.NinoxBoobookConfig));
            CollectionAssert.AreEqual(
                last2Configs.Select(x => x.ConfigPath).ToArray(),
                new[]
                {
                    thisTestBoobookConfig.FullName,
                    thisTestBoobookConfig.FullName,
                });
        }

        [TestMethod]
        public void MultiRecogniserDeserializationValidatesAnalysisNamesArePresent()
        {
            var literalConfig = $@"
AnalysisNames: ~
";
            var configFile = this.TestOutputDirectory.CombineFile("QUT.MultiRecognizer.yml");

            File.WriteAllText(configFile.FullName, literalConfig);

            var exception = Assert.ThrowsException<ConfigFileException>(
                () => ConfigFile.Deserialize<MultiRecognizer.MultiRecognizerConfig>(configFile));
            StringAssert.Contains(
                exception.Message,
                "AnalysisNames cannot be null or empty. It should be a list with at least one config file in it");
        }

        [TestMethod]
        public void MultiRecogniserWorks()
        {
            var literalConfig = $@"
AnalysisNames:
    - Towsey.NinoxStrenua
    - Towsey.NinoxBoobook
";
            var configFile = this.TestOutputDirectory.CombineFile("QUT.MultiRecognizer.yml");
            File.WriteAllText(configFile.FullName, literalConfig);
            var config = Recognizer.ParseConfig(configFile);

            var results = Recognizer.Recognize(
                Recording,
                config,
                TimeSpan.Zero,
                null,
                this.TestOutputDirectory,
                null);

            // basically checking we have a mix of each type of event - we really don't care how many of each since
            // this test is not testing the recognisers themselves
            Assert.IsTrue(results.GetAllEvents().Count() > 2);
            Assert.IsTrue(results.GetAllEvents().Where(x => (x as SpectralEvent)?.Name == "NinoxBoobook").Count() > 1);
            Assert.IsTrue(results.GetAllEvents().Where(x => (x as SpectralEvent)?.Name == "NinoxStrenua").Count() > 1);
        }
    }
}
