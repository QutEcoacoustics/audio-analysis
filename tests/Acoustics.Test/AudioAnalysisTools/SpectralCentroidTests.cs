// <copyright file="SpectralCentroidTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for methods to do with spectral centroids.
    /// </summary>
    [TestClass]
    public class SpectralCentroidTests
    {
        /// <summary>
        /// The canonical recording used for this recognizer is a 31 second recording made by Yvonne Phillips at Gympie National Park, 2015-08-18.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "gympie_np_1192_331618_20150818_054959_31_0.wav");

        [TestMethod]
        public void TestCalculateSpectralCentroid()
        {
            // set up the spectrum and nyquist
            int nyquist = 11025;
            double[] spectrum1 = { 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0 };
            double[] spectrum2 = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
            double[] spectrum3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.0, 1.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] spectrum4 = { 1.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.0 };

            // frequency bin width = 11025 / 32 = 344.53125
            // Half bin width = 172.2656 Hz.
            // A normalised freq bin has width = 0.03125
            var centroid1 = SpectralCentroid.CalculateSpectralCentroid(spectrum1, nyquist);
            var centroid2 = SpectralCentroid.CalculateSpectralCentroid(spectrum2, nyquist);
            var centroid3 = SpectralCentroid.CalculateSpectralCentroid(spectrum3, nyquist);
            var centroid4 = SpectralCentroid.CalculateSpectralCentroid(spectrum4, nyquist);

            Assert.AreEqual(0.515625, centroid1);
            Assert.AreEqual(0.5, centroid2);
            Assert.AreEqual(0.5, centroid3);
            Assert.AreEqual(0.5, centroid4);
        }

        [TestMethod]
        public void TestCalculateSpectralCentroids()
        {
            // set up the spectrum and nyquist
            int nyquist = 11025;
            double[] spectrum1 = { 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0 };
            double[] spectrum2 = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
            double[] spectrum3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.0, 1.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] spectrum4 = { 1.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.0 };

            var frameCount = 4;
            var binCount = 32;
            var matrixData = new double[frameCount, binCount];
            MatrixTools.SetRow(matrixData, 0, spectrum1);
            MatrixTools.SetRow(matrixData, 1, spectrum2);
            MatrixTools.SetRow(matrixData, 2, spectrum3);
            MatrixTools.SetRow(matrixData, 3, spectrum4);

            var centroids = SpectralCentroid.CalculateSpectralCentroids(matrixData, nyquist);
            double[] expectedArray = { 0.515625, 0.5, 0.5, 0.5 };

            CollectionAssert.AreEqual(expectedArray, centroids);
        }

        [TestMethod]
        public void TestCalculateSpectralCentroidsInOneSecondBlocks()
        {
            // set up the spectrum and nyquist
            double[] centroidArray = { 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0, 0, 1.0 };
            double framesPerSecond = 4.0;
            var centroids = SpectralCentroid.AverageSpectralCentroidsInOneSecondSegments(centroidArray, framesPerSecond);
            double[] expectedArray1 = { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };
            CollectionAssert.AreEqual(expectedArray1, centroids);

            framesPerSecond = 4.2;
            centroids = SpectralCentroid.AverageSpectralCentroidsInOneSecondSegments(centroidArray, framesPerSecond);
            double[] expectedArray2 = { 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.66666666666666663 };

            CollectionAssert.AreEqual(expectedArray2, centroids);
        }

        [TestMethod]
        public void TestCalculateSpectralCentroidsInOneSecondBlocksOnRealRecording()
        {
            var recording = new WavReader(TestAsset);
            var config = new SpectrogramSettings();
            var amplitudeSpectrogram = new AmplitudeSpectrogram(config, recording);

            var centroids = SpectralCentroid.CalculateSpectralCentroidsInOneSecondSegments(amplitudeSpectrogram);
            var length = centroids.Length;
            Assert.AreEqual(31, length);

            var centroid1 = centroids[length / 2];
            var centroid2 = centroids[length - 1];

            var delta = TestHelper.AllowedDelta;
            Assert.AreEqual(0.33138923037601808, centroids[0], delta);
            Assert.AreEqual(0.32098870879909, centroid1, delta);
            Assert.AreEqual(0.32775708863777775, centroid2, delta);

            //Assert.IsNull(scoreTrack);
        }
    }
}
