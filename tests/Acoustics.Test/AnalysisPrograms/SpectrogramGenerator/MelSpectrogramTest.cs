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
        /// Test the output from EnvelopeAndFft.
        /// Only test those variables that are used to construct sonograms
        /// The remaining output variables are tested in TestEnvelopeAndFft2().
        /// </summary>
        [TestMethod]
        public void TestMelSpectrogram()
        {
            bool doPreemphasis = false;

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
                DoPreemphasis = false,
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

            // Now recover the data
            var duration = this.recording.WavReader.Time;
            var sr = this.recording.SampleRate;
            var nyquist = cepstrogram.NyquistFrequency;
            var frameCount = cepstrogram.FrameCount;
            var epislon = cepstrogram.Configuration.epsilon;
            var windowPower = cepstrogram.Configuration.WindowPower;
            var data = cepstrogram.Data;

            //FIX THE BELOWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW
            // DO THE TESTS
            int expectedSR = 22050;
            Assert.AreEqual(expectedSR, sr);
            Assert.AreEqual(60.244535147392290249433106575964M, this.recording.WavReader.ExactDurationSeconds);
            Assert.AreEqual(2594, frameCount);
            int expectedBitsPerSample = 16;
            double expectedEpsilon = Math.Pow(0.5, expectedBitsPerSample - 1);
            Assert.AreEqual(expectedEpsilon, epislon);
            double expectedWindowPower = 203.0778;
            Assert.AreEqual(expectedWindowPower, windowPower, 0.0001);

            // test timeframes and frequency bin

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(data);

            var sumFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_DataColumnSums.bin");

            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], Delta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, Delta);

            //FileTools.WriteArray2File_Formatted(expectedColSums, "C:\\temp\\array.txt", "0.00");
        }
    }
}
