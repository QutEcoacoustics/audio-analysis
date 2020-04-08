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
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING
        /// Check it on standard one minute recording, SR=22050.
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale1()
        {
            var recordingPath = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");
            var opFileStem = "BAC2_20071008";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "Octave1ScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);

            // default octave scale
            var fst = FreqScaleType.Linear125Octaves6Tones30Nyquist11025;
            var freqScale = new FrequencyScale(fst);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = freqScale.WindowSize,
                WindowOverlap = 0.75,
                SourceFName = recording.BaseName,
                NoiseReductionType = NoiseReductionType.None,
                NoiseReductionParameter = 0.0,
            };

            // Generate amplitude sonogram and then conver to octave scale
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);

            // THIS IS THE CRITICAL LINE. COULD DO WITH SEPARATE UNIT TEST
            sonogram.Data = OctaveFreqScale.ConvertAmplitudeSpectrogramToDecibelOctaveScale(sonogram.Data, freqScale);

            // DO NOISE REDUCTION
            var dataMatrix = SNR.NoiseReduce_Standard(sonogram.Data);
            sonogram.Data = dataMatrix;
            sonogram.Configuration.WindowSize = freqScale.WindowSize;

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath);

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

            Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 46, 125 },
                { 79, 250 },
                { 111, 500 },
                { 143, 1000 },
                { 175, 2000 },
                { 207, 4000 },
                { 239, 8000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(645, image.Width);
            Assert.AreEqual(310, image.Height);
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
            var opFileStem = "JascoMarineGBR1";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "Octave2ScaleSonogram.png");

            var recording = new AudioRecording(recordingPath);
            var fst = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            var freqScale = new FrequencyScale(fst);

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

            var expectedBinBoundsFile = PathHelper.ResolveAsset("FrequencyScale", opFileStem + "_Octave2ScaleBinBounds.EXPECTED.json");
            var expectedBinBounds = Json.Deserialize<int[,]>(expectedBinBoundsFile);

            Assert.That.MatricesAreEqual(expectedBinBounds, freqScale.BinBounds);

            // Check that freqScale.GridLineLocations are correct
            var expected = new[,]
            {
                { 34, 125 },
                { 62, 250 },
                { 89, 500 },
                { 117, 1000 },
                { 145, 2000 },
                { 173, 4000 },
                { 201, 8000 },
                { 229, 16000 },
                { 256, 32000 },
            };

            Assert.That.MatricesAreEqual(expected, freqScale.GridLineLocations);

            // Check that image dimensions are correct
            Assert.AreEqual(201, image.Width);
            Assert.AreEqual(310, image.Height);
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
            var freqScale = new FrequencyScale(FreqScaleType.Linear125Octaves7Tones28Nyquist32000);
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
            Assert.AreEqual(310, image.Height);
        }
    }
}