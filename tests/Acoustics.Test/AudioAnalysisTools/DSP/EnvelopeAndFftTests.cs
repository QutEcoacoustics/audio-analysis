// <copyright file="EnvelopeAndFftTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnvelopeAndFftTests
    {
        private DirectoryInfo outputDirectory;
        public const double Delta = 0.000_000_001;

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
        public void TestEnvelopeAndFft1()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));
            int windowSize = 512;

            // window overlap is used only for sonograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                windowSize,
                windowOverlap,
                windowFunction);

            // Now recover the data
            // The following data is required when constructing sonograms
            var duration = recording.WavReader.Time;
            var sr = recording.SampleRate;
            var frameCount = fftdata.FrameCount;
            var fractionOfHighEnergyFrames = fftdata.FractionOfHighEnergyFrames;
            var epislon = fftdata.Epsilon;
            var windowPower = fftdata.WindowPower;
            var amplSpectrogram = fftdata.AmplitudeSpectrogram;

            // The below info is only used when calculating spectral and summary indices
            /*
            // energy level information
            int clipCount = fftdata.ClipCount;
            int maxAmpCount = fftdata.MaxAmplitudeCount;
            double maxSig = fftdata.MaxSignalValue;
            double minSig = fftdata.MinSignalValue;

            // envelope info
            var avArray = fftdata.Average;
            var envelope = fftdata.Envelope;
            var frameEnergy = fftdata.FrameEnergy;
            var frameDecibels = fftdata.FrameDecibels;

            // freq scale info
            var nyquistBin = fftdata.NyquistBin;
            var nyquistFreq = fftdata.NyquistFreq;
            var freqBinWidth = fftdata.FreqBinWidth;
            */

            // DO THE TESTS
            int expectedSR = 22050;
            Assert.AreEqual(expectedSR, sr);
            Assert.AreEqual(60.244535147392290249433106575964M, recording.WavReader.ExactDurationSeconds);
            Assert.AreEqual(2594, frameCount);
            int expectedBitsPerSample = 16;
            double expectedEpsilon = Math.Pow(0.5, expectedBitsPerSample - 1);
            Assert.AreEqual(expectedEpsilon, epislon);
            double expectedWindowPower = 203.0778;
            Assert.AreEqual(expectedWindowPower, windowPower, 0.0001);
            Assert.AreEqual(0.0, fractionOfHighEnergyFrames, 0.0000001);

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(amplSpectrogram);

            var sumFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_DataColumnSums.bin");

            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(sumFile, columnSums);

            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            var totalDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Sum();
            var avgDelta = expectedColSums.Zip(columnSums, ValueTuple.Create).Select(x => Math.Abs(x.Item1 - x.Item2)).Average();
            Assert.AreEqual(expectedColSums[0], columnSums[0], Delta, $"\nE: {expectedColSums[0]:R}\nA: {columnSums[0]:R}\nD: {expectedColSums[0] - columnSums[0]:R}\nT: {totalDelta:R}\nA: {avgDelta}\nn: {expectedColSums.Length}");
            CollectionAssert.That.AreEqual(expectedColSums, columnSums, Delta);
        }

        /// <summary>
        /// Test the output from EnvelopeAndFft.
        /// Only test those variables that are used to calculate spectral and summary indices
        /// The other output variables are tested in TestEnvelopeAndFft1().
        /// </summary>
        [TestMethod]
        public void TestEnvelopeAndFft2()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));
            int windowSize = 512;

            // window overlap is used only for sonograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                windowSize,
                windowOverlap,
                windowFunction);

            // Now recover the data

            /*
            // The following data is required when constructing sonograms
            var duration = recording.WavReader.Time;
            var sr = recording.SampleRate;
            var frameCount = fftdata.FrameCount;
            var fractionOfHighEnergyFrames = fftdata.FractionOfHighEnergyFrames;
            var epislon = fftdata.Epsilon;
            var windowPower = fftdata.WindowPower;
            var amplSpectrogram = fftdata.AmplitudeSpectrogram;
            */

            // The below info is only used when calculating spectral and summary indices
            // energy level information
            int clipCount = fftdata.ClipCount;
            int maxAmpCount = fftdata.HighAmplitudeCount;
            double maxSig = fftdata.MaxSignalValue;
            double minSig = fftdata.MinSignalValue;

            // envelope info
            var avArray = fftdata.Average;
            var envelope = fftdata.Envelope;
            var frameEnergy = fftdata.FrameEnergy;
            var frameDecibels = fftdata.FrameDecibels;

            // freq scale info
            var nyquistBin = fftdata.NyquistBin;
            var nyquistFreq = fftdata.NyquistFreq;
            var freqBinWidth = fftdata.FreqBinWidth;

            // DO THE TESTS of clipping and signal level info
            // energy level information
            Assert.AreEqual(0, clipCount);
            Assert.AreEqual(0, maxAmpCount);
            Assert.AreEqual(-0.250434888760033, minSig, 0.000001);
            Assert.AreEqual(0.255165257728813, maxSig, 0.000001);

            // DO THE TESTS of energy array info

            // first write to here and move binary file to resources folder.
            // var averageArrayFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_AvSigArray.bin");
            // Binary.Serialize(averageArrayFile, avArray);
            var averageFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_AvSigArray.bin");
            var expectedAvArray = Binary.Deserialize<double[]>(averageFile);
            CollectionAssert.AreEqual(expectedAvArray, avArray);

            // var envelopeArrayFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_EnvelopeArray.bin");
            // Binary.Serialize(envelopeArrayFile, envelope);
            var envelopeFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_EnvelopeArray.bin");
            var expectedEnvelope = Binary.Deserialize<double[]>(envelopeFile);
            CollectionAssert.AreEqual(expectedEnvelope, envelope);

            var frameEnergyFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_FrameEnergyArray.bin");

            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(frameEnergyFile, frameEnergy);

            var expectedFrameEnergy = Binary.Deserialize<double[]>(frameEnergyFile);
            CollectionAssert.AreEqual(expectedFrameEnergy, frameEnergy);

            var frameDecibelsFile = PathHelper.ResolveAsset("EnvelopeAndFft", "BAC2_20071008-085040_FrameDecibelsArray.bin");

            // uncomment this to update the binary data. Should be rarely needed
            // AT: Updated 2017-02-15 because FFT library changed in 864f7a491e2ea0e938161bd390c1c931ecbdf63c
            //Binary.Serialize(frameDecibelsFile, frameDecibels);

            var expectedFrameDecibels = Binary.Deserialize<double[]>(frameDecibelsFile);
            CollectionAssert.That.AreEqual(expectedFrameDecibels, frameDecibels, Delta);

            // freq info
            Assert.AreEqual(255, nyquistBin);
            Assert.AreEqual(11025, nyquistFreq);
            Assert.AreEqual(43.0664, freqBinWidth, 0.00001);
        }
    }
}