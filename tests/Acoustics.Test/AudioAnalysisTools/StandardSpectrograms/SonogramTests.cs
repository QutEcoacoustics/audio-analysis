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
    using SixLabors.ImageSharp.PixelFormats;

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
    public class SonogramTests : OutputDirectoryTest
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
        /// The return value should = 96.98816759 dB.
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
            var spectrogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(spectrogram.Data);
            Assert.AreEqual(512, columnSums.Length);
            Assert.AreEqual(39.060622221789323, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(66.607759731106825, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(158.63015599662759, columnSums[127], TestHelper.AllowedDelta);
            Assert.AreEqual(123.03479657693498, columnSums[255], TestHelper.AllowedDelta);
            Assert.AreEqual(1.8051504882600575, columnSums[511], TestHelper.AllowedDelta);
        }

        [TestMethod]
        public void TestDecibelSpectrogram()
        {
            // Produce an ampllitude spectrogram and then convert to decibels
            var spectrogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);
            var decibelMatrix = MFCCStuff.DecibelSpectra(spectrogram.Data, spectrogram.Configuration.WindowPower, spectrogram.SampleRate, spectrogram.Configuration.epsilon);

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(decibelMatrix);
            Assert.AreEqual(512, columnSums.Length);
            Assert.AreEqual(-166818.90211294816, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(-154336.80598725122, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(-146742.38008515659, columnSums[127], TestHelper.AllowedDelta);
            Assert.AreEqual(-150328.6447096896, columnSums[255], TestHelper.AllowedDelta);
            Assert.AreEqual(-215697.79141538762, columnSums[511], TestHelper.AllowedDelta);
        }

        [TestMethod]
        public void SonogramDecibelMethodsAreEquivalent()
        {
            // Method 1
            var sonogram = new AmplitudeSonogram(this.sonoConfig, this.recording.WavReader);
            var expectedDecibelSonogram = MFCCStuff.DecibelSpectra(sonogram.Data, sonogram.Configuration.WindowPower, sonogram.SampleRate, sonogram.Configuration.epsilon);

            // Method 2: make sure that the decibel spectrum is the same no matter which path we take to calculate it.
            var actualDecibelSpectrogram = new SpectrogramStandard(this.sonoConfig, this.recording.WavReader);

            CollectionAssert.That.AreEqual(expectedDecibelSonogram, actualDecibelSpectrogram.Data, TestHelper.AllowedDelta);
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
            image.Save(this.outputDirectory.CombineFile(this.recording.BaseName + ".png"));

            Assert.AreEqual(1621, image.Width);
            Assert.AreEqual(656, image.Height);
        }

        [TestMethod]
        public void TestSonogramHitsOverlay()
        {
            int width = 100;
            int height = 256;

            // make a substitute sonogram image
            var pretendSonogram = new Image<Rgb24>(width, height);

            // make a hits matrix with crossed diagonals
            var hitsMatrix = new int[height, width];
            for (int i = 0; i < height; i++)
            {
                int col = (int)Math.Floor(width * i / (double)height);
                int intensity = col;
                hitsMatrix[i, col] = i;
                hitsMatrix[i, width - col - 1] = i;
            }

            // now add in hits to the spectrogram image.
            if (hitsMatrix != null)
            {
                pretendSonogram = Image_MultiTrack.OverlayScoresAsRedTransparency(pretendSonogram, hitsMatrix);
            }

            //pretendSonogram.Save("C:\\temp\\image.png");
            var pixel = new Argb32(255, 0, 0);
            var expectedColor = new Color(pixel);
            var actualColor = pretendSonogram[0, height - 1];
            Assert.AreEqual<Color>(expectedColor, actualColor);

            pixel = new Argb32(128, 0, 0);
            expectedColor = new Color(pixel);
            actualColor = pretendSonogram[width / 2, height / 2];
            Assert.AreEqual<Color>(expectedColor, actualColor);

            pixel = new Argb32(0, 0, 0);
            expectedColor = new Color(pixel);
            actualColor = pretendSonogram[0, 0];
            Assert.AreEqual<Color>(expectedColor, actualColor);
        }
    }
}