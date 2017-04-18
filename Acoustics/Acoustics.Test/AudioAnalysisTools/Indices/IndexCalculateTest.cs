// <copyright file="ExampleTestTemplate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// </summary>
    [TestClass]
    public class IndexCalculateTest
    {
        private DirectoryInfo outputDirectory;

        public IndexCalculateTest()
        {
            // setup logic here
        }

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = TestHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        /// Test the various summary indices
        /// </summary>
        [TestMethod]
        public void TestOfSummaryIndices()
        {
            var sourceRecording = new FileInfo(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = @"Indices\Towsey.Acoustic.yml".ToFileInfo();
            var indexPropertiesConfig = new FileInfo(@"Configs\IndexPropertiesConfig.yml");
            var outputDir = this.outputDirectory;

            // 1. get the config dictionary
            var configDict = Oscillations2014.GetConfigDictionary(configFile, true);
            configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

            // 2. Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var recording = new AudioRecording(sourceRecording);
            var subsegmentOffsetTimeSpan = TimeSpan.Zero;
            int sampleRateOfOriginalAudioFile = 22050;
            var indexCalculationDuration = TimeSpan.FromSeconds(60);
            var bgNoiseNeighborhood = TimeSpan.FromSeconds(5); // not use in this test where subsegment = 60 duration.
            var segmentStartOffset = TimeSpan.Zero; // assume zero offset
            dynamic configuration = Yaml.Deserialise(configFile);

            var results = IndexCalculate.Analysis(
                recording,
                subsegmentOffsetTimeSpan,
                indexCalculationDuration,
                bgNoiseNeighborhood,
                indexPropertiesConfig,
                sampleRateOfOriginalAudioFile,
                segmentStartOffset,
                configuration,
                returnSonogramInfo: true);

            var summaryIndices = results.SummaryIndexValues;
            Assert.AreEqual(0.6793287, summaryIndices.AcousticComplexity, 0.000001);
            Assert.AreEqual(0.484520, summaryIndices.Activity, 0.000001);
            Assert.AreEqual(0.000000, summaryIndices.AvgEntropySpectrum, 0.000001);
            Assert.AreEqual(-30.946519, summaryIndices.AvgSignalAmplitude, 0.000001);
            Assert.AreEqual(11.533420, summaryIndices.AvgSnrOfActiveFrames, 0.000001);
            Assert.AreEqual(-39.740775, summaryIndices.BackgroundNoise, 0.000001);
            Assert.AreEqual(15, summaryIndices.ClusterCount);
            Assert.AreEqual(0.153148, summaryIndices.EntropyOfAverageSpectrum, 0.000001);
            Assert.AreEqual(0.301938, summaryIndices.EntropyOfCoVSpectrum, 0.000001);
            Assert.AreEqual(0.259239, summaryIndices.EntropyOfPeaksSpectrum, 0.000001);
            Assert.AreEqual(0.522080, summaryIndices.EntropyOfVarianceSpectrum, 0.000001);
            Assert.AreEqual(0.0, summaryIndices.EntropyPeaks, 0.000001);
            Assert.AreEqual(2.0, summaryIndices.EventsPerSecond, 0.000001);
            Assert.AreEqual(0.139952, summaryIndices.HighFreqCover, 0.000001);
            Assert.AreEqual(0.137873, summaryIndices.MidFreqCover, 0.000001);
            Assert.AreEqual(0.054840, summaryIndices.LowFreqCover, 0.000001);
            Assert.AreEqual(0.957433, summaryIndices.NDSI, 0.000001);

            //CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            //CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, new[] { 3, 2, 1 });

            //FileEqualityHelpers.TextFileEqual(new FileInfo("data.txt"), new FileInfo("data.txt"));
            //FileEqualityHelpers.FileEqual(new FileInfo("data.bin"), new FileInfo("data.bin"));

            // output initial data
            //var actualData = new[] { 1, 2, 3 };
            //Json.Serialise("data.json".ToFileInfo(), actualData);
            //Csv.WriteMatrixToCsv("data.csv".ToFileInfo(), actualData);
            //Binary.Serialize("data.bin".ToFileInfo(), actualData);
        }

        /// <summary>
        /// Test the various spectral indices
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices()
        {
            var sourceRecording = @"Recordings\BAC2_20071008-085040.wav".ToFileInfo();
            var configFile = @"Indices\Towsey.Sonogram.yml".ToFileInfo();
            var outputDir = this.outputDirectory;

            // 1. get the config dictionary
            var configDict = Oscillations2014.GetConfigDictionary(configFile, true);
            configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

            // 2. Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

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
        }

    }
}
