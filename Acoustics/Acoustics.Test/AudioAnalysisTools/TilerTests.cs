// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TilerTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the TilerTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    using EcoSounds.Mvc.Tests;

    using global::AudioAnalysisTools.TileImage;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Note the files in /TestResources are copied 
    /// automagically by using a build step.
    /// </summary>
    [TestClass]
    public class TilerTests
    {
        private PanoJsTilingProfile tilingProfile;
        private Tiler tiler;
        private DirectoryInfo outputDirectory;

        [TestInitialize]
        public void Setup()
        {
            this.tilingProfile = new PanoJsTilingProfile();

            this.outputDirectory = TestHelper.GetTempDir();

            this.tiler = new Tiler(
                this.outputDirectory,
                this.tilingProfile,
                new SortedSet<double>() { 60.0, 24, 12, 6, 2, 1 },
                new SortedSet<double>() { 1, 1, 1, 1, 1, 1 },
                60.0,
                1440,
                300);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestCalculatedLayers()
        {
            var layers = this.tiler.CalculatedLayers;

            Assert.AreEqual(6, layers.Count);

            var numberOfTiles = new double[] { 6,  12, 24, 48, 144, 288};
            var i = 0;
            foreach (var layer in layers)
            {
                Assert.AreEqual(numberOfTiles[i], layer.XTiles);
                Assert.AreEqual(1, layer.YTiles);

                i++;
            }

        }

        [TestMethod]
        public void Test60Resolution()
        {
            var testBitmap = new Bitmap("1440px.png");

            this.tiler.Tile(testBitmap, 60.0, new Point(0, 0));

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);

            var producedFiles = this.outputDirectory.GetFiles();
            
            Assert.AreEqual(6, producedFiles.Length);
        }
    }
}
