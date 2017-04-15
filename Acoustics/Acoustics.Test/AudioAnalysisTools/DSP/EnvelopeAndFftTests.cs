// <copyright file="ExampleTestTemplate.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

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
    public class EnvelopeAndFftTests
    {
        private DirectoryInfo outputDirectory;

        public EnvelopeAndFftTests()
        {
            // setup logic here
        }

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

        [TestMethod]
        public void TestEnvelopeAndFft()
        {
            var recording = new AudioRecording(@"Recordings\BAC2_20071008-085040.wav");
            int windowSize = 512;

            // window overlap is used only for sonograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFFTs(
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
            // energy level information
            int y = fftdata.ClipCount;
            int e = fftdata.MaxAmplitudeCount;
            double f = fftdata.MaxSignalValue;
            double g = fftdata.MinSignalValue;

            // array info
            var b = fftdata.Average;
            var a = fftdata.Envelope;
            var c = fftdata.FrameEnergy;
            var decibelsPerFrame = fftdata.FrameDecibels;

            // freq scale info
            var h = fftdata.NyquistBin;
            var i = fftdata.NyquistFreq;
            var d = fftdata.FreqBinWidth;

            Assert.AreEqual(0, 0);
            Assert.AreEqual(0.1, 0.1, double.Epsilon);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, new[] { 3, 2, 1 });

            FileEqualityHelpers.TextFileEqual(new FileInfo("data.txt"), new FileInfo("data.txt"));
            FileEqualityHelpers.FileEqual(new FileInfo("data.bin"), new FileInfo("data.bin"));

            // output initial data
            // var actualData = new[] { 1, 2, 3 };
            // Json.Serialise("data.json".ToFileInfo(), actualData);
            // Csv.WriteMatrixToCsv("data.csv".ToFileInfo(), actualData);
            // Binary.Serialize("data.bin".ToFileInfo(), actualData);
        }
    }
}
