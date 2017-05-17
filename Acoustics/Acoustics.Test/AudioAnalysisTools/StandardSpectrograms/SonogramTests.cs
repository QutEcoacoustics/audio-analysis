// <copyright file="SonogramTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.StandardSpectrograms
{
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
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
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
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        #endregion

        /// <summary>
        /// METHOD TO CHECK IF Standard AMPLITUDE Sonogram IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void TestAmplitudeSonogram()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));

            // specfied linear scale
            var freqScale = new FrequencyScale(nyquist: 11025, frameSize: 1024, herzInterval: 1000);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // DO EQUALITY TEST on the AMPLITUDE SONGOGRAM DATA
            // Do not bother with the image because this is only an amplitude spectrogram.
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var expectedFile = PathHelper.ResolveAsset("StandardSonograms", "BAC2_20071008_AmplSonogramData.EXPECTED.bin");

            // run this once to generate expected test data (and remember to copy out of bin/debug!)
            //Binary.Serialize(expectedFile, sonogram.Data);

            var expected = Binary.Deserialize<double[,]>(expectedFile);

            CollectionAssert.AreEqual(expected, sonogram.Data);
        }

        [TestMethod]
        public void TestDecibelSpectrogram()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));

            // specfied linear scale
            var freqScale = new FrequencyScale(nyquist: 11025, frameSize: 1024, herzInterval: 1000);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // DO EQUALITY TEST on the AMPLITUDE SONGOGRAM DATA
            // Do not bother with the image because this is only an amplitude spectrogram.
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // DO FILE EQUALITY TEST on the DECIBEL SONGOGRAM DATA
            // Do not bother with the image because this has been tested elsewhere.
            var decibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            var expectedFile = PathHelper.ResolveAsset("StandardSonograms", "BAC2_20071008_DecibelSonogramData.EXPECTED.bin");

            // run this once to generate expected test data (and remember to copy out of bin/debug!)
            //Binary.Serialize(expectedFile, decibelSonogram);

            var expected = Binary.Deserialize<double[,]>(expectedFile);

            CollectionAssert.AreEqual(expected, decibelSonogram);
        }

        [TestMethod]
        public void SonogramDecibelMethodsAreEquivalent()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));

            // specfied linear scale
            var freqScale = new FrequencyScale(nyquist: 11025, frameSize: 1024, herzInterval: 1000);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Method 1
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var expectedDecibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            // Method 2: make sure that the decibel spectrum is the same no matter which path we take to calculate it.
            var actualDecibelSpectrogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            CollectionAssert.AreEqual(expectedDecibelSonogram, actualDecibelSpectrogram.Data);
        }
    }
}
