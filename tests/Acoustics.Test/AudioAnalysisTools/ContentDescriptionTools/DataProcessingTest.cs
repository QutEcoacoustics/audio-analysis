// <copyright file="DataProcessingTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools.Indices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// tests methods in DataProcessing class used for Content Description.
    /// </summary>
    [TestClass]
    public class DataProcessingTest
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

        [TestMethod]
        public void TestReadIndexMatrices()
        {
            //var testSpectra = PathHelper.ResolveAssetPath("20160725_203006_continuous1__Towsey.Acoustic.ACI.csv");

            //var matrix = IndexMatrices.ReadSpectrogram(testSpectra.ToFileInfo(), out var binCount);

            //Assert.AreEqual(30, matrix.GetLength(0));
            //Assert.AreEqual(256, matrix.GetLength(1));
            //Assert.AreEqual(256, binCount);

            //Assert.AreEqual(0.462924678189025, matrix[0, 0]);
            //Assert.AreEqual(0.42779069684277, matrix[0, 1]);
            //Assert.AreEqual(0.46412042529103, matrix[1, 0]);
            //Assert.AreEqual(0.444650614488611, matrix[1, 1]);
        }

        [TestMethod]
        public void TestGetIndicesForOneMinute()
        {
        }

        [TestMethod]
        public void TestScanSpectrumWithTemplate()
        {
        }

        [TestMethod]
        public void TestGetFreqBinVector()
        {
        }
    }
}
