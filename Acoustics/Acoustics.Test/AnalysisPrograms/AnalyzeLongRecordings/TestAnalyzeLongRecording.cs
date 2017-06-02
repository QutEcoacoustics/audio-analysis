// <copyright file="TestAnalyzeLongRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.AnalyzeLongRecordings
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
    /// (3) use an appropriate serialization format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// (5) Wherever possible, don't use test assets
    /// </summary>
    [TestClass]
    public class TestAnalyzeLongRecording
    {
        private DirectoryInfo outputDirectory;

        public TestAnalyzeLongRecording()
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
        public void TestAnalyzeSr22050Recording()
        {
            int sampleRate = 22050;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics);
            var recordingPath = this.outputDirectory.CombineFile("TemporaryRecording.wav");

            WavWriter.WriteWavFileViaFfmpeg(recordingPath, recording.WavReader);

            var configPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");
            var outputImagePath = this.outputDirectory.CombineFile("SineSignal1_LinearFreqScale.png");

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = configPath,
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);

            // TODO: @towsey needs to actually write the assertions (the blow ones are just defaults)
            Assert.Inconclusive("Not implemented");
        }

        /// <summary>
        /// Tests the analysis of an artificial seven minute long recording consisting of five harmonics.
        /// Acoustic indices as calculated from Octave frequency scale spectrogram.
        /// </summary>
        [TestMethod]
        public void TestAnalyzeSr64000Recording()
        {
            int sampleRate = 64000;
            double duration = 420; // signal duration in seconds = 7 minutes
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics);
            var recordingPath = this.outputDirectory.CombineFile("TemporaryRecording.wav");

            WavWriter.WriteWavFileViaFfmpeg(recordingPath, recording.WavReader);

            var configPath = PathHelper.ResolveConfigFile("Towsey.Acoustic.yml");
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "SineSignal1_LinearFreqScale.png");

            var arguments = new AnalyseLongRecording.Arguments
            {
                Source = recordingPath,
                Config = configPath,
                Output = this.outputDirectory,
                MixDownToMono = true,
            };

            AnalyseLongRecording.Execute(arguments);

            // TODO: @towsey needs to actually write the assertions
            Assert.Inconclusive("Not implemented");
        }
    }
}