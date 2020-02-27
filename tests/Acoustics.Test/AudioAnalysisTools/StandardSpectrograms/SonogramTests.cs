// <copyright file="SonogramTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.AudioAnalysisTools.DSP;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using Path = System.IO.Path;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialization format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710.
    /// </summary>
    [TestClass]
    public class SonogramTests
    {
        private const double AllowedDelta = 0.000001;
        private DirectoryInfo outputDirectory;
        private AudioRecording recording;
        private FrequencyScale freqScale;
        private SonogramConfig sonoConfig;

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
            this.recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));

            // specified linear scale
            this.freqScale = new FrequencyScale(nyquist: 11025, frameSize: 1024, hertzGridInterval: 1000);

            // set up the config for each spectrogram
            this.sonoConfig = new SonogramConfig
            {
                WindowSize = this.freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = this.recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
            this.recording.Dispose();

            //this.freqScale.();
            //this.sonoConfig.Dispose();
        }

        /// <summary>
        /// METHOD TO CHECK that averaging of decibel values is working.
        /// var array = new[] { 96.0, 100.0, 90.0, 97.0 };
        /// The return value should = 96.98816759 dB
        /// </summary>
        [TestMethod]
        public void TestAverageOfDecibelValues()
        {
            var decibelArray1 = new[] { 96.0, 100.0, 90.0, 97.0 };
            var decibelArray2 = new[] { -96.0, -100.0, -90.0, -97.0 };

            // run this once to generate expected test data
            // uncomment this to update the binary data. Should be rarely needed

            var average = SpectrogramTools.AverageAnArrayOfDecibelValues(decibelArray1);
            Assert.AreEqual(96.98816759, average, AllowedDelta);
            average = SpectrogramTools.AverageAnArrayOfDecibelValues(decibelArray2);
            Assert.AreEqual(-94.11528038, average, AllowedDelta);
        }

        /// <summary>
        /// METHOD TO CHECK IF Standard AMPLITUDE Sonogram IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void TestAmplitudeSonogram()
        {
            // DO EQUALITY TEST on the AMPLITUDE SONGOGRAM DATA
            // Do not bother with the image because this is only an amplitude spectrogram.
            var sonogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);
            var expectedFile = PathHelper.ResolveAsset("StandardSonograms", "BAC2_20071008_AmplSonogramData.EXPECTED.bin");

            // run this once to generate expected test data
            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(expectedFile, sonogram.Data);

            var expected = Binary.Deserialize<double[,]>(expectedFile);

            CollectionAssert.That.AreEqual(expected, sonogram.Data, EnvelopeAndFftTests.Delta);
        }

        [TestMethod]
        public void TestDecibelSpectrogram()
        {
            // DO EQUALITY TEST on the AMPLITUDE SONGOGRAM DATA
            // Do not bother with the image because this is only an amplitude spectrogram.
            var sonogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);

            // DO FILE EQUALITY TEST on the DECIBEL SONGOGRAM DATA
            // Do not bother with the image because this has been tested elsewhere.
            var decibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            var expectedFile = PathHelper.ResolveAsset("StandardSonograms", "BAC2_20071008_DecibelSonogramData.EXPECTED.bin");

            // run this once to generate expected test data
            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(expectedFile, decibelSonogram);

            var expected = Binary.Deserialize<double[,]>(expectedFile);

            CollectionAssert.That.AreEqual(expected, decibelSonogram, EnvelopeAndFftTests.Delta);
        }

        [TestMethod]
        public void SonogramDecibelMethodsAreEquivalent()
        {
            // Method 1
            var sonogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);
            var expectedDecibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            // Method 2: make sure that the decibel spectrum is the same no matter which path we take to calculate it.
            var actualDecibelSpectrogram = new SpectrogramStandard(this.sonoConfig, this.recording.WavReader);

            CollectionAssert.That.AreEqual(expectedDecibelSonogram, actualDecibelSpectrogram.Data, EnvelopeAndFftTests.Delta);
        }

        [TestMethod]
        public void TestAnnotatedSonogramWithPlots()
        {
            // Make a decibel spectrogram
            var actualDecibelSpectrogram = new SpectrogramStandard(this.sonoConfig, this.recording.WavReader);

            // prepare normalisation bounds for three plots
            double minDecibels = -100.0;
            double maxDecibels = -50;

            //double decibelThreshold = 12.5 dB above -100 dB;
            var normThreshold = 0.25;

            //plot 1
            int minHz = 2000;
            int maxHz = 3000;
            var decibelArray = SNR.CalculateFreqBandAvIntensity(actualDecibelSpectrogram.Data, minHz, maxHz, actualDecibelSpectrogram.NyquistFrequency);
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, minDecibels, maxDecibels);
            var plot1 = new Plot("Intensity 2-3 kHz", normalisedIntensityArray, normThreshold);

            //plot 2
            minHz = 3000;
            maxHz = 4000;
            decibelArray = SNR.CalculateFreqBandAvIntensity(actualDecibelSpectrogram.Data, minHz, maxHz, actualDecibelSpectrogram.NyquistFrequency);
            normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, minDecibels, maxDecibels);
            var plot2 = new Plot("Intensity 3-4 kHz", normalisedIntensityArray, normThreshold);

            //plot 3
            minHz = 4000;
            maxHz = 5000;
            decibelArray = SNR.CalculateFreqBandAvIntensity(actualDecibelSpectrogram.Data, minHz, maxHz, actualDecibelSpectrogram.NyquistFrequency);
            normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, minDecibels, maxDecibels);
            var plot3 = new Plot("Intensity 4-5 kHz", normalisedIntensityArray, normThreshold);

            // combine the plots
            var plots = new List<Plot> { plot1, plot2, plot3 };

            // create three events
            var startOffset = TimeSpan.Zero;
            var events = new List<AcousticEvent>
            {
                new AcousticEvent(startOffset, 10.0, 10.0, 2000, 3000),
                new AcousticEvent(startOffset, 25.0, 10.0, 3000, 4000),
                new AcousticEvent(startOffset, 40.0, 10.0, 4000, 5000),
            };

            var image = SpectrogramTools.GetSonogramPlusCharts(actualDecibelSpectrogram, events, plots, null);

            // create the image for visual confirmation
            image.Save(Path.Combine(this.outputDirectory.FullName, this.recording.BaseName + ".png"));

            Assert.AreEqual(1621, image.Width);
            Assert.AreEqual(647, image.Height);
        }
    }
}
