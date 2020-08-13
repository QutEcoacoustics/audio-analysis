// <copyright file="FrequencyScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using static global::TowseyLibrary.FFT;
    using Path = System.IO.Path;

    /// <summary>
    /// Test methods for the various Frequency Scales
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them.
    /// </summary>
    [TestClass]
    public class FrequencyScaleTests
    {
        private DirectoryInfo outputDirectory;

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

        /// <summary>
        /// METHOD TO CHECK IF Default linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void LinearFrequencyScaleDefault()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var outputImagePath = this.outputDirectory.CombineFile("DefaultLinearScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);

            // default linear scale
            var fst = FreqScaleType.Linear;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst, freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 23, 1000 },
                { 46, 2000 },
                { 69, 3000 },
                { 92, 4000 },
                { 116, 5000 },
                { 139, 6000 },
                { 162, 7000 },
                { 185, 8000 },
                { 208, 9000 },
                { 232, 10000 },
                { 255, 11000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(310, image.Height);
            Assert.AreEqual(3247, image.Width);
        }

        /// <summary>
        /// METHOD TO CHECK IF SPECIFIED linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void LinearFrequencyScale()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var outputImagePath = this.outputDirectory.CombineFile("DefaultLinearScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);

            // specfied linear scale
            int nyquist = 11025;
            int frameSize = 1024;
            int hertzInterval = 1000;
            var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);
            var fst = freqScale.ScaleType;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.FinalBinCount * 2,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst, freqScale.GridLineLocations);
            image.Save(outputImagePath);

            var expected = new[,]
            {
                { 46, 1000 },
                { 92, 2000 },
                { 139, 3000 },
                { 185, 4000 },
                { 232, 5000 },
                { 278, 6000 },
                { 325, 7000 },
                { 371, 8000 },
                { 417, 9000 },
                { 464, 10000 },
                { 510, 11000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(566, image.Height);
            Assert.AreEqual(1621, image.Width);
        }

        /// <summary>
        /// Test of the default Mel FREQ SCALE
        /// Check it on pure tone spectrum.
        /// By default, the split between linear and log is at 1000 Hz.
        /// NOTE: This mel frequency scale class is not actually used to produce Mel scale spectrograms.
        ///         Currently, the creation of mel scale spectrograms bypasses use of this FrequencyScale class.
        /// </summary>
        [TestMethod]
        public void TestMelFrequencyScale()
        {
            var fst = FreqScaleType.Mel;
            var freqScale = new FrequencyScale(fst);

            // test contents of the bin bounds matrix.
            Assert.AreEqual(1000, freqScale.LinearBound);

            // test contents of the octave bin bounds matrix.
            int[,] melBinBounds = freqScale.BinBounds;
            Assert.AreEqual(64, melBinBounds.GetLength(0));
            Assert.AreEqual(1922, melBinBounds[30, 1]);
            Assert.AreEqual(2164, melBinBounds[32, 1]);
            Assert.AreEqual(10033, melBinBounds[62, 1]);
            Assert.AreEqual(10516, melBinBounds[63, 1]);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 20, 1000 },
                { 30, 2000 },
                { 37, 3000 },
                { 43, 4000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);
        }

        /// <summary>
        /// Test making a me-frequency spectrogram using an artificial amplitude spectrogram as input.
        /// NOTE: This method bypasses the use of the FrequencyScale class.
        /// By default, the Mel scale used here is linear to 1000 Hz.
        /// </summary>
        [TestMethod]
        public void TestMakeMelScaleSpectrogram()
        {
            int sampleRate = 22050;
            int windowSize = 512;
            int defaultMelBinCount = 64;
            var recordingBitsPerSample = 16;
            var epsilon = Math.Pow(0.5, recordingBitsPerSample - 1);

            // make fft class using rectangular window - just to get a value for Window power.
            var fft = new FFT(windowSize);

            var config = new SonogramConfig
            {
                WindowSize = windowSize,
                WindowOverlap = 0.0,
                SourceFName = "Dummy",
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
                epsilon = epsilon,
                WindowPower = fft.WindowPower,
            };

            // make a dummy spectrogram
            int frameCount = 100;
            int binCount = windowSize / 2;
            double[,] matrix = new double[frameCount, binCount];
            for (int i = 0; i < 100; i++)
            {
                matrix[i, 0] = 1.0;
                matrix[i, 128] = 1.0;
                matrix[i, 255] = 1.0;
            }

            var spectrogramMatrix = SpectrogramMelScale.MakeMelScaleSpectrogram(config, matrix, sampleRate);

            //Assert.That.MatricesAreEqual(expected, actual);
            Assert.AreEqual(frameCount, spectrogramMatrix.GetLength(0));
            Assert.AreEqual(defaultMelBinCount, spectrogramMatrix.GetLength(1));

            Assert.AreEqual(-73.784157442630288, spectrogramMatrix[0, 0], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 1], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 32], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 62], 0.0001);
            Assert.AreEqual(-98.078506874942121, spectrogramMatrix[0, 63], 0.0001);
        }

        /// <summary>
        /// Test static method which returns bin index for a given frequency.
        /// </summary>
        [TestMethod]
        public void TestAssignmentOfGridLinesForOctaveFrequencyScale()
        {
            int nyquist = 11025;
            int linearBound = 500;

            // a contrived set of bin bounds.
            int[,] octaveBinBounds = new[,]
            {
                { 2, 100 },
                { 4, 200 },
                { 6, 300 },
                { 8, 400 },
                { 12, 500 },
                { 14, 600 },
                { 16, 700 },
                { 18, 800 },
                { 20, 900 },
                { 23, 1000 },
                { 25, 1200 },
                { 30, 1400 },
                { 40, 1700 },
                { 46, 2001 },
                { 50, 2500 },
                { 55, 3000 },
                { 60, 3500 },
                { 69, 4002 },
                { 80, 5000 },
                { 84, 6000 },
                { 88, 7000 },
                { 92, 8004 },
                { 98, 10000 },
                { 105, 11000 },
            };

            var gridLineLocations = OctaveFreqScale.GetGridLineLocations(nyquist, linearBound, octaveBinBounds);

            var expected = new[,]
            {
                { 4, 500 },
                { 9, 1000 },
                { 13, 2000 },
                { 17, 4000 },
                { 21, 8000 },
            };

            Assert.That.MatricesAreEqual(expected, gridLineLocations);
        }

        /// <summary>
        /// Test static method which returns bin index for a given frequency.
        /// </summary>
        [TestMethod]
        public void TestReturnOfBinIndex()
        {
            var freqScale = new FrequencyScale(FreqScaleType.OctaveDataReduction);

            // test contents of the octave bin bounds matrix.
            int[,] octaveBinBounds = freqScale.BinBounds;

            Assert.AreEqual(20, octaveBinBounds.GetLength(0));

            int hertzValue = 500;
            var binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(2, binId);

            hertzValue = 1000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(4, binId);

            hertzValue = 2000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(7, binId);

            hertzValue = 4000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(12, binId);

            hertzValue = 8000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(17, binId);
        }

        /// <summary>
        /// Test of the default standard split LINEAR-Octave FREQ SCALE
        /// Check it on pure tone spectrum.
        /// By default, the split between linear and octave is at 1000 Hz.
        /// </summary>
        [TestMethod]
        public void TestSplitLinearOctaveFrequencyScale()
        {
            // Test default octave scale where default linear portion is 0-1000Hz.
            //var fst = FreqScaleType.Linear125Octaves6Tones30Nyquist11025;
            var fst = FreqScaleType.OctaveStandard;
            int nyquist = 11025;
            int frameSize = 512;
            int linearBound = 1000;
            int octaveToneCount = 32;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, frameSize, linearBound, octaveToneCount, gridInterval);

            Assert.AreEqual(freqScale.ScaleType, FreqScaleType.OctaveStandard);

            // test contents of the octave bin bounds matrix.
            int[,] octaveBinBounds = freqScale.BinBounds;
            Assert.AreEqual(103, octaveBinBounds.GetLength(0));
            Assert.AreEqual(991, octaveBinBounds[23, 1]);
            Assert.AreEqual(1034, octaveBinBounds[24, 1]);
            Assert.AreEqual(255, octaveBinBounds[102, 0]);
            Assert.AreEqual(10982, octaveBinBounds[102, 1]);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 23, 1000 },
                { 46, 2000 },
                { 69, 4000 },
                { 92, 8000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // generate pure tone spectrum.
            double[] linearSpectrum = new double[256];
            linearSpectrum[0] = 1.0;
            linearSpectrum[128] = 1.0;
            linearSpectrum[255] = 1.0;

            double[] octaveSpectrum = OctaveFreqScale.ConvertLinearSpectrumToOctaveScale(octaveBinBounds, linearSpectrum);

            Assert.AreEqual(103, octaveSpectrum.Length);
            Assert.AreEqual(1.0, octaveSpectrum[0]);
            Assert.AreEqual(0.0, octaveSpectrum[1]);
            Assert.AreEqual(0.0, octaveSpectrum[78]);
            Assert.AreEqual(0.125, octaveSpectrum[79]);
            Assert.AreEqual(0.125, octaveSpectrum[80]);
            Assert.AreEqual(0.0, octaveSpectrum[81]);
            Assert.AreEqual(0.0, octaveSpectrum[101]);
            Assert.AreEqual(0.166666666666666, octaveSpectrum[102], 0.000001);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale1()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            //var opFileStem = "BAC2_20071008";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "Octave1ScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);

            var fst = FreqScaleType.OctaveDataReduction;
            int nyquist = recording.SampleRate / 2;
            int frameSize = 16384;
            int linearBound = 125;
            int octaveToneCount = 25;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, frameSize, linearBound, octaveToneCount, gridInterval);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.75,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then convert to octave scale
            var amplitudeSpectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // TODO THIS IS THE CRITICAL LINE. COULD DO WITH SEPARATE UNIT TEST
            amplitudeSpectrogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(amplitudeSpectrogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(amplitudeSpectrogram.Data);
            amplitudeSpectrogram.Data = dataMatrix;
            amplitudeSpectrogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = amplitudeSpectrogram.GetImageFullyAnnotated(amplitudeSpectrogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // NOTE: After fixing bugs in Octave Scale code, the following expected BinBounds is no longer correct.
            //       Instead check size and some locations as below.

#pragma warning disable SA1500 // Braces for multi-line statements should not share line
            var expectedBinBounds = new[,]
            {
                { 0, 0 }, { 1, 3 }, { 2, 5 }, { 3, 8 }, { 4, 11 }, { 5, 13 }, { 6, 16 }, { 7, 19 }, { 8, 22 },
                { 9, 24 }, { 10, 27 }, { 11, 30 }, { 12, 32 }, { 13, 35 }, { 14, 38 }, { 15, 40 }, { 16, 43 },
                { 17, 46 }, { 18, 48 }, { 19, 51 }, { 20, 54 }, { 21, 57 }, { 22, 59 }, { 23, 62 }, { 24, 65 },
                { 25, 67 }, { 26, 70 }, { 27, 73 }, { 28, 75 }, { 29, 78 }, { 30, 81 }, { 31, 83 }, { 32, 86 },
                { 33, 89 }, { 34, 92 }, { 35, 94 }, { 36, 97 }, { 37, 100 }, { 38, 102 }, { 39, 105 }, { 40, 108 },
                { 41, 110 }, { 42, 113 }, { 43, 116 }, { 44, 118 }, { 45, 121 }, { 46, 124 }, { 47, 127 }, { 48, 129 },
                { 49, 132 }, { 50, 135 }, { 51, 137 }, { 52, 140 }, { 53, 143 }, { 55, 148 }, { 56, 151 }, { 57, 153 },
                { 58, 156 }, { 59, 159 }, { 61, 164 }, { 62, 167 }, { 63, 170 }, { 65, 175 }, { 66, 178 }, { 68, 183 },
                { 69, 186 }, { 71, 191 }, { 72, 194 }, { 74, 199 }, { 75, 202 }, { 77, 207 }, { 79, 213 }, { 80, 215 },
                { 82, 221 }, { 84, 226 }, { 86, 231 }, { 88, 237 }, { 89, 240 }, { 91, 245 }, { 93, 250 }, { 95, 256 },
                { 97, 261 }, { 100, 269 }, { 102, 275 }, { 104, 280 }, { 106, 285 }, { 109, 293 }, { 111, 299 },
                { 113, 304 }, { 116, 312 }, { 118, 318 }, { 121, 326 }, { 124, 334 }, { 126, 339 }, { 129, 347 },
                { 132, 355 }, { 135, 363 }, { 138, 371 }, { 141, 380 }, { 144, 388 }, { 147, 396 }, { 150, 404 },
                { 153, 412 }, { 157, 423 }, { 160, 431 }, { 164, 441 }, { 167, 450 }, { 171, 460 }, { 175, 471 },
                { 178, 479 }, { 182, 490 }, { 186, 501 }, { 190, 511 }, { 194, 522 }, { 199, 536 }, { 203, 546 },
                { 208, 560 }, { 212, 571 }, { 217, 584 }, { 221, 595 }, { 226, 608 }, { 231, 622 }, { 236, 635 },
                { 241, 649 }, { 247, 665 }, { 252, 678 }, { 258, 694 }, { 263, 708 }, { 269, 724 }, { 275, 740 },
                { 281, 756 }, { 287, 773 }, { 293, 789 }, { 300, 807 }, { 306, 824 }, { 313, 842 }, { 320, 861 },
                { 327, 880 }, { 334, 899 }, { 341, 918 }, { 349, 939 }, { 356, 958 }, { 364, 980 }, { 372, 1001 },
                { 380, 1023 }, { 388, 1044 }, { 397, 1069 }, { 406, 1093 }, { 415, 1117 }, { 424, 1141 }, { 433, 1165 },
                { 442, 1190 }, { 452, 1217 }, { 462, 1244 }, { 472, 1270 }, { 482, 1297 }, { 493, 1327 }, { 504, 1357 },
                { 515, 1386 }, { 526, 1416 }, { 537, 1445 }, { 549, 1478 }, { 561, 1510 }, { 573, 1542 }, { 586, 1577 },
                { 599, 1612 }, { 612, 1647 }, { 625, 1682 }, { 639, 1720 }, { 653, 1758 }, { 667, 1795 }, { 682, 1836 },
                { 697, 1876 }, { 712, 1916 }, { 728, 1960 }, { 744, 2003 }, { 760, 2046 }, { 776, 2089 }, { 793, 2134 },
                { 811, 2183 }, { 829, 2231 }, { 847, 2280 }, { 865, 2328 }, { 884, 2379 }, { 903, 2431 }, { 923, 2484 },
                { 943, 2538 }, { 964, 2595 }, { 985, 2651 }, { 1007, 2710 }, { 1029, 2770 }, { 1051, 2829 },
                { 1074, 2891 }, { 1098, 2955 }, { 1122, 3020 }, { 1146, 3085 }, { 1172, 3155 }, { 1197, 3222 },
                { 1223, 3292 }, { 1250, 3365 }, { 1278, 3440 }, { 1305, 3513 }, { 1334, 3591 }, { 1363, 3669 },
                { 1393, 3749 }, { 1424, 3833 }, { 1455, 3916 }, { 1487, 4002 }, { 1519, 4089 }, { 1552, 4177 },
                { 1586, 4269 }, { 1621, 4363 }, { 1657, 4460 }, { 1693, 4557 }, { 1730, 4657 }, { 1768, 4759 },
                { 1806, 4861 }, { 1846, 4969 }, { 1886, 5076 }, { 1928, 5190 }, { 1970, 5303 }, { 2013, 5418 },
                { 2057, 5537 }, { 2102, 5658 }, { 2148, 5782 }, { 2195, 5908 }, { 2243, 6037 }, { 2292, 6169 },
                { 2343, 6307 }, { 2394, 6444 }, { 2446, 6584 }, { 2500, 6729 }, { 2555, 6877 }, { 2610, 7025 },
                { 2668, 7181 }, { 2726, 7337 }, { 2786, 7499 }, { 2847, 7663 }, { 2909, 7830 }, { 2973, 8002 },
                { 3038, 8177 }, { 3104, 8355 }, { 3172, 8538 }, { 3242, 8726 }, { 3313, 8917 }, { 3385, 9111 },
                { 3459, 9310 }, { 3535, 9515 }, { 3612, 9722 }, { 3691, 9935 }, { 3772, 10153 }, { 3855, 10376 },
                { 3939, 10602 }, { 4026, 10837 }, { 4095, 11022 },
                { 4095, 11022 },
            };
#pragma warning restore SA1500 // Braces for multi-line statements should not share line

            //Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            Assert.AreEqual(255, freqScale.BinBounds.GetLength(0));
            Assert.AreEqual(23, freqScale.BinBounds[23, 0]);
            Assert.AreEqual(31, freqScale.BinBounds[23, 1]);
            Assert.AreEqual(46, freqScale.BinBounds[46, 0]);
            Assert.AreEqual(62, freqScale.BinBounds[46, 1]);
            Assert.AreEqual(113, freqScale.BinBounds[100, 0]);
            Assert.AreEqual(152, freqScale.BinBounds[100, 1]);
            Assert.AreEqual(246, freqScale.BinBounds[128, 0]);
            Assert.AreEqual(331, freqScale.BinBounds[128, 1]);
            Assert.AreEqual(8191, freqScale.BinBounds[254, 0]);
            Assert.AreEqual(11024, freqScale.BinBounds[254, 1]);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 93, 125 },
                { 118, 250 },
                { 143, 500 },
                { 168, 1000 },
                { 193, 2000 },
                { 218, 4000 },
                { 243, 8000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(321, image.Width);
            Assert.AreEqual(309, image.Height);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING on Jasco MArine Recording, SR = 64000
        /// 24 BIT JASCO RECORDINGS from GBR must be converted to 16 bit.
        /// ffmpeg -i source_file.wav -sample_fmt s16 out_file.wav
        /// e.g. ". C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg\ffmpeg.exe" -i "C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56.wav" -sample_fmt s16 "C:\SensorNetworks\Output\OctaveFreqScale\JascoeMarineGBR116bit.wav"
        /// ffmpeg binaries are in C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg.
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale2()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "MarineJasco_AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit-60sec.wav");
            //var opFileStem = "JascoMarineGBR1";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Octave2ScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);
            //var fst = FreqScaleType.Linear125OctaveTones28Nyquist32000;
            var fst = FreqScaleType.OctaveCustom;
            int nyquist = recording.SampleRate / 2;
            int frameSize = 16384;
            int linearBound = 125;
            int octaveToneCount = 28;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, frameSize, linearBound, octaveToneCount, gridInterval);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.2,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // DO FILE EQUALITY TESTS
            // Check that freqScale.OctaveBinBounds are correct
            // WARNING: TODO FIX UP THIS TEST.
            //var expectedBinBoundsFile = PathHelper.ResolveAsset("FrequencyScale", opFileStem + "_Octave2ScaleBinBounds.EXPECTED.json");
            //var expectedBinBounds = Json.Deserialize<int[,]>(expectedBinBoundsFile);
            //Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            // INSTEAD DO THIS TEST
            Assert.AreEqual(255, freqScale.BinBounds.GetLength(0));
            Assert.AreEqual(23, freqScale.BinBounds[23, 0]);
            Assert.AreEqual(62, freqScale.BinBounds[23, 1]);
            Assert.AreEqual(54, freqScale.BinBounds[52, 0]);
            Assert.AreEqual(145, freqScale.BinBounds[52, 1]);
            Assert.AreEqual(177, freqScale.BinBounds[100, 0]);
            Assert.AreEqual(476, freqScale.BinBounds[100, 1]);
            Assert.AreEqual(354, freqScale.BinBounds[128, 0]);
            Assert.AreEqual(953, freqScale.BinBounds[128, 1]);
            Assert.AreEqual(8191, freqScale.BinBounds[254, 0]);
            Assert.AreEqual(22047, freqScale.BinBounds[254, 1]);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 47, 125 },
                { 74, 250 },
                { 102, 500 },
                { 130, 1000 },
                { 158, 2000 },
                { 186, 4000 },
                { 214, 8000 },
                { 242, 16000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(201, image.Width);
            Assert.AreEqual(309, image.Height);
        }

        /// <summary>
        /// Test of frequency scale used for spectral data reduction.
        /// Reduces a 256 spectrum to 20 value vector.
        /// Check it on artificial spectrum with three tones.
        /// By default, the split between linear and octave is at 1000 Hz.
        /// </summary>
        [TestMethod]
        public void TestSpectralReductionScale()
        {
            var fst = FreqScaleType.OctaveDataReduction;
            var freqScale = new FrequencyScale(fst);
            Assert.AreEqual(freqScale.ScaleType, FreqScaleType.OctaveDataReduction);

            // test contents of the octave bin bounds matrix.
            Assert.AreEqual(20, freqScale.BinBounds.GetLength(0));

            var expectedBinBounds = new[,]
            {
                { 0, 0 }, { 1, 258 }, { 2, 517 }, { 3, 775 }, { 4, 1034 }, { 5, 1292 }, { 6, 1550 }, { 47, 2024 },
                { 54, 2326 }, { 62, 2670 }, { 71, 3058 }, { 81, 3488 }, { 93, 4005 }, { 107, 4608 }, { 123, 5297 }, { 141, 6072 },
                { 162, 6977 }, { 186, 8010 }, { 214, 9216 }, { 255, 10982 },
            };

            Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            // generate pure tone spectrum.
            double[] linearSpectrum = new double[256];
            linearSpectrum[0] = 1.0;
            linearSpectrum[128] = 1.0;
            linearSpectrum[255] = 1.0;

            double[] octaveSpectrum = OctaveFreqScale.ConvertLinearSpectrumToOctaveScale(freqScale.BinBounds, linearSpectrum);

            Assert.AreEqual(20, octaveSpectrum.Length);
            Assert.AreEqual(1.0, octaveSpectrum[0]);
            Assert.AreEqual(0.0, octaveSpectrum[1]);
            Assert.AreEqual(0.0, octaveSpectrum[13]);
            Assert.AreEqual(0.042483660130718942, octaveSpectrum[14]);
            Assert.AreEqual(0.014245014245014251, octaveSpectrum[15]);
            Assert.AreEqual(0.0, octaveSpectrum[16]);
            Assert.AreEqual(0.0, octaveSpectrum[18]);
            Assert.AreEqual(0.047619047619047616, octaveSpectrum[19], 0.000001);
        }

        /// <summary>
        /// Tests linear freq scale using an artificial recording containing five sine waves.
        /// </summary>
        [TestMethod]
        public void TestFreqScaleOnArtificialSignal1()
        {
            int sampleRate = 22050;
            double duration = 20; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            int windowSize = 512;
            var freqScale = new FrequencyScale(sampleRate / 2, windowSize, 1000);
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Signal1_LinearFreqScale.png");

            var recording = DspFilters.GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Cosine);
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.0,
                SourceFName = "Signal1",
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.12,
            };

            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // pick a row, any row
            var oneSpectrum = MatrixTools.GetRow(sonogram.Data, 40);
            oneSpectrum = DataTools.filterMovingAverage(oneSpectrum, 5);
            var peaks = DataTools.GetPeaks(oneSpectrum);
            for (int i = 5; i < peaks.Length - 5; i++)
            {
                if (peaks[i])
                {
                    LoggedConsole.WriteLine($"bin ={freqScale.BinBounds[i, 0]},  Herz={freqScale.BinBounds[i, 1]}-{freqScale.BinBounds[i + 1, 1]}  ");
                }
            }

            foreach (int h in harmonics)
            {
                LoggedConsole.WriteLine($"Harmonic {h}Herz  should be in bin  {freqScale.GetBinIdForHerzValue(h)}");
            }

            // spectrogram without framing, annotation etc
            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: {DataTools.Array2String(harmonics)}   SR={sampleRate}  Window={windowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // Check that image dimensions are correct
            Assert.AreEqual(861, image.Width);
            Assert.AreEqual(310, image.Height);

            Assert.IsTrue(peaks[11]);
            Assert.IsTrue(peaks[22]);
            Assert.IsTrue(peaks[45]);
            Assert.IsTrue(peaks[92]);
            Assert.IsTrue(peaks[185]);
        }

        /// <summary>
        /// Tests octave freq scale using an artificial recording containing five sine waves.
        /// </summary>
        [TestMethod]
        public void TestFreqScaleOnArtificialSignal2()
        {
            int sampleRate = 64000;
            double duration = 30; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };

            //var fst = FreqScaleType.Linear125OctaveTones28Nyquist32000;
            var fst = FreqScaleType.OctaveCustom;
            int nyquist = sampleRate / 2;
            int frameSize = 16384;
            int linearBound = 125;
            int octaveToneCount = 28;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, frameSize, linearBound, octaveToneCount, gridInterval);
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Signal2_OctaveFreqScale.png");
            var recording = DspFilters.GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Cosine);

            // init the default sonogram config
            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.2,
                SourceFName = "Signal2",
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // pick a row, any row
            var oneSpectrum = MatrixTools.GetRow(sonogram.Data, 40);
            oneSpectrum = DataTools.filterMovingAverage(oneSpectrum, 5);
            var peaks = DataTools.GetPeaks(oneSpectrum);

            var peakIds = new List<int>();
            for (int i = 5; i < peaks.Length - 5; i++)
            {
                if (peaks[i])
                {
                    int peakId = freqScale.BinBounds[i, 0];
                    peakIds.Add(peakId);
                    LoggedConsole.WriteLine($"Spectral peak located in bin {peakId},  Herz={freqScale.BinBounds[i, 1]}");
                }
            }

            foreach (int h in harmonics)
            {
                LoggedConsole.WriteLine($"Harmonic {h}Herz should be in bin {freqScale.GetBinIdForHerzValue(h)}");
            }

            Assert.AreEqual(5, peakIds.Count);
            Assert.AreEqual(129, peakIds[0]);
            Assert.AreEqual(257, peakIds[1]);
            Assert.AreEqual(513, peakIds[2]);
            Assert.AreEqual(1025, peakIds[3]);
            Assert.AreEqual(2049, peakIds[4]);

            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: {DataTools.Array2String(harmonics)}   SR={sampleRate}  Window={freqScale.WindowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // Check that image dimensions are correct
            Assert.AreEqual(146, image.Width);
            Assert.AreEqual(311, image.Height);
        }
    }
}