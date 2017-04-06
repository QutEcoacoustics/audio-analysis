// <copyright file="FrequencyScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    /// <summary>
    /// Test methods for the various Frequency Scales
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// </summary>
    [TestClass]
    public class FrequencyScaleTests
    {
        private DirectoryInfo outputDirectory;

        #region Additional test attributes

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
            this.outputDirectory = TestHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        #endregion

        /// <summary>
        /// METHOD TO CHECK IF Default linear FREQ SCALE IS WORKING
        /// Check it on standard one minute recording.
        /// </summary>
        [TestMethod]
        public void LinearFrequencyScaleDefault()
        {
            // relative path because post-Build command transfers files to ...\\Work\GitHub\...\bin\Debug subfolder.
            var recordingPath = @"Recordings\BAC2_20071008-085040.wav";
            var opFileStem = "BAC2_20071008";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "DefaultLinearScaleSonogram.png");

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

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath, ImageFormat.Png);

            // DO UNIT TESTING
            var stemOfExpectedFile = opFileStem + "_DefaultLinearScaleGridLineLocations.EXPECTED.json";
            var stemOfActualFile = opFileStem + "_DefaultLinearScaleGridLineLocations.ACTUAL.json";

            // Check that freqScale.GridLineLocations are correct
            var expectedFile1 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile1.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile1, freqScale.GridLineLocations);
            FileEqualityHelpers.TextFileEqual(expectedFile1, resultFile1);

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
            var recordingPath = @"Recordings\BAC2_20071008-085040.wav";
            var opFileStem = "BAC2_20071008";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "LinearScaleSonogram.png");

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

            var image = sonogram.GetImageFullyAnnotated(sonogram.GetImage(), "SPECTROGRAM: " + fst.ToString(), freqScale.GridLineLocations);
            image.Save(outputImagePath, ImageFormat.Png);

            // DO FILE EQUALITY TEST
            var stemOfExpectedFile = opFileStem + "_LinearScaleGridLineLocations.EXPECTED.json";
            var stemOfActualFile = opFileStem + "_LinearScaleGridLineLocations.ACTUAL.json";

            // Check that freqScale.GridLineLocations are correct
            var expectedFile1 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile1.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile1, freqScale.GridLineLocations);
            FileEqualityHelpers.TextFileEqual(expectedFile1, resultFile1);

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
            var recordingPath = @"Recordings\BAC2_20071008-085040.wav";
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
            image.Save(outputImagePath, ImageFormat.Png);

            // DO FILE EQUALITY TESTS
            // Check that freqScale.OctaveBinBounds are correct
            var stemOfExpectedFile = opFileStem + "_Octave1ScaleBinBounds.EXPECTED.json";
            var stemOfActualFile = opFileStem + "_Octave1ScaleBinBounds.ACTUAL.json";
            var expectedFile1 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile1.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile1, freqScale.OctaveBinBounds);
            FileEqualityHelpers.TextFileEqual(expectedFile1, resultFile1);

            // Check that freqScale.GridLineLocations are correct
            stemOfExpectedFile = opFileStem + "_Octave1ScaleGridLineLocations.EXPECTED.json";
            stemOfActualFile = opFileStem + "_Octave1ScaleGridLineLocations.ACTUAL.json";
            var expectedFile2 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile2.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            FileEqualityHelpers.TextFileEqual(expectedFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(645, image.Width);
            Assert.AreEqual(310, image.Height);
        }

        /// <summary>
        /// METHOD TO CHECK IF Octave FREQ SCALE IS WORKING on Jasco MArine Recording, SR = 64000
        /// 24 BIT JASCO RECORDINGS from GBR must be converted to 16 bit.
        /// ffmpeg -i source_file.wav -sample_fmt s16 out_file.wav
        /// e.g. ". C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg\ffmpeg.exe" -i "C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56.wav" -sample_fmt s16 "C:\SensorNetworks\Output\OctaveFreqScale\JascoeMarineGBR116bit.wav"
        /// ffmpeg binaries are in C:\Work\Github\audio-analysis\Extra Assemblies\ffmpeg
        /// </summary>
        [TestMethod]
        public void OctaveFrequencyScale2()
        {
            var recordingPath = @"Recordings\MarineJasco_AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit-60sec.wav";
            var opFileStem = "JascoMarineGBR1";
            var outputDir = this.outputDirectory;
            var outputImagePath = Path.Combine(outputDir.FullName, "Octave2ScaleSonogram.png");

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
            image.Save(outputImagePath, ImageFormat.Png);

            // DO FILE EQUALITY TESTS
            // Check that freqScale.OctaveBinBounds are correct
            var stemOfExpectedFile = opFileStem + "_Octave2ScaleBinBounds.EXPECTED.json";
            var stemOfActualFile = opFileStem + "_Octave2ScaleBinBounds.ACTUAL.json";
            var expectedFile1 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile1.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile1 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile1, freqScale.OctaveBinBounds);
            FileEqualityHelpers.TextFileEqual(expectedFile1, resultFile1);

            // Check that freqScale.GridLineLocations are correct
            stemOfExpectedFile = opFileStem + "_Octave2ScaleGridLineLocations.EXPECTED.json";
            stemOfActualFile = opFileStem + "_Octave2ScaleGridLineLocations.ACTUAL.json";
            var expectedFile2 = new FileInfo("FrequencyScale\\" + stemOfExpectedFile);
            if (!expectedFile2.Exists)
            {
                LoggedConsole.WriteErrorLine("An EXPECTED results file does not exist. Test will fail!");
                LoggedConsole.WriteErrorLine("If ACTUAL results file is correct, move it to dir <...\\TestResources\\FrequencyScale> and change its suffix to <.EXPECTED.json>");
            }

            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            FileEqualityHelpers.TextFileEqual(expectedFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(201, image.Width);
            Assert.AreEqual(310, image.Height);
        }
    }
}
