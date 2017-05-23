// <copyright file="IndexCalculateTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    // using TestHelpers;

    /// <summary>
    /// Test methods for the various standard Sonograms or Spectrograms
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
            var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");

            // var outputDir = this.outputDirectory;
            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var indexCalculateConfig = IndexCalculateConfig.GetConfig(configFile);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            //indexCalculateConfig.IndexCalculationDuration = TimeSpan.FromSeconds(20);
            //indexCalculateConfig.SetTypeOfFreqScale("Octave");

            var results = IndexCalculate.Analysis(
                new AudioRecording(sourceRecording),
                TimeSpan.Zero,
                indexPropertiesConfig,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var summaryIndices = results.SummaryIndexValues;

            Assert.AreEqual(0.6793287, summaryIndices.AcousticComplexity, 0.000001);
            Assert.AreEqual(0.484520, summaryIndices.Activity, 0.000001);
            Assert.AreEqual(0.000000, summaryIndices.AvgEntropySpectrum, 0.000001);
            Assert.AreEqual(-30.946519, summaryIndices.AvgSignalAmplitude, 0.000001);
            Assert.AreEqual(11.533420, summaryIndices.AvgSnrOfActiveFrames, 0.000001);
            Assert.AreEqual(-39.740775, summaryIndices.BackgroundNoise, 0.000001);
            Assert.AreEqual(21, summaryIndices.ClusterCount);
            Assert.AreEqual(0.153191, summaryIndices.EntropyOfAverageSpectrum, 0.000001);
            Assert.AreEqual(0.301929, summaryIndices.EntropyOfCoVSpectrum, 0.000001);
            Assert.AreEqual(0.260999, summaryIndices.EntropyOfPeaksSpectrum, 0.000001);
            Assert.AreEqual(0.522080, summaryIndices.EntropyOfVarianceSpectrum, 0.000001);
            Assert.AreEqual(0.0, summaryIndices.EntropyPeaks, 0.000001);
            Assert.AreEqual(2.0, summaryIndices.EventsPerSecond, 0.000001);
            Assert.AreEqual(0.140306, summaryIndices.HighFreqCover, 0.000001);
            Assert.AreEqual(0.137873, summaryIndices.MidFreqCover, 0.000001);
            Assert.AreEqual(0.055341, summaryIndices.LowFreqCover, 0.000001);
            Assert.AreEqual(0.957433, summaryIndices.Ndsi, 0.000001);
            Assert.AreEqual(27.877206, summaryIndices.Snr, 0.000001);
            Assert.AreEqual(6.240310, summaryIndices.SptDensity, 0.000001);
            Assert.AreEqual(TimeSpan.Zero, summaryIndices.StartOffset);
            Assert.AreEqual(0.162216, summaryIndices.TemporalEntropy, 0.000001);
            Assert.AreEqual(401, summaryIndices.ThreeGramCount, 0.000001);
        }

        /// <summary>
        /// Test the various spectral indices
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices()
        {
            var sourceRecording = PathHelper.ResolveAsset(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");
            var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");

            // var outputDir = this.outputDirectory;
            var resourcesDir = PathHelper.ResolveAssetPath("Indices");

            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var indexCalculateConfig = IndexCalculateConfig.GetConfig(configFile);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            //indexCalculateConfig.IndexCalculationDuration = TimeSpan.FromSeconds(20);
            //indexCalculateConfig.SetTypeOfFreqScale("Octave");

            var results = IndexCalculate.Analysis(
                new AudioRecording(sourceRecording),
                TimeSpan.Zero,
                indexPropertiesConfig,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            // After serialising the expected vector, comment the Binary.Serialise line and copy file to dir TestResources\Indices.

            // ACI
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\ACI.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ACI);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ACI);

            // BGN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\BGN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.BGN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.BGN);

            // CVR
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\CVR.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.CVR);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.CVR);

            // DMN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\PMN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.PMN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.PMN);

            // ENT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\ENT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ENT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ENT);

            // EVN
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\EVN.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.EVN);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.EVN);

            // POW
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\POW.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.POW);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.POW);

            // RHZ
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RHZ.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RHZ);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RHZ);

            // RNG
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RNG.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RNG);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RNG);

            // RPS
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RPS.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RPS);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RPS);

            // RVT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\RVT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.RVT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.RVT);

            // R3D
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\R3D.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.R3D);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.R3D);

            // SPT
            expectedSpectrumFile = new FileInfo(resourcesDir + "\\SPT.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.SPT);
            expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.SPT);
        }

        /// <summary>
        /// Test index calculation when the IndexCalculationDuration= 20 seconds & subsegmentOffsetTimeSpan = 40seconds
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices_ICD20()
        {
            var sourceRecording = PathHelper.ResolveAsset(@"Recordings\BAC2_20071008-085040.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.Acoustic.yml");
            var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesConfig.yml");
            var resourcesDir = PathHelper.ResolveAssetPath("Indices");

            // var outputDir = this.outputDirectory;
            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            var recording = new AudioRecording(sourceRecording);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            var indexCalculateConfig = IndexCalculateConfig.GetConfig(configFile);
            indexCalculateConfig.IndexCalculationDuration = TimeSpan.FromSeconds(20);

            var results = IndexCalculate.Analysis(
                recording,
                TimeSpan.FromSeconds(40), // assume that this is the third of three 20 second subsegments
                indexPropertiesConfig,
                22050,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            // After serialising the expected vector, comment the Binary.Serialise line and copy file to dir TestResources\Indices.

            // ACI
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\ACI_ICD20.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ACI);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ACI);
        }

        /// <summary>
        /// Test index calculation when the Herz FreqScaleType = Octave.
        /// Only test the ACI spectral index as reasonable to assume that the rest will work if ACI works.
        /// </summary>
        [TestMethod]
        public void TestOfSpectralIndices_Octave()
        {
            var sourceRecording = PathHelper.ResolveAsset(@"C:\SensorNetworks\WavFiles\MarineRecordings\JascoGBR\AMAR119-00000139.00000139.Chan_1-24bps.1375012796.2013-07-28-11-59-56-16bit.wav");
            var configFile = PathHelper.ResolveConfigFile(@"Towsey.AcousticMarine.yml");
            var indexPropertiesConfig = PathHelper.ResolveConfigFile(@"IndexPropertiesMarineConfig.yml");
            var resourcesDir = PathHelper.ResolveAssetPath("Indices");

            // var outputDir = this.outputDirectory;
            // Create temp directory to store output
            if (!this.outputDirectory.Exists)
            {
                this.outputDirectory.Create();
            }

            int smapleRateOfOriginalRecording = 64000;
            int startSample = smapleRateOfOriginalRecording * 120; // start two minutews into recording
            int subsegmentSampleCount = smapleRateOfOriginalRecording * 60; // get the third minute
            var recording = new AudioRecording(sourceRecording);
            double[] subsamples = DataTools.Subarray(recording.WavReader.Samples, startSample, subsegmentSampleCount);
            var wr = new Acoustics.Tools.Wav.WavReader(subsamples, 1, 16, smapleRateOfOriginalRecording);
            var subsegmentRecording = new AudioRecording(wr);
            //string path = @"C:\SensorNetworks\WavFiles\temp\tempfile.wav";
            //WavWriter.Write16bitWavFile(wr.Samples, smapleRateOfOriginalRecording, path);

            // CHANGE CONFIG PARAMETERS HERE IF REQUIRED
            var indexCalculateConfig = IndexCalculateConfig.GetConfig(configFile);
            indexCalculateConfig.SetTypeOfFreqScale("Octave");

            var results = IndexCalculate.Analysis(
                subsegmentRecording,
                TimeSpan.Zero,
                indexPropertiesConfig,
                smapleRateOfOriginalRecording,
                TimeSpan.Zero,
                indexCalculateConfig,
                returnSonogramInfo: true);

            var spectralIndices = results.SpectralIndexValues;

            // TEST the SPECTRAL INDICES
            // After serialising the expected vector, comment the Binary.Serialise line and copy file to dir TestResources\Indices.

            // ACI
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\ACI_OctaveScale.bin");
            // Binary.Serialize(expectedSpectrumFile, spectralIndices.ACI);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, spectralIndices.ACI);
        }
    }
}
