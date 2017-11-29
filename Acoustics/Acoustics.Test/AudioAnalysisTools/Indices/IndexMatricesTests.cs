﻿// <copyright file="IndexMatricesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Indices
{
    using System;
    using System.Linq;

    using Acoustics.Shared.Csv;
    using Acoustics.Shared.Extensions;
    using global::AudioAnalysisTools.Indices;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    using Zio;

    [TestClass]
    public class IndexMatricesTests : OutputDirectoryTest
    {
        [TestMethod]
        public void TestReadSpectrogram()
        {
            var testSpectra = PathHelper.ResolveAssetPath("20160725_203006_continuous1__Towsey.Acoustic.ACI.csv");

            var matrix = IndexMatrices.ReadSpectrogram(testSpectra.ToFileInfo(), out var binCount);

            Assert.AreEqual(30, matrix.GetLength(0));
            Assert.AreEqual(256, matrix.GetLength(1));
            Assert.AreEqual(256, binCount);

            Assert.AreEqual(0.462924678189025, matrix[0, 0]);
            Assert.AreEqual(0.42779069684277, matrix[0, 1]);
            Assert.AreEqual(0.46412042529103, matrix[1, 0]);
            Assert.AreEqual(0.444650614488611, matrix[1, 1]);
        }

        [TestMethod]
        public void TestReadSpectralIndices()
        {
            var testSpectra = PathHelper.ResolveAssetPath("20160725_203006_continuous1__Towsey.Acoustic.ACI.csv");

            var dir = PathHelper.TestResources.ToDirectoryEntry();

            var matrix = IndexMatrices.ReadSpectralIndices(
                    dir,
                    "20160725_203006_continuous1",
                    "Towsey.Acoustic",
                    new[] { "ACI" })
                .Single()
                .Value;

            Assert.AreEqual(256, matrix.GetLength(0));
            Assert.AreEqual(30, matrix.GetLength(1));

            Assert.AreEqual(0.462924678189025, matrix[255, 0]);
            Assert.AreEqual(0.42779069684277, matrix[254, 0]);
            Assert.AreEqual(0.46412042529103, matrix[255, 1]);
            Assert.AreEqual(0.444650614488611, matrix[254, 1]);

            var matrix2 = IndexMatrices.ReadSpectrogram(testSpectra.ToFileInfo(), out var binCount);
            matrix2 = MatrixTools.MatrixRotate90Anticlockwise(matrix2);

            var actualEnumerator = matrix2.GetEnumerator();
            foreach (var expected in matrix)
            {
                actualEnumerator.MoveNext();

                Assert.AreEqual(expected, (double)actualEnumerator.Current, 1E-14, $"delta: {expected - (double)actualEnumerator.Current}");
            }
        }

        [TestMethod]
        public void TestWriteReadSpectrogram()
        {
            var random = TestHelpers.Random.GetRandom();
            var testSpectra = random.NextMatrix(100, 50);

            var testFile = this.outputDirectory.CombineFile("test.matrix.csv");
            Csv.WriteMatrixToCsv(testFile, testSpectra);

            var matrix = IndexMatrices.ReadSpectrogram(testFile, out var binCount);

            Assert.AreEqual(100, matrix.GetLength(0));
            Assert.AreEqual(50, matrix.GetLength(1));
            Assert.AreEqual(50, binCount);

            var actualEnumerator = matrix.GetEnumerator();
            foreach (var expected in testSpectra)
            {
                actualEnumerator.MoveNext();

                Assert.AreEqual(expected, (double)actualEnumerator.Current, 1E-14, $"delta: {expected - (double)actualEnumerator.Current}");
            }
        }
    }
}
