// <copyright file="ClusterIndexTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// tests the clustering algorithm used to calculate the summary indices Clustercount and TrigramCount.
    /// </summary>
    [TestClass]
    public class ClusterIndexTest
    {
        private DirectoryInfo outputDirectory;

        public ClusterIndexTest()
        {
            // setup logic here
        }

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
        /// If you want to see images of the clusters etc, etc, for debugging purposes, then call TEST METHOD
        /// SpectalClustering.TESTMETHOD_SpectralClustering() from the Sandpit class.
        /// </summary>
        [TestMethod]
        public void TestBinaryClusteringOfSpectra()
        {
            var wavFilePath = PathHelper.ResolveAsset(@"Recordings", "BAC2_20071008-085040.wav");

            // var outputDir = this.outputDirectory;

            int frameSize = 512;
            var recording = new AudioRecording(wavFilePath); // get recording segment

            // for deriving binary spectrogram
            double binaryThreshold = SpectralClustering.DefaultBinaryThresholdInDecibels; // A decibel threshold for post noise removal
            double[,] spectrogramData = SpectralClustering.GetDecibelSpectrogramNoiseReduced(recording, frameSize);

            // We only use the midband of the Spectrogram, i.e. the band between lowerBinBound and upperBinBound.
            // In June 2016, the mid-band was set to lowerBound=1000Hz, upperBound=8000hz, because this band contains most bird activity, i.e. it is the Bird-Band
            // This was done in order to make the cluster summary indices more reflective of bird call activity.
            int freqBinCount = spectrogramData.GetLength(1);
            double binWidth = recording.Nyquist / (double)freqBinCount;
            int lowerFreqBound = 1000;
            int lowerBinBound = (int)Math.Ceiling(lowerFreqBound / binWidth);
            int upperFreqBound = 8000;
            int upperBinBound = (int)Math.Ceiling(upperFreqBound / binWidth);

            var midBandSpectrogram = MatrixTools.Submatrix(spectrogramData, 0, lowerBinBound, spectrogramData.GetLength(0) - 1, upperBinBound);
            var clusterInfo = SpectralClustering.ClusterTheSpectra(midBandSpectrogram, lowerBinBound, upperBinBound, binaryThreshold);

            // transfer cluster info to spectral index results
            var clusterSpectrum = SpectralClustering.RestoreFullLengthSpectrum(clusterInfo.ClusterSpectrum, freqBinCount, lowerBinBound);

            // test the cluster count - also called spectral diversity in some papers
            Assert.AreEqual(clusterInfo.ClusterCount, 17);

            // test the trigram count - another way of thinking about spectral change
            Assert.AreEqual(clusterInfo.TriGramUniqueCount, 342);

            // test what used to be the CLS spectral index. Sum of the rows of the weight vectors.
            var expectedSpectrumFile = PathHelper.ResolveAsset("BinaryClustering", "clusterSpectrum.bin");

            // Binary.Serialize(expectedSpectrumFile, clusterSpectrum);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.AreEqual(expectedVector, clusterSpectrum);
        }
    }
}
