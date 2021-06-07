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
    public class MelSpectrogramTest : OutputDirectoryTest
    {
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
            int windowSize = 512;

            // The following frame step yields 50 frames/s which can make spectrogram interpretation easier.
            int windowStep = 441;

            // must set the window overlap as this is actually used.
            double windowOverlap = (windowSize - windowStep) / (double)windowSize;

            bool doMelScale = true;
            int filterbankCount = 64;

            // the following are not required - they are only for cepstral coefficients.
            //int ccCount = 12; bool includeDelta = true; bool includeDoubleDelta = true;
            var mfccConfig = new MfccConfiguration(doMelScale, filterbankCount, 0, false, false);

            // This constructor initializes default values for Melscale spectrograms.
            var config = new SonogramConfig()
            {
                SampleRate = this.recording.WavReader.SampleRate,
                DoPreemphasis = true,
                epsilon = this.recording.Epsilon,
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = windowOverlap,
                Duration = this.recording.Duration,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                mfccConfig = mfccConfig,
            };

            // Get the Mel-frequency Spectrogram
            var melSpectrogram = new SpectrogramMelScale(config, this.recording.WavReader);
            var data = melSpectrogram.Data;
            data = DataTools.normalise(data);

            // check that the image is what you expect.
            var image = melSpectrogram.GetImage();
            this.SaveTestOutput(outputDirectory => BaseSonogram.SaveDebugSpectrogram(image, outputDirectory, "melScaleImage_preemphasis_TEST"));

            // DO THE TESTS
            Assert.AreEqual(11025, melSpectrogram.NyquistFrequency);
            Assert.AreEqual(3012, melSpectrogram.FrameCount);
            Assert.AreEqual(3012, data.GetLength(0));

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(64, frBinCount);
            Assert.AreEqual(48.648309222211026, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(50.465126583997368, columnSums[15], TestHelper.AllowedDelta);
            Assert.AreEqual(118.8046866543946, columnSums[31], TestHelper.AllowedDelta);
            Assert.AreEqual(7.5001092063873793, columnSums[63], TestHelper.AllowedDelta);
        }

        /// <summary>
        /// Test the output from generating a cepstrogram.
        /// Delta and DoubleDelta both true.
        /// </summary>
        [TestMethod]
        public void TestCepstrogram()
        {
            int windowSize = 512;

            // The following frame step yields 50 frames/s which can make spectrogram interpretation easier.
            int windowStep = 441;

            // must set the window overlap as this is actually used.
            double windowOverlap = (windowSize - windowStep) / (double)windowSize;

            bool doMelScale = true;
            int filterbankCount = 64;
            int ccCount = 12;
            bool includeDelta = true;
            bool includeDoubleDelta = true;

            var mfccConfig = new MfccConfiguration(doMelScale, filterbankCount, ccCount, includeDelta, includeDoubleDelta);

            // This constructor initializes default values for Mfcc spectrograms.
            var config = new SonogramConfig()
            {
                SampleRate = this.recording.WavReader.SampleRate,
                DoPreemphasis = true,
                epsilon = this.recording.Epsilon,
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = windowOverlap,
                Duration = this.recording.Duration,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
                mfccConfig = mfccConfig,
            };

            // Get the cepstrogram
            var cepstrogram = new SpectrogramCepstral(config, this.recording.WavReader);
            var data = cepstrogram.Data;

            // DO THE TESTS
            Assert.AreEqual(11025, cepstrogram.NyquistFrequency);
            Assert.AreEqual(3012, cepstrogram.FrameCount);
            Assert.AreEqual(3012, data.GetLength(0));

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(39, frBinCount);

            // check that the image is something like you expect.
            //var image = cepstrogram.GetImage();
            //image.Save("C:\\temp\\mfccimage_WithPreemphasisTEST.png");

            var sumFile = PathHelper.ResolveAsset("SpectrogramTestResults", "BAC2_20071008-085040_CeptrogramDataColumnSums_WithPreemphasis.bin");

            // uncomment this to update the binary data. Should be rarely needed
            //Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], TestHelper.AllowedDelta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, TestHelper.AllowedDelta);
        }

        /// <summary>
        /// This test first generates a mel-scale spectrogram and then goes through
        /// the individual steps to create a cepstrogram, testing the intermediate data structures on the way.
        /// Also do not do pre-emphasis, nor double-deltas.
        /// </summary>
        [TestMethod]
        public void TestCepstrogramMinusDoubleDeltas()
        {
            int windowSize = 512;

            // The following frame step yields 50 frames/s which can make spectrogram interpretation easier.
            int windowStep = 441;

            // must set the window overlap as this is actually used.
            double windowOverlap = (windowSize - windowStep) / (double)windowSize;

            bool doMelScale = false;
            int filterbankCount = 32;
            int ccCount = 12;
            bool includeDelta = true;
            bool includeDoubleDelta = false;

            var mfccConfig = new MfccConfiguration(doMelScale, filterbankCount, ccCount, includeDelta, includeDoubleDelta);

            // This constructor initializes default values for Melscale and Mfcc spectrograms and other parameters.
            var config = new SonogramConfig()
            {
                SampleRate = this.recording.WavReader.SampleRate,
                DoPreemphasis = false,
                epsilon = this.recording.Epsilon,
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = windowOverlap,
                Duration = this.recording.Duration,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
                mfccConfig = mfccConfig,
            };

            // Get the mel Spectrogram
            var melSpectrogram = new SpectrogramMelScale(config, this.recording.WavReader);

            // Convert the decibel values to log-energy values
            var melM = MatrixTools.MultiplyMatrixByFactor(melSpectrogram.Data, 0.1);

            Assert.AreEqual(3012, melM.GetLength(0));
            Assert.AreEqual(32, melM.GetLength(1));

            //normalise log values and square. Supposed to help.
            melM = MatrixTools.NormaliseMatrixValues(melM);
            melM = MatrixTools.SquareValues(melM);

            // Calculate cepstral coefficients
            var cepM = MFCCStuff.Cepstra(melM, ccCount);
            cepM = MatrixTools.NormaliseMatrixValues(cepM);

            Assert.AreEqual(3012, cepM.GetLength(0));
            Assert.AreEqual(12, cepM.GetLength(1));

            // (vii) Calculate the full range of MFCC coefficients ie including decibel and deltas, etc
            // normalise the array of frame log-energy values. These will later be added into the mfcc feature vectors.
            var frameLogEnergyNormed = DataTools.normalise(melSpectrogram.DecibelsPerFrame);
            Assert.AreEqual(3012, frameLogEnergyNormed.Length);

            cepM = MFCCStuff.AcousticVectors(cepM, frameLogEnergyNormed, includeDelta, includeDoubleDelta);
            Assert.AreEqual(26, cepM.GetLength(1));

            melSpectrogram.Data = cepM;
            //var image1 = melSpectrogram.GetImage();
            //image1.Save("C:\\temp\\mfccimage_no-preemphasisTEST1.png");

          // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(cepM);

            var sumFile = PathHelper.ResolveAsset("SpectrogramTestResults", "BAC2_20071008-085040_CeptrogramDataColumnSums_WithoutPreemphasis.bin");

            // uncomment this to update the binary data. Should be rarely needed
            //Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], TestHelper.AllowedDelta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, TestHelper.AllowedDelta);

            // Do the test again but going directly to a cepstrogram.
            var cepstrogram = new SpectrogramCepstral(config, this.recording.WavReader);
            var data = cepstrogram.Data;

            // check that the image is something like you expect.
            //var image = cepstrogram.GetImage();
            //image.Save("C:\\temp\\mfccimage_no-preemphasisTEST2.png");

            // DO THE TESTS
            Assert.AreEqual(3012, cepstrogram.FrameCount);
            Assert.AreEqual(3012, data.GetLength(0));
            Assert.AreEqual(26, data.GetLength(1));
            double[] columnSums2 = MatrixTools.SumColumns(data);
            Assert.AreEqual(expectedColSums[0], columnSums[0], TestHelper.AllowedDelta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums2, TestHelper.AllowedDelta);
        }
    }
}
