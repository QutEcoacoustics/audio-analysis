// <copyright file="FrequencyScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// </summary>
    [TestClass]
    public class SonogramTests
    {
        private DirectoryInfo outputDirectory;

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
        /// METHOD TO CHECK IF Standard AMPLITUDE Sonogram IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void Sonograms()
        {
            var recordingPath = @"Recordings\BAC2_20071008-085040.wav";
            var opFileStem = "BAC2_20071008";
            var outputDir = this.outputDirectory;

            var recording = new AudioRecording(recordingPath);

            // specfied linear scale
            int nyquist = 11025;
            int frameSize = 1024;
            int hertzInterval = 1000;
            var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // DO FILE EQUALITY TEST on the AMPLITUDE SONGOGRAM DATA
            // Do not bother with the image because this is only an amplitude spectrogram.
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var stemOfExpectedFile = opFileStem + "_AmplSonogramData.EXPECTED.json";
            var stemOfActualFile = opFileStem + "_AmplSonogramData.ACTUAL.json";
            var expectedFile1 = new FileInfo("StandardSonograms\\" + stemOfExpectedFile);
            if (!expectedFile1.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var actualFile1 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(actualFile1, sonogram.Data);
            TextFileEqualityTests.TextFileEqual(expectedFile1, actualFile1);

            // DO FILE EQUALITY TEST on the DECIBEL SONGOGRAM DATA
            // Do not bother with the image because this has been tested elsewhere.
            var decibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            stemOfExpectedFile = opFileStem + "_DecibelSonogramData.EXPECTED.json";
            stemOfActualFile = opFileStem + "_DecibelSonogramData.ACTUAL.json";
            var expectedFile2 = new FileInfo("StandardSonograms\\" + stemOfExpectedFile);
            if (!expectedFile2.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var actualFile2 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(actualFile2, decibelSonogram);
            TextFileEqualityTests.TextFileEqual(expectedFile2, actualFile2);

            // now make sure that the decibel spectrum is the same no matter which path we take to calculate it.
            var sonogram2 = new SpectrogramStandard(sonoConfig, recording.WavReader);
            CollectionAssert.AreEqual(decibelSonogram, sonogram2.Data);
        }
    }
}
