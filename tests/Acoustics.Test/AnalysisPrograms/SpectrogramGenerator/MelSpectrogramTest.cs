// <copyright file="MelSpectrogramTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;

    [TestClass]
    [TestCategory("Spectrograms")]
    public class MelSpectrogramTest
    {
        public const double Delta = 0.000_000_001;
        private readonly AudioRecording recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));
        private DirectoryInfo outputDirectory;

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
        /// Test the output from generating a Mel-frequency Spectrogram.
        /// </summary>
        [TestMethod]
        public void TestMelSpectrogram()
        {
            bool doMelScale = true;
            int filterbankCount = 64;

            //int ccCount = 12;
            //bool includeDelta = true;
            //bool includeDoubleDelta = true;

            var mfccConfig = new MfccConfiguration(doMelScale, filterbankCount, 0, false, false);

            // This constructor initializes default values for Melscale and Mfcc spectrograms and other parameters.
            var config = new SonogramConfig()
            {
                SampleRate = this.recording.WavReader.SampleRate,
                DoPreemphasis = true,
                epsilon = this.recording.Epsilon,
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0,
                Duration = this.recording.Duration,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                mfccConfig = mfccConfig,
            };

            // Get the Mel-frequency Spectrogram
            var melSpectrogram = new SpectrogramMelScale(config, this.recording.WavReader);
            var data = melSpectrogram.Data;

            // DO THE TESTS
            Assert.AreEqual(11025, melSpectrogram.NyquistFrequency);
            Assert.AreEqual(2594, melSpectrogram.FrameCount);
            Assert.AreEqual(2594, data.GetLength(0));

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(64, frBinCount);

            var sumFile = PathHelper.ResolveAsset("SpectrogramTestResults", "BAC2_20071008-085040_MelSpectrogramDataColumnSums_WithPreemphasis.bin");

            // uncomment this to update the binary data. Should be rarely needed
            Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], Delta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, Delta);

            // check that the image is something like you expect.
            var image = melSpectrogram.GetImage();
            image.Save("C:\\temp\\melScaleimage_preemphasis.png");
        }

        /// <summary>
        /// Test the output from generating a cepstrogram.
        /// </summary>
        [TestMethod]
        public void TestCepstrogram()
        {
            bool doMelScale = true;
            int filterbankCount = 64;
            int ccCount = 12;
            bool includeDelta = true;
            bool includeDoubleDelta = true;

            var mfccConfig = new MfccConfiguration(doMelScale, filterbankCount, ccCount, includeDelta, includeDoubleDelta);

            // This constructor initializes default values for Melscale and Mfcc spectrograms and other parameters.
            var config = new SonogramConfig()
            {
                SampleRate = this.recording.WavReader.SampleRate,
                DoPreemphasis = true,
                epsilon = this.recording.Epsilon,
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0,
                Duration = this.recording.Duration,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                mfccConfig = mfccConfig,
            };

            // Get the cepstrogram
            var cepstrogram = new SpectrogramCepstral(config, this.recording.WavReader);
            var data = cepstrogram.Data;

            // DO THE TESTS
            Assert.AreEqual(11025, cepstrogram.NyquistFrequency);
            Assert.AreEqual(2594, cepstrogram.FrameCount);
            Assert.AreEqual(2594, data.GetLength(0));

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(39, frBinCount);

            var sumFile = PathHelper.ResolveAsset("SpectrogramTestResults", "BAC2_20071008-085040_CeptrogramDataColumnSums_WithPreemphasis.bin");

            // uncomment this to update the binary data. Should be rarely needed
            //Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], Delta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, Delta);

            // check that the image is something like you expect.
            var image = cepstrogram.GetImage();
            image.Save("C:\\temp\\image_preemphasis.png");
        }
    }
}
