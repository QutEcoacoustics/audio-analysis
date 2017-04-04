namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
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

        [TestMethod]
        public void LinearFrequencyScaleDefault()
        {
            var recordingPath = @"BAC\BAC2_20071008-085040.wav";
            var outputDir = this.outputDirectory;
            var expectedResultsDir = Path.Combine(outputDir.FullName, "ExpectedTestResults").ToDirectoryInfo();
            var outputImagePath = Path.Combine(outputDir.FullName, "linearScaleSonogram_default.png");
            var opFileStem = "BAC2_20071008";

            var recording = new AudioRecording(recordingPath);

            // default linear scale
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,

                //NoiseReductionType = NoiseReductionType.Standard,
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
            var expectedTestFile = new FileInfo("FrequencyScale\\FrequencyDefaultScaleTest.EXPECTED.json");
            var resultFile = new FileInfo(Path.Combine(outputDir.FullName, opFileStem + "FrequencyDefaultScaleTest.ACTUAL.json"));
            Json.Serialise(resultFile, freqScale.GridLineLocations);

            // Check that freqScale.GridLineLocations are correct
            TextFileEqualityTests.TextFileEqual(expectedTestFile, resultFile);

            // Check that image dimensions are correct
            Assert.AreEqual(256, image.Height);
            Assert.AreEqual(1000, image.Width);
        }
    }
}
