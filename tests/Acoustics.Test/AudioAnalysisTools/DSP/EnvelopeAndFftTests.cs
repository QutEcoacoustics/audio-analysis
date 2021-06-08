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

            // set default value for pre-emphasis.
            // set true ony when dealing with human speech.
            bool doPreemphasis = false;

            // window overlap is used only for spectrograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                doPreemphasis,
                windowSize,
                windowOverlap,
                windowFunction);

            // Now recover the data
            // The following data is required when constructing spectrograms
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

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(amplSpectrogram);
            Assert.AreEqual(256, columnSums.Length);
            Assert.AreEqual(105.42693858247799, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(160.10428474265564, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(179.50923510429988, columnSums[63], TestHelper.AllowedDelta);
            Assert.AreEqual(137.86815307838563, columnSums[127], TestHelper.AllowedDelta);
            Assert.AreEqual(6.5980770405964346, columnSums[255], TestHelper.AllowedDelta);
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

            // set default value for pre-emphasis.
            // set true ony when dealing with human speech.
            bool doPreemphasis = false;

            // window overlap is used only for spectrograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                doPreemphasis,
                windowSize,
                windowOverlap,
                windowFunction);

            // Now recover the data

            /*
            // The following data is required when constructing spectrograms
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

            // freq info
            Assert.AreEqual(255, nyquistBin);
            Assert.AreEqual(11025, nyquistFreq);
            Assert.AreEqual(43.0664, freqBinWidth, 0.00001);

            // DO THE TEST of the array of average frame amplitude
            Assert.AreEqual(2594, avArray.Length);
            Assert.AreEqual(0.0069539881015961051, avArray[0], TestHelper.AllowedDelta);
            Assert.AreEqual(0.0051731257820367832, avArray[1], TestHelper.AllowedDelta);
            Assert.AreEqual(0.010161590739158267, avArray[500], TestHelper.AllowedDelta);
            Assert.AreEqual(0.0032369886242255858, avArray[2593], TestHelper.AllowedDelta);

            // DO THE TEST of the array of frame envelope
            Assert.AreEqual(2594, envelope.Length);
            Assert.AreEqual(0.027222510452589496, envelope[0], TestHelper.AllowedDelta);
            Assert.AreEqual(0.020416882839442121, envelope[1], TestHelper.AllowedDelta);
            Assert.AreEqual(0.037781914731284526, envelope[500], TestHelper.AllowedDelta);
            //Assert.AreEqual(105.42693858247799, envelope[2593], TestHelper.AllowedDelta);

            // DO THE TEST of the array of frame energies
            Assert.AreEqual(2594, frameEnergy.Length);
            Assert.AreEqual(7.1671891187912771E-05, frameEnergy[0], TestHelper.AllowedDelta);
            Assert.AreEqual(4.1982822382603658E-05, frameEnergy[1], TestHelper.AllowedDelta);
            Assert.AreEqual(0.0001698774541264456, frameEnergy[500], TestHelper.AllowedDelta);
            //Assert.AreEqual(105.42693858247799, frameEnergy[2593], TestHelper.AllowedDelta);

            // DO THE TEST of the array of frame decibels
            Assert.AreEqual(2594, frameDecibels.Length);
            Assert.AreEqual(-41.446511357615393, frameDecibels[0], TestHelper.AllowedDelta);
            Assert.AreEqual(-43.769283684218408, frameDecibels[1], TestHelper.AllowedDelta);
            Assert.AreEqual(-37.69864256199849, frameDecibels[500], TestHelper.AllowedDelta);
            Assert.AreEqual(-48.023284472990866, frameDecibels[2593], TestHelper.AllowedDelta);
        }

        /// <summary>
        /// Test the output from EnvelopeAndFft.
        /// This is the same as TestEnvelopeAndFft1() EXCEPT that Pre-empahsis is set true.
        /// Pre-empahsis is set true only when dealing speech. Never when calculating acoustic indices.
        /// Only test those variables that are used to construct sonograms.
        /// </summary>
        [TestMethod]
        public void TestEnvelopeAndFft3()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));
            int windowSize = 512;
            bool doPreemphasis = true;
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                doPreemphasis,
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

            // Test spectrogram data matrix by comparing the vector of column sums.
            double[] columnSums = MatrixTools.SumColumns(amplSpectrogram);
            Assert.AreEqual(10.232929387353428, columnSums[0], TestHelper.AllowedDelta);
            Assert.AreEqual(10.246512198651626, columnSums[1], TestHelper.AllowedDelta);
            Assert.AreEqual(135.34785008733473, columnSums[63], TestHelper.AllowedDelta);
            Assert.AreEqual(191.22175680642957, columnSums[127], TestHelper.AllowedDelta);
            Assert.AreEqual(12.66971600559739, columnSums[255], TestHelper.AllowedDelta);

            // The effect of pre-emphasis on the first 10 column sums.
            //ID   -preE  +preE
            // 0  105.43  10.23
            // 1  160.10  10.25
            // 2  220.47  12.96
            // 3  265.59  17.49
            // 4  319.03  24.20
            // 5  410.82  35.50
            // 6  432.17  40.17
            // 7  353.41  34.89
            // 8  221.72  24.35
            // 9  162.78  21.10
        }
    }
}