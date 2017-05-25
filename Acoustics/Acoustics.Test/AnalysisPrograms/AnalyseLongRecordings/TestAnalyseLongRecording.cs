// <copyright file="TestAnalyseLongRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.AnalyseLongRecordings
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using EcoSounds.Mvc.Tests;
    using global::AnalysisPrograms.AnalyseLongRecordings;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using static global::AnalysisPrograms.Acoustic;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// (5) Wherever possible, don't use test assets
    /// </summary>
    [TestClass]
    public class TestAnalyseLongRecording
    {
        private DirectoryInfo outputDirectory;

        public TestAnalyseLongRecording()
        {
            // setup logic here
        }

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        /// Tests the analysis of an artificial seven minute long recording consisting of five harmonics.
        /// Acoustic indices as calculated from Linear frequency scale spectrogram.
        /// </summary>
        [TestMethod]
        public void TestAnalyseSr22050Recording()
        {
            int sampleRate = 22050;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics);
            string recordingPath = Path.Combine(this.outputDirectory.FullName, "TemporaryRecording.wav");

            // WARNING: The following method does NOT work. need to wait until Anthony has fixed this.
            // TODO: Anthony to fix next method call before this UNIT TEST will work.
            WavWriter.Write16BitWavFile(recording.WavReader.Samples, sampleRate, recordingPath);

            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "SineSignal1_LinearFreqScale.png");

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            //if (!arguments.Source.Exists)
            //{
            //    LoggedConsole.WriteErrorLine("WARNING! The Source Recording file cannot be found! This will cause an exception.");
            //}

            //if (!arguments.Config.Exists)
            //{
            //    LoggedConsole.WriteErrorLine("WARNING! The Configuration file cannot be found! This will cause an exception.");
            //}

            AnalyseLongRecording.Execute(arguments);

            Assert.AreEqual(0, 0);
            Assert.AreEqual(0.1, 0.1, double.Epsilon);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, new[] { 3, 2, 1 });

            FileEqualityHelpers.TextFileEqual(new FileInfo("data.txt"), new FileInfo("data.txt"));
            FileEqualityHelpers.FileEqual(new FileInfo("data.bin"), new FileInfo("data.bin"));

            // output initial data
            var actualData = new[] { 1, 2, 3 };

            //Json.Serialise("data.json".ToFileInfo(), actualData);
            //Csv.WriteMatrixToCsv("data.csv".ToFileInfo(), actualData);
            //Binary.Serialize("data.bin".ToFileInfo(), actualData);

            // Example: modifying a default config file.
            // get the default config file
            var defaultConfigFile = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");
            var config = Yaml.Deserialise<LdSpectrogramConfig>(defaultConfigFile);

            // make changes to config file as required for test
            var testConfig = new FileInfo(this.outputDirectory + "\\SpectrogramFalseColourConfig.yml");
            Yaml.Serialise(testConfig, config);
        }

        /// <summary>
        /// Tests the analysis of an artificial seven minute long recording consisting of five harmonics.
        /// Acoustic indices as calculated from Octave frequency scale spectrogram.
        /// </summary>
        [TestMethod]
        public void TestAnalyseSr64000Recording()
        {
            int sampleRate = 64000;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics);
            string recordingPath = Path.Combine(this.outputDirectory.FullName, "TemporaryRecording.wav");

            // WARNING: The following method does NOT work. need to wait until Anthony has fixed this.
            // TODO: Anthony to fix next method call before this UNIT TEST will work.
            WavWriter.Write16BitWavFile(recording.WavReader.Samples, sampleRate, recordingPath);

            string configPath = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.yml";
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "SineSignal1_LinearFreqScale.png");

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath.ToFileInfo(),
                Config = configPath.ToFileInfo(),
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);
        }
    }
}