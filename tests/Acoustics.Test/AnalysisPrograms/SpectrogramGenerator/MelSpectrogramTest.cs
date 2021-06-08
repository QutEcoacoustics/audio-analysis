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

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(64, frBinCount);
            Assert.AreEqual(48.648309222211026, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(93.49997884080535, columnSums[1], TestHelper.AllowedDelta);
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

            // check that the image is what you expect.
            var image = cepstrogram.GetImage();
            this.SaveTestOutput(outputDirectory => BaseSonogram.SaveDebugSpectrogram(image, outputDirectory, "mfccimage_WithPreemphasisTEST"));

            // DO THE TESTS
            Assert.AreEqual(11025, cepstrogram.NyquistFrequency);
            Assert.AreEqual(3012, cepstrogram.FrameCount);
            Assert.AreEqual(3012, data.GetLength(0));

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(39, frBinCount);
            Assert.AreEqual(851.9554620620411, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(1381.875961657845, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(1938.8434781109502, columnSums[12], TestHelper.AllowedDelta);
            Assert.AreEqual(1505.9283619924238, columnSums[13], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.07944927851, columnSums[14], TestHelper.AllowedDelta);
            Assert.AreEqual(1505.9960186256506, columnSums[25], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0307897275595, columnSums[26], TestHelper.AllowedDelta);
            Assert.AreEqual(1505.9953976547922, columnSums[27], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0039343217934, columnSums[38], TestHelper.AllowedDelta);
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

            // Calculate the full range of MFCC coefficients ie including decibel and deltas, etc
            // normalise the array of frame log-energy values. These will later be added into the mfcc feature vectors.
            var frameLogEnergyNormed = DataTools.normalise(melSpectrogram.DecibelsPerFrame);
            Assert.AreEqual(3012, frameLogEnergyNormed.Length);
            cepM = MFCCStuff.AcousticVectors(cepM, frameLogEnergyNormed, includeDelta, includeDoubleDelta);
            Assert.AreEqual(26, cepM.GetLength(1));

            melSpectrogram.Data = cepM;

            // check that the image is what you expect.
            var image = melSpectrogram.GetImage();
            this.SaveTestOutput(outputDirectory => BaseSonogram.SaveDebugSpectrogram(image, outputDirectory, "mfccimage_no-preemphasisTEST1"));

            // Test cepstrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(cepM);
            int frBinCount = columnSums.Length;
            Assert.AreEqual(26, frBinCount);
            Assert.AreEqual(733.5221023203834, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(1874.8811842724304, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(1389.16766148194, columnSums[12], TestHelper.AllowedDelta);
            Assert.AreEqual(1505.9720853599465, columnSums[13], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0965820877702, columnSums[14], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0089293490105, columnSums[25], TestHelper.AllowedDelta);

            // Do the test again but start directly with a cepstrogram.
            //The test results should be same as above.
            var cepstrogram = new SpectrogramCepstral(config, this.recording.WavReader);
            var data = cepstrogram.Data;

            // check that the image is what you expect.
            image = melSpectrogram.GetImage();
            this.SaveTestOutput(outputDirectory => BaseSonogram.SaveDebugSpectrogram(image, outputDirectory, "mfccimage_no-preemphasisTEST2"));

            // DO TESTS on CEPSTROGRAM
            Assert.AreEqual(3012, cepstrogram.FrameCount);
            Assert.AreEqual(3012, data.GetLength(0));
            Assert.AreEqual(26, data.GetLength(1));

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums2 = MatrixTools.SumColumns(cepstrogram.Data);
            Assert.AreEqual(26, columnSums2.Length);
            Assert.AreEqual(733.5221023203834, columnSums2[0], TestHelper.AllowedDelta);
            Assert.AreEqual(1874.8811842724304, columnSums2[1], TestHelper.AllowedDelta);
            Assert.AreEqual(1389.16766148194, columnSums2[12], TestHelper.AllowedDelta);
            Assert.AreEqual(1505.9720853599465, columnSums2[13], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0965820877702, columnSums2[14], TestHelper.AllowedDelta);
            Assert.AreEqual(1506.0089293490105, columnSums2[25], TestHelper.AllowedDelta);
        }
    }
}
