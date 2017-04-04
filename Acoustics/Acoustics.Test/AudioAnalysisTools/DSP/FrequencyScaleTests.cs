// <copyright file="FrequencyScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    /// <summary>
    /// Test methods for the various Frequency Scales
    /// </summary>
    [TestClass]
    public class FrequencyScaleTests
    {
        public FrequencyScaleTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        private DirectoryInfo outputDirectory;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        /*
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        */

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

        #endregion

        /// <summary>
        /// METHOD TO CHECK IF Default linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void LinearFrequencyScaleDefault()
        {
            var recordingPath = @"BAC\BAC2_20071008-085040.wav";
            var outputDir = this.outputDirectory;
            var expectedResultsDir = Path.Combine(outputDir.FullName, "ExpectedTestResults").ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "linearScaleSonogram_default.png");

            var recording = new AudioRecording(recordingPath);

            // default linear scale
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath, ImageFormat.Png);

            // DO UNIT TESTING
            var opFileStem = "BAC2_20071008";

            // Check that freqScale.OctaveBinBounds are correct
            var expectedTestFile1 = new FileInfo("FrequencyScale\\FrequencyDefaultOctaveBinBounds.EXPECTED.json");
            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultOctaveBinBounds.ACTUAL.json"));
            Json.Serialise(resultFile1, freqScale.OctaveBinBounds);
            TextFileEqualityTests.TextFileEqual(expectedTestFile1, resultFile1);

            // Check that freqScale.GridLineLocations are correct
            var expectedTestFile2 = new FileInfo("FrequencyScale\\FrequencyDefaultGridLineLocations.EXPECTED.json");
            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultGridLineLocations.ACTUAL.json"));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            TextFileEqualityTests.TextFileEqual(expectedTestFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(256, image.Height);
            Assert.AreEqual(1000, image.Width);
        }

        /// <summary>
        /// METHOD TO CHECK IF SPECIFIED linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void LinearFrequencyScale()
        {
            var recordingPath = @"BAC\BAC2_20071008-085040.wav";
            var outputDir = this.outputDirectory;
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "linearScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);

            // specfied linear scale
            int nyquist = 11025;
            int frameSize = 1024;
            int hertzInterval = 1000;
            var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath, ImageFormat.Png);

            // DO FILE EQUALITY TEST
            var opFileStem = "BAC2_20071008";

            // Check that freqScale.OctaveBinBounds are correct
            var expectedTestFile1 = new FileInfo("FrequencyScale\\FrequencyDefaultOctaveBinBounds.EXPECTED.json");
            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyLinearBinBounds.ACTUAL.json"));
            Json.Serialise(resultFile1, freqScale.OctaveBinBounds);
            TextFileEqualityTests.TextFileEqual(expectedTestFile1, resultFile1);

            // Check that freqScale.GridLineLocations are correct
            var expectedTestFile2 = new FileInfo("FrequencyScale\\FrequencyDefaultGridLineLocations.EXPECTED.json");
            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyGridLineLocations.ACTUAL.json"));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            TextFileEqualityTests.TextFileEqual(expectedTestFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(256, image.Height);
            Assert.AreEqual(1000, image.Width);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale1()
        {
            var recordingPath = @"BAC\BAC2_20071008-085040.wav";
            var outputDir = @"C:\SensorNetworks\SoftwareTests\TestFrequencyScale".ToDirectoryInfo();
            var expectedResultsDir = Path.Combine(outputDir.FullName, TestTools.ExpectedResultsDir).ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "octaveScaleSonogram1.png");

            var recording = new AudioRecording(recordingPath);

            // default octave scale
            var fst = FreqScaleType.Linear125Octaves6Tones30Nyquist11025;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.75,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then conver to octave scale
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath, ImageFormat.Png);

            // DO FILE EQUALITY TEST
            var opFileStem = "BAC2_20071008";

            // Check that freqScale.OctaveBinBounds are correct
            var expectedTestFile1 = new FileInfo("FrequencyScale\\FrequencyDefaultOctaveBinBounds.EXPECTED.json");
            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultOctaveBinBounds.ACTUAL.json"));
            Json.Serialise(resultFile1, freqScale.OctaveBinBounds);
            TextFileEqualityTests.TextFileEqual(expectedTestFile1, resultFile1);

            // Check that freqScale.GridLineLocations are correct
            var expectedTestFile2 = new FileInfo("FrequencyScale\\FrequencyDefaultGridLineLocations.EXPECTED.json");
            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultGridLineLocations.ACTUAL.json"));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            TextFileEqualityTests.TextFileEqual(expectedTestFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(256, image.Height);
            Assert.AreEqual(1000, image.Width);
        }
    }
}
