﻿// <copyright file="IndexCalculateTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // using TestHelpers;

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

            // var outputDir = this.outputDirectory;

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
            Assert.AreEqual(0.957433, summaryIndices.Ndsi, 0.000001);
            Assert.AreEqual(27.877206, summaryIndices.Snr, 0.000001);
            Assert.AreEqual(6.256191, summaryIndices.SptDensity, 0.000001);
            Assert.AreEqual(TimeSpan.Zero, summaryIndices.StartOffset);
            Assert.AreEqual(0.162215, summaryIndices.TemporalEntropy, 0.000001);
            Assert.AreEqual(116, summaryIndices.ThreeGramCount, 0.000001);

            // CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            // CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, new[] { 3, 2, 1 });

            // FileEqualityHelpers.TextFileEqual(new FileInfo("data.txt"), new FileInfo("data.txt"));
            // FileEqualityHelpers.FileEqual(new FileInfo("data.bin"), new FileInfo("data.bin"));

            // output initial data
            // var actualData = new[] { 1, 2, 3 };
            // Json.Serialise("data.json".ToFileInfo(), actualData);
            // Csv.WriteMatrixToCsv("data.csv".ToFileInfo(), actualData);
            // Binary.Serialize("data.bin".ToFileInfo(), actualData);
        }

        /// <summary>
        /// Test the various spectral indices
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices()
        {
            var sourceRecording = new FileInfo(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = @"Indices\Towsey.Acoustic.yml".ToFileInfo();
            var indexPropertiesConfig = new FileInfo(@"Configs\IndexPropertiesConfig.yml");

            // var outputDir = this.outputDirectory;
            var outputDir = "Indices";

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

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            // After serialising the expected vector, comment the Binary.Serialise line and copy file to dir TestResources\Indices.

            // ACI
            var expectedSpectrumFile = new FileInfo(outputDir + "\\ACI.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ACI);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ACI);

            // BGN
            expectedSpectrumFile = new FileInfo(outputDir + "\\BGN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.BGN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.BGN);

            // CLS
            expectedSpectrumFile = new FileInfo(outputDir + "\\CLS.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.CLS);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.CLS);

            // CVR
            expectedSpectrumFile = new FileInfo(outputDir + "\\CVR.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.CVR);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.CVR);

            // DMN
            expectedSpectrumFile = new FileInfo(outputDir + "\\DMN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.DMN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.DMN);

            // ENT
            expectedSpectrumFile = new FileInfo(outputDir + "\\ENT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ENT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ENT);

            // EVN
            expectedSpectrumFile = new FileInfo(outputDir + "\\EVN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.EVN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.EVN);

            // POW
            expectedSpectrumFile = new FileInfo(outputDir + "\\POW.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.POW);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.POW);

            // RHZ
            expectedSpectrumFile = new FileInfo(outputDir + "\\RHZ.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RHZ);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RHZ);

            // RNG
            expectedSpectrumFile = new FileInfo(outputDir + "\\RNG.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RNG);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RNG);

            // RPS
            expectedSpectrumFile = new FileInfo(outputDir + "\\RPS.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RPS);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RPS);

            // RVT
            expectedSpectrumFile = new FileInfo(outputDir + "\\RVT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RVT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RVT);

            // R3D
            expectedSpectrumFile = new FileInfo(outputDir + "\\R3D.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.R3D);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.R3D);

            // SPT
            expectedSpectrumFile = new FileInfo(outputDir + "\\SPT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.SPT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.SPT);
        }
    }
}