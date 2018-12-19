// <copyright file="IndexCalculateTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
<<<<<<< HEAD

    // using TestHelpers;
=======
>>>>>>> master

    /// <summary>
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// See this commit for dealing with BLOBs: https://github.com/QutBioacoustics/audio-analysis/commit/55142089c8eb65d46e2f96f1d2f9a30d89b62710
    /// </summary>
    [TestClass]
    public class IndexCalculateTest
    {
        private const double AllowedDelta = 0.000001;
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
        /// Test the various summary indices
        /// </summary>
        [TestMethod]
        public void TestOfSummaryIndices()
        {
            var sourceRecording = PathHelper.ResolveAsset(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");
<<<<<<< HEAD
=======

>>>>>>> master
            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");

            // var outputDir = this.outputDirectory;
            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var indexCalculateConfig = ConfigFile.Deserialize<IndexCalculateConfig>(configFile);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            //indexCalculateConfig.IndexCalculationDuration = TimeSpan.FromSeconds(20);
            //indexCalculateConfig.SetTypeOfFreqScale("Octave");

            var results = IndexCalculate.Analysis(
                new AudioRecording(sourceRecording),
                TimeSpan.Zero,
                indexCalculateConfig.IndexProperties,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var summaryIndices = results.SummaryIndexValues;

            Assert.AreEqual(0.6793287, summaryIndices.AcousticComplexity, AllowedDelta);
            Assert.AreEqual(0.484520, summaryIndices.Activity, AllowedDelta);
            Assert.AreEqual(0.000000, summaryIndices.AvgEntropySpectrum, AllowedDelta);
            Assert.AreEqual(-30.946519, summaryIndices.AvgSignalAmplitude, AllowedDelta);
            Assert.AreEqual(11.533420, summaryIndices.AvgSnrOfActiveFrames, AllowedDelta);
            Assert.AreEqual(-39.740775, summaryIndices.BackgroundNoise, AllowedDelta);
            Assert.AreEqual(21, summaryIndices.ClusterCount);
            Assert.AreEqual(0.153191, summaryIndices.EntropyOfAverageSpectrum, AllowedDelta);
            Assert.AreEqual(0.301929, summaryIndices.EntropyOfCoVSpectrum, AllowedDelta);
            Assert.AreEqual(0.260999, summaryIndices.EntropyOfPeaksSpectrum, AllowedDelta);
            Assert.AreEqual(0.522080, summaryIndices.EntropyOfVarianceSpectrum, AllowedDelta);
            Assert.AreEqual(0.0, summaryIndices.EntropyPeaks, AllowedDelta);
            Assert.AreEqual(2.0, summaryIndices.EventsPerSecond, AllowedDelta);
            Assert.AreEqual(0.140306, summaryIndices.HighFreqCover, AllowedDelta);
            Assert.AreEqual(0.137873, summaryIndices.MidFreqCover, AllowedDelta);
            Assert.AreEqual(0.055341, summaryIndices.LowFreqCover, AllowedDelta);
            Assert.AreEqual(0.957433, summaryIndices.Ndsi, AllowedDelta);
            Assert.AreEqual(27.877206, summaryIndices.Snr, AllowedDelta);
            Assert.AreEqual(6.240310, summaryIndices.SptDensity, AllowedDelta);
            Assert.AreEqual(0, summaryIndices.ResultStartSeconds);
            Assert.AreEqual(0.162216, summaryIndices.TemporalEntropy, AllowedDelta);
            Assert.AreEqual(401, summaryIndices.ThreeGramCount, AllowedDelta);
        }

        /// <summary>
        /// Test the various spectral indices
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices()
        {
            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
            var sourceRecording = PathHelper.ResolveAsset(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");
<<<<<<< HEAD
            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
=======
>>>>>>> master

            // var outputDir = this.outputDirectory;
            var resourcesDir = PathHelper.ResolveAssetPath("Indices");

            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var indexCalculateConfig = ConfigFile.Deserialize<IndexCalculateConfig>(configFile);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            //indexCalculateConfig.IndexCalculationDuration = TimeSpan.FromSeconds(20);
            //indexCalculateConfig.SetTypeOfFreqScale("Octave");

            var results = IndexCalculate.Analysis(
                new AudioRecording(sourceRecording),
                TimeSpan.Zero,
                indexCalculateConfig.IndexProperties,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            // After serialising the expected vector and writing to the resources directory, comment the Binary.Serialise line.

            // ACI
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\ACI.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ACI);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.ACI, AllowedDelta);

            // BGN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\BGN.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.BGN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.BGN, AllowedDelta);

            // CVR
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\CVR.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.CVR);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.CVR, AllowedDelta);

            // ENT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\ENT.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ENT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.ENT, AllowedDelta);

            // EVN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\EVN.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.EVN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.EVN, AllowedDelta);

<<<<<<< HEAD
            // OSC
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\OSC.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralIndices.OSC);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.OSC, AllowedDelta);

            // PMN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\PMN.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.PMN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.PMN, AllowedDelta);

            // POW
            //expectedSpectrumFile = new FileInfo(resourcesDir + "\\POW.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.POW);
            //expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            //CollectionAssert.That.AreEqual(expectedVector, spectralIndices.POW, AllowedDelta);

            // R3D
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\R3D.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.R3D);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.R3D, AllowedDelta);
=======
            // PMN - power minus noise
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\PMN.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.PMN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.PMN, AllowedDelta);
>>>>>>> master

            // RHZ
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RHZ.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RHZ);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.RHZ, AllowedDelta);

            // RNG
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RNG.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RNG);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.RNG, AllowedDelta);

            // RPS
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RPS.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RPS);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.RPS, AllowedDelta);

            // RVT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RVT.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RVT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.RVT, AllowedDelta);

            // SPT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\SPT.bin");

            // Binary.Serialize(expectedSpectrumFile, spectralIndices.SPT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.SPT, AllowedDelta);

            var outputImagePath = Path.Combine(this.outputDirectory.FullName, "SpectralIndices.png");
            var image = SpectralIndexValues.CreateImageOfSpectralIndices(spectralIndices);
            image.Save(outputImagePath);
        }

        /// <summary>
        /// Test index calculation when the IndexCalculationDuration= 20 seconds & subsegmentOffsetTimeSpan = 40seconds
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices_ICD20()
        {
            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
            var sourceRecording = PathHelper.ResolveAsset(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");
<<<<<<< HEAD
            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
=======
>>>>>>> master

            // var outputDir = this.outputDirectory;
            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var recording = new AudioRecording(sourceRecording);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            var indexCalculateConfig = ConfigFile.Deserialize<IndexCalculateConfig>(configFile);
            indexCalculateConfig.IndexCalculationDurationTimeSpan = TimeSpan.FromSeconds(20);

            var results = IndexCalculate.Analysis(
                recording,
                TimeSpan.FromSeconds(40), // assume that this is the third of three 20 second subsegments
                indexCalculateConfig.IndexProperties,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            var resourcesDir = PathHelper.ResolveAssetPath("Indices");
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\BGN_ICD20.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralIndices.BGN);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.BGN, AllowedDelta);

            expectedSpectrumFile = new FileInfo(resourcesDir + "\\CVR_ICD20.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralIndices.CVR);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.CVR, AllowedDelta);

            var outputImagePath1 = Path.Combine(this.outputDirectory.FullName, "SpectralIndices_ICD20.png");
            var image = SpectralIndexValues.CreateImageOfSpectralIndices(spectralIndices);
            image.Save(outputImagePath1);
        }

        /// <summary>
        /// Test index calculation when the Hertz FreqScaleType = Octave.
        /// Only test the BGN spectral index as reasonable to assume that the rest will work if ACI works.
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices_Octave()
        {
            // create a two-minute artificial recording containing five harmonics.
            int sampleRate = 64000;
            double duration = 120; // signal duration in seconds
            int[] harmonics = { 500, 1000, 2000, 4000, 8000 };
            var recording = DspFilters.GenerateTestRecording(sampleRate, duration, harmonics, WaveType.Sine);

            // cut out one minute from 30 - 90 seconds and incorporate into AudioRecording
            int startSample = sampleRate * 30; // start two minutes into recording
            int subsegmentSampleCount = sampleRate * 60; // get 60 seconds
            double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, startSample, subsegmentSampleCount);
            var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, sampleRate);
            var subsegmentRecording = new AudioRecording(wr);

            //var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");

            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            var indexCalculateConfig = ConfigFile.Deserialize<IndexCalculateConfig>(configFile);
            indexCalculateConfig.FrequencyScale = FreqScaleType.Octave;

            var freqScale = new FrequencyScale(indexCalculateConfig.FrequencyScale);
            indexCalculateConfig.FrameLength = freqScale.WindowSize;

            var results = IndexCalculate.Analysis(
                subsegmentRecording,
                TimeSpan.Zero,
                indexCalculateConfig.IndexProperties,
                sampleRate,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // draw the output image of all spectral indices
            var outputImagePath1 = Path.Combine(this.outputDirectory.FullName, "SpectralIndices_Octave.png");
            var image = SpectralIndexValues.CreateImageOfSpectralIndices(spectralIndices);
            image.Save(outputImagePath1);

            // TEST the BGN SPECTRAL INDEX
            Assert.AreEqual(256, spectralIndices.BGN.Length);

            var resourcesDir = PathHelper.ResolveAssetPath("Indices");
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\BGN_OctaveScale.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralIndices.BGN);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.BGN, AllowedDelta);

            expectedSpectrumFile = new FileInfo(resourcesDir + "\\CVR_OctaveScale.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralIndices.CVR);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralIndices.CVR, AllowedDelta);
        }
    }
}
