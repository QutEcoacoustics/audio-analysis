// <copyright file="ExampleTestTemplate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialization format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// </summary>
    [TestClass]
    public class EnvelopeAndFftTests
    {
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
            Assert.AreEqual("00:01:00.2450000", duration.ToString());
            Assert.AreEqual(2594, frameCount);
            Assert.AreEqual(0.880878951426369, fractionOfHighEnergyFrames, 0.000000001);
            int expectedBitsPerSample = 16;
            double expectedEpsilon = Math.Pow(0.5, expectedBitsPerSample - 1);
            Assert.AreEqual(expectedEpsilon, epislon);
            double expectedWindowPower = 203.0778;
            Assert.AreEqual(expectedWindowPower, windowPower, 0.0001);

            // Test sonogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(amplSpectrogram);

            // first write to here and move binary file to resources folder.
            // var sumFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_DataColumnSums.bin");
            // Binary.Serialize(sumFile, columnSums);
            var sumFile = PathHelper.ResolveAsset(@"EnvelopeAndFft\BAC2_20071008-085040_DataColumnSums.bin");
            var expectedColSums = Binary.Deserialize<double[]>(sumFile);
            CollectionAssert.AreEqual(expectedColSums, columnSums);
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
            var averageFile = PathHelper.ResolveAsset(@"EnvelopeAndFft\BAC2_20071008-085040_AvSigArray.bin");
            var expectedAvArray = Binary.Deserialize<double[]>(averageFile);
            CollectionAssert.AreEqual(expectedAvArray, avArray);

            // var envelopeArrayFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_EnvelopeArray.bin");
            // Binary.Serialize(envelopeArrayFile, envelope);
            var envelopeFile = PathHelper.ResolveAsset(@"EnvelopeAndFft\BAC2_20071008-085040_EnvelopeArray.bin");
            var expectedEnvelope = Binary.Deserialize<double[]>(envelopeFile);
            CollectionAssert.AreEqual(expectedEnvelope, envelope);

            // var frameEnergyArrayFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_FrameEnergyArray.bin");
            // Binary.Serialize(frameEnergyArrayFile, frameEnergy);
            var frameEnergyFile = PathHelper.ResolveAsset(@"EnvelopeAndFft\BAC2_20071008-085040_FrameEnergyArray.bin");
            var expectedFrameEnergy = Binary.Deserialize<double[]>(frameEnergyFile);
            CollectionAssert.AreEqual(expectedFrameEnergy, frameEnergy);

            var frameDecibelsArrayFile = new FileInfo(this.outputDirectory + @"\BAC2_20071008-085040_FrameDecibelsArray.bin");
            Binary.Serialize(frameDecibelsArrayFile, frameDecibels);
            var frameDecibelsFile = PathHelper.ResolveAsset(@"EnvelopeAndFft\BAC2_20071008-085040_FrameDecibelsArray.bin");
            var expectedFrameDecibels = Binary.Deserialize<double[]>(frameDecibelsFile);
            CollectionAssert.AreEqual(expectedFrameDecibels, frameDecibels);

            // freq info
            Assert.AreEqual(255, nyquistBin);
            Assert.AreEqual(11025, nyquistFreq);
            Assert.AreEqual(43.0664, freqBinWidth, 0.00001);

            // CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            // CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, new[] { 3, 2, 1 });

            // FileEqualityHelpers.TextFileEqual(new FileInfo("data.txt"), new FileInfo("data.txt"));
            // FileEqualityHelpers.FileEqual(new FileInfo("data.bin"), new FileInfo("data.bin"));

            // output initial data
            // var actualData = new[] { 1, 2, 3 };
            // Json.Serialise("data.json".ToFileInfo(), actualData);
            // Csv.WriteMatrixToCsv("data.csv".ToFileInfo(), actualData);
            // Binary.Serialize("data.bin".ToFileInfo(), actualData);
        }
    }
}
