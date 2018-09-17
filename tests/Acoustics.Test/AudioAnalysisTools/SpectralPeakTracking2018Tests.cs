// <copyright file="SpectralPeakTracking2018Tests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using global::AudioAnalysisTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class SpectralPeakTracking2018Tests
    {
        [TestMethod]
        public void GetPeakBinsIndexTest()
        {
            var inputMatrix = PathHelper.ResolveAsset("SpectralPeakTracking", "matrix1.csv");
            var matrix = Csv.ReadMatrixFromCsv<double>(inputMatrix, TwoDimensionalArray.None);
            int minFreqBin = 7;
            int maxFreqBin = 15;

            int[] expectedPeakBinsIndex =
            {
                7, 11, 12, 14, 14, 14, 14, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 9, 10, 10, 12, 14,
                14, 14, 14, 14, 6, 6, 6, 6, 6, 6, 6, 6, 8, 8, 12, 12, 14, 14, 14, 14, 14, 14, 14, 6, 6, 6, 6, 6
            };

            var actualPeakBinsIndex = SpectralPeakTracking2018.GetPeakBinsIndex(matrix, minFreqBin, maxFreqBin);

            for (int i = 0; i < expectedPeakBinsIndex.Length; i++)
            {
                Assert.AreEqual(expectedPeakBinsIndex[i], actualPeakBinsIndex[i]);
            }
        }

        [TestMethod]
        public void FindLocalSpectralPeaksTest()
        {
            var inputMatrix = PathHelper.ResolveAsset("SpectralPeakTracking", "matrix2.csv");
            var matrix = Csv.ReadMatrixFromCsv<double>(inputMatrix, TwoDimensionalArray.None);

            int[][] expectedLocalPeaksIndex = { new[] { 0, 7 }, new[] { 1, 11 } };

            int[] peakBinsIndex = new int[] { 7, 11, 6, 6, 6, 6, 6, 6, 6, 6 };

            int widthMidBand = 4;
            int topBufferSize = 2;
            int bottomBufferSize = 2;
            double threshold = 4.0;

            var actualLocalPeaks = SpectralPeakTracking2018.FindLocalSpectralPeaks(matrix, peakBinsIndex, widthMidBand,
                topBufferSize, bottomBufferSize, threshold);

            for (int i = 0; i < expectedLocalPeaksIndex.GetLength(0); i++)
            {
                Assert.AreEqual(expectedLocalPeaksIndex[i][0], actualLocalPeaks[i][0]);
                Assert.AreEqual(expectedLocalPeaksIndex[i][1], actualLocalPeaks[i][1]);
            }
        }
    }
}
