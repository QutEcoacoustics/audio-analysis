// <copyright file="FrequencyScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    //using static global::TowseyLibrary.FFT;
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

            Assert.AreEqual(-72.15547149521359, spectrogramMatrix[0, 0], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 1], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 32], 0.0001);
            Assert.AreEqual(-75.543977604537076, spectrogramMatrix[0, 49], 0.0001);
            Assert.AreEqual(-157.82548429035143, spectrogramMatrix[0, 62], 0.0001);
            Assert.AreEqual(-84.3026462113695, spectrogramMatrix[0, 63], 0.0001);
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

            Assert.AreEqual(19, octaveBinBounds.GetLength(0));

            int hertzValue = 500;
            var binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(2, binId);

            hertzValue = 1000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(3, binId);

            hertzValue = 2000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(6, binId);

            hertzValue = 4000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(11, binId);

            hertzValue = 8000;
            binId = freqScale.GetBinIdForHerzValue(hertzValue);
            Assert.AreEqual(16, binId);
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

            double[] octaveSpectrum = SpectrogramTools.RescaleSpectrumUsingFilterbank(octaveBinBounds, linearSpectrum);

            Assert.AreEqual(103, octaveSpectrum.Length);
            Assert.AreEqual(1.0, octaveSpectrum[0]);
            Assert.AreEqual(0.0, octaveSpectrum[1]);
            Assert.AreEqual(0.0, octaveSpectrum[78]);
            Assert.AreEqual(0.125, octaveSpectrum[79]);
            Assert.AreEqual(0.125, octaveSpectrum[80]);
            Assert.AreEqual(0.0, octaveSpectrum[81]);
            Assert.AreEqual(0.0, octaveSpectrum[101]);
            Assert.AreEqual(0.1666666666, octaveSpectrum[102], 0.000001);
        }

        /// <summary>
        /// METHOD TO CHECK IF Conversion of linear amplitude spectrum to Octave FREQ SCALE IS WORKING
        /// Check it on spectrum derived from an assumed signal, SR=22050.
        /// </summary>
        [TestMethod]
        public void TestConversionOfAmplitudeSpectrogramToOctaveScaled()
        {
            int sr = 22050;
            int windowSize = 512;

            // set up the frequency scale.
            var fst = FreqScaleType.OctaveStandard;
            int nyquist = sr / 2;
            int linearBound = 1000;
            int octaveToneCount = 25; //not used
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, windowSize, linearBound, octaveToneCount, gridInterval);

            // make a dummy spectrum
            int binCount = windowSize / 2;
            var linearSpectrum = new double[binCount];
            linearSpectrum[0] = 1.0;
            linearSpectrum[64] = 1.0;
            linearSpectrum[128] = 1.0;
            linearSpectrum[192] = 1.0;
            linearSpectrum[255] = 1.0;

            var octaveSpectrum = SpectrogramTools.RescaleSpectrumUsingFilterbank(freqScale.BinBounds, linearSpectrum);
            Assert.AreEqual(1.000, octaveSpectrum[0], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[1], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[55], 0.0001);
            Assert.AreEqual(0.250, octaveSpectrum[56], 0.0001);
            Assert.AreEqual(0.250, octaveSpectrum[57], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[78], 0.0001);
            Assert.AreEqual(0.125, octaveSpectrum[79], 0.0001);
            Assert.AreEqual(0.125, octaveSpectrum[80], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[81], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[92], 0.0001);
            Assert.AreEqual(0.166, octaveSpectrum[93], 0.001);
            Assert.AreEqual(0.000, octaveSpectrum[94], 0.0001);
            Assert.AreEqual(0.000, octaveSpectrum[101], 0.0001);
            Assert.AreEqual(0.166, octaveSpectrum[102], 0.001);

            // Now test conversion of amplitude to power values.
            // make fft class using rectangular window - just to get a value for Window power.
            var fft = new FFT(windowSize);
            double windowPower = fft.WindowPower;
            var recordingBitsPerSample = 16;
            var epsilon = Math.Pow(0.5, recordingBitsPerSample - 1);

            // set up matrix derived from previous spectrum to test conversion to power.
            var octaveScaleM = new double[2, octaveSpectrum.Length];
            MatrixTools.SetRow(octaveScaleM, 0, octaveSpectrum);
            MatrixTools.SetRow(octaveScaleM, 1, octaveSpectrum);
            var powerSpectrogram = OctaveFreqScale.ConvertAmplitudeToPowerSpectrogram(octaveScaleM, windowPower, sr);

            Assert.AreEqual(1.771541E-07, powerSpectrogram[0, 0], 0.000001);
            Assert.AreEqual(0.000, powerSpectrogram[1, 1], 0.0001);
            Assert.AreEqual(0.000, powerSpectrogram[0, 55], 0.0001);
            Assert.AreEqual(1.107213E-08, powerSpectrogram[1, 56], 0.000001);
            Assert.AreEqual(1.107213E-08, powerSpectrogram[0, 57], 0.000001);
            Assert.AreEqual(0.000, powerSpectrogram[1, 78], 0.0001);
            Assert.AreEqual(2.768034E-09, powerSpectrogram[0, 79], 0.0001);
            Assert.AreEqual(2.768034E-09, powerSpectrogram[1, 80], 0.0001);
            Assert.AreEqual(0.000, powerSpectrogram[0, 81], 0.0001);
            Assert.AreEqual(0.000, powerSpectrogram[1, 92], 0.0001);
            Assert.AreEqual(4.920949E-09, powerSpectrogram[0, 93], 0.0001);
            Assert.AreEqual(0.000, powerSpectrogram[1, 94], 0.0001);
            Assert.AreEqual(0.000, powerSpectrogram[0, 101], 0.0001);
            Assert.AreEqual(1.771541E-07, powerSpectrogram[1, 102], 0.001);

            // now test conversion from power to decibels.
            // Convert the power values to log using: dB = 10*log(power)
            var powerEpsilon = epsilon * epsilon / windowPower / sr;
            var dbSpectrogram = MatrixTools.SpectrogramPower2DeciBels(powerSpectrogram, powerEpsilon, out var min, out var max);
            Assert.AreEqual(-67.516485, dbSpectrogram[0, 0], 0.000001);
            Assert.AreEqual(-160.83578, dbSpectrogram[1, 1], 0.0001);
            Assert.AreEqual(-160.83578, dbSpectrogram[0, 55], 0.0001);
            Assert.AreEqual(-79.557685, dbSpectrogram[1, 56], 0.000001);
            Assert.AreEqual(-79.557685, dbSpectrogram[0, 57], 0.000001);
            Assert.AreEqual(-160.83578, dbSpectrogram[1, 78], 0.0001);
            Assert.AreEqual(-85.578285, dbSpectrogram[0, 79], 0.0001);
            Assert.AreEqual(-85.578285, dbSpectrogram[1, 80], 0.0001);
            Assert.AreEqual(-160.83578, dbSpectrogram[0, 81], 0.0001);
            Assert.AreEqual(-160.83578, dbSpectrogram[1, 92], 0.0001);
            Assert.AreEqual(-83.079510, dbSpectrogram[0, 93], 0.0001);
            Assert.AreEqual(-160.83578, dbSpectrogram[1, 94], 0.0001);
            Assert.AreEqual(-160.83578, dbSpectrogram[0, 101], 0.0001);
            Assert.AreEqual(-83.079510, dbSpectrogram[1, 102], 0.0001);
        }

        /// <summary>
        /// Tests octave freq scale using an artificial recording containing five sine waves.
        /// </summary>
        [TestMethod]
        public void TestFreqScaleOnArtificialSignal2()
        {
            int sr = 64000;
            double duration = 30; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };

            //var fst = FreqScaleType.Linear125OctaveTones28Nyquist32000;
            var fst = FreqScaleType.OctaveCustom;
            int nyquist = sr / 2;
            int frameSize = 16384;
            int linearBound = 125;
            int octaveToneCount = 28;
            int gridInterval = 1000;
            var freqScale = new FrequencyScale(fst, nyquist, frameSize, linearBound, octaveToneCount, gridInterval);
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Signal2_OctaveFreqScale.png");
            var recording = DspFilters.GenerateTestRecording(sr, duration, harmonics, WaveType.Cosine);

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
            var windowPower = sonogram.Configuration.WindowPower;
            var epsilon = sonogram.Configuration.epsilon;
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToFreqScaledDecibels(sonogram.Data, windowPower, sr, epsilon, freqScale);

            // pick a row, any row - they should all be the same.
            var oneSpectrum = MatrixTools.GetRow(sonogram.Data, 40);
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
            string title = $"Spectrogram of Harmonics: {DataTools.Array2String(harmonics)}   SR={sr}  Window={freqScale.WindowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            image.Save(outputImagePath);

            // Check that image dimensions are correct
            Assert.AreEqual(146, image.Width);
            Assert.AreEqual(311, image.Height);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale1()
        {
            //var opFileStem = "BAC2_20071008";
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "Octave1ScaleSonogram.png");
            var recording = new AudioRecording(recordingPath);

            var fst = FreqScaleType.OctaveCustom;
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

            // THIS IS THE CRITICAL LINE. It has separate UNIT TEST above.
            double windowPower = amplitudeSpectrogram.Configuration.WindowPower;
            int sampleRate = amplitudeSpectrogram.SampleRate;
            double epsilon = amplitudeSpectrogram.Configuration.epsilon;
            amplitudeSpectrogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToFreqScaledDecibels(amplitudeSpectrogram.Data, windowPower, sampleRate, epsilon, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(amplitudeSpectrogram.Data);
            amplitudeSpectrogram.Data = dataMatrix;
            amplitudeSpectrogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = amplitudeSpectrogram.GetImageFullyAnnotated(amplitudeSpectrogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

            //string binLiteral = freqScale.BinBounds.PrintAsLiteral<int>();
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
            var expectedBinBounds = new[,]
            {
                { 0, 0 }, { 1, 1 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 7 }, { 6, 8 }, { 7, 9 }, { 8, 11 },
                { 9, 12 }, { 10, 13 }, { 11, 15 }, { 12, 16 }, { 13, 17 }, { 14, 19 }, { 15, 20 }, { 16, 22 },
                { 17, 23 }, { 18, 24 }, { 19, 26 }, { 20, 27 }, { 21, 28 }, { 22, 30 }, { 23, 31 }, { 24, 32 },
                { 25, 34 }, { 26, 35 }, { 27, 36 }, { 28, 38 }, { 29, 39 }, { 30, 40 }, { 31, 42 }, { 32, 43 },
                { 33, 44 }, { 34, 46 }, { 35, 47 }, { 36, 48 }, { 37, 50 }, { 38, 51 }, { 39, 52 }, { 40, 54 },
                { 41, 55 }, { 42, 57 }, { 43, 58 }, { 44, 59 }, { 45, 61 }, { 46, 62 }, { 47, 63 }, { 48, 65 },
                { 49, 66 }, { 50, 67 }, { 51, 69 }, { 52, 70 }, { 53, 71 }, { 54, 73 }, { 55, 74 }, { 56, 75 },
                { 57, 77 }, { 58, 78 }, { 59, 79 }, { 60, 81 }, { 61, 82 }, { 62, 83 }, { 63, 85 }, { 64, 86 },
                { 65, 87 }, { 66, 89 }, { 67, 90 }, { 68, 92 }, { 69, 93 }, { 70, 94 }, { 71, 96 }, { 72, 97 },
                { 73, 98 }, { 74, 100 }, { 75, 101 }, { 76, 102 }, { 77, 104 }, { 78, 105 }, { 79, 106 }, { 80, 108 },
                { 81, 109 }, { 82, 110 }, { 83, 112 }, { 84, 113 }, { 85, 114 }, { 86, 116 }, { 87, 117 }, { 88, 118 },
                { 89, 120 }, { 90, 121 }, { 91, 122 }, { 92, 124 }, { 93, 125 }, { 96, 129 }, { 99, 133 },
                { 101, 136 }, { 104, 140 }, { 107, 144 }, { 110, 148 }, { 113, 152 }, { 116, 156 }, { 120, 161 },
                { 123, 166 }, { 127, 171 }, { 130, 175 }, { 134, 180 }, { 137, 184 }, { 141, 190 }, { 145, 195 },
                { 149, 201 }, { 153, 206 }, { 158, 213 }, { 162, 218 }, { 167, 225 }, { 171, 230 }, { 176, 237 },
                { 181, 244 }, { 186, 250 }, { 191, 257 }, { 197, 265 }, { 202, 272 }, { 208, 280 }, { 214, 288 },
                { 220, 296 }, { 226, 304 }, { 232, 312 }, { 239, 322 }, { 246, 331 }, { 253, 340 }, { 260, 350 },
                { 267, 359 }, { 274, 369 }, { 282, 380 }, { 290, 390 }, { 298, 401 }, { 306, 412 }, { 315, 424 },
                { 324, 436 }, { 333, 448 }, { 342, 460 }, { 352, 474 }, { 362, 487 }, { 372, 501 }, { 382, 514 },
                { 393, 529 }, { 404, 544 }, { 416, 560 }, { 427, 575 }, { 439, 591 }, { 452, 608 }, { 464, 624 },
                { 477, 642 }, { 491, 661 }, { 505, 680 }, { 519, 698 }, { 533, 717 }, { 548, 738 }, { 564, 759 },
                { 579, 779 }, { 596, 802 }, { 612, 824 }, { 630, 848 }, { 647, 871 }, { 666, 896 }, { 684, 921 },
                { 703, 946 }, { 723, 973 }, { 744, 1001 }, { 764, 1028 }, { 786, 1058 }, { 808, 1087 }, { 831, 1118 },
                { 854, 1149 }, { 878, 1182 }, { 903, 1215 }, { 928, 1249 }, { 954, 1284 }, { 981, 1320 },
                { 1009, 1358 },
                { 1037, 1396 }, { 1066, 1435 }, { 1096, 1475 }, { 1127, 1517 }, { 1158, 1558 }, { 1191, 1603 },
                { 1224, 1647 }, { 1259, 1694 }, { 1294, 1741 }, { 1331, 1791 }, { 1368, 1841 }, { 1406, 1892 },
                { 1446, 1946 }, { 1487, 2001 }, { 1528, 2056 }, { 1571, 2114 }, { 1615, 2174 }, { 1661, 2235 },
                { 1708, 2299 }, { 1756, 2363 }, { 1805, 2429 }, { 1856, 2498 }, { 1908, 2568 }, { 1961, 2639 },
                { 2017, 2715 }, { 2073, 2790 }, { 2131, 2868 }, { 2191, 2949 }, { 2253, 3032 }, { 2316, 3117 },
                { 2381, 3204 }, { 2448, 3295 }, { 2517, 3387 }, { 2588, 3483 }, { 2661, 3581 }, { 2735, 3681 },
                { 2812, 3784 }, { 2891, 3891 }, { 2973, 4001 }, { 3056, 4113 }, { 3142, 4229 }, { 3230, 4347 },
                { 3321, 4469 }, { 3415, 4596 }, { 3511, 4725 }, { 3609, 4857 }, { 3711, 4994 }, { 3815, 5134 },
                { 3922, 5278 }, { 4033, 5428 }, { 4146, 5580 }, { 4262, 5736 }, { 4382, 5897 }, { 4505, 6063 },
                { 4632, 6234 }, { 4762, 6409 }, { 4896, 6589 }, { 5034, 6775 }, { 5175, 6965 }, { 5321, 7161 },
                { 5470, 7362 }, { 5624, 7569 }, { 5782, 7782 }, { 5945, 8001 }, { 6112, 8226 }, { 6284, 8457 },
                { 6460, 8694 }, { 6642, 8939 }, { 6829, 9191 }, { 7021, 9449 }, { 7218, 9714 }, { 7421, 9987 },
                { 7630, 10269 }, { 7844, 10557 }, { 8191, 11024 },
            };
#pragma warning restore SA1500 // Braces for multi-line statements should not share line

            Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

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
            //var opFileStem = "JascoMarineGBR1";
            var recordingPath = PathHelper.ResolveAsset("Recordings", "MarineJasco_AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit-60sec.wav");
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Octave2ScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);
            int sr = recording.SampleRate;

            var fst = FreqScaleType.OctaveCustom;
            int nyquist = sr / 2;
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
            var windowPower = sonogram.Configuration.WindowPower;
            var epsilon = sonogram.Configuration.epsilon;
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToFreqScaledDecibels(sonogram.Data, windowPower, sr, epsilon, freqScale);

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
            Assert.AreEqual(19, freqScale.BinBounds.GetLength(0));

            var expectedBinBounds = new[,]
            {
                { 0, 0 }, { 8, 345 }, { 16, 689 },                                        // linear up to 1000 Hz
                { 24, 1034 }, { 32, 1378 }, { 40, 1723 },                                 // linear 1000-2000 Hz
                { 48, 2067 }, { 54, 2326 }, { 62, 2670 }, { 71, 3058 }, { 81, 3488 },     // first octave 2-4 kHz
                { 93, 4005 }, { 107, 4608 }, { 123, 5297 }, { 141, 6072 }, { 162, 6977 }, // second octave 4-8 kHz
                { 186, 8010 }, { 214, 9216 }, { 255, 10982 },                             // residual octave 8-11 kHz
            };

            Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            // generate pure tone spectrum.
            double[] linearSpectrum = new double[256];
            linearSpectrum[0] = 1.0;
            linearSpectrum[128] = 1.0;
            linearSpectrum[255] = 1.0;

            double[] octaveSpectrum = SpectrogramTools.RescaleSpectrumUsingFilterbank(freqScale.BinBounds, linearSpectrum);

            Assert.AreEqual(19, octaveSpectrum.Length);
            Assert.AreEqual(0.222222, octaveSpectrum[0], 0.00001);
            Assert.AreEqual(0.0, octaveSpectrum[1], 0.00001);
            Assert.AreEqual(0.0, octaveSpectrum[12], 0.00001);
            Assert.AreEqual(0.042483, octaveSpectrum[13], 0.00001);
            Assert.AreEqual(0.014245, octaveSpectrum[14], 0.00001);
            Assert.AreEqual(0.0, octaveSpectrum[15]);
            Assert.AreEqual(0.0, octaveSpectrum[17]);
            Assert.AreEqual(0.047619, octaveSpectrum[18], 0.00001);
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
    }
}