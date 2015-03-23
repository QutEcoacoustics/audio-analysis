// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TilerTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
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

    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.TileImage;

    using EcoSounds.Mvc.Tests;

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
                60.0, 
                1440, 
                new SortedSet<double>() { 1, 1, 1, 1, 1, 1 }, 
                1.0, 
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

            var xScales = new[] { 60.0, 24, 12, 6, 2, 1 };
            var xNormalScales = new[] { 1.0, 2.5, 5, 10, 30, 60 };
            var numberOfTiles = new[] { 6, 12, 24, 48, 144, 288 };
            var layerWidths = new[] { 1440, 300 * 12, 300 * 24, 300 * 48, 300 * 144, 300 * 288 };
            var i = 0;
            foreach (var layer in layers)
            {
                Assert.AreEqual(numberOfTiles[i], layer.XTiles);
                Assert.AreEqual(1, layer.YTiles);
                Assert.AreEqual(layerWidths[i], layer.Width);
                Assert.AreEqual(300, layer.Height);
                Assert.AreEqual(xScales[i], layer.XScale);
                Assert.AreEqual(1.0, layer.YScale);
                Assert.AreEqual(i, layer.ScaleIndex);
                Assert.AreEqual(xNormalScales[i], layer.XNormalizedScale);
                Assert.AreEqual(1.0, layer.YNormalizedScale);

                i++;
            }
        }

        [TestMethod]
        public void Test60Resolution()
        {
            var testBitmap = new Bitmap("1440px.png");
            var superTile = new SuperTile()
                                {
                                    Image = testBitmap,
                                    Scale = TimeSpan.FromSeconds(60.0),
                                    TimeOffset = TimeSpan.Zero
                                };
            this.tiler.Tile(superTile, superTile);

            ////Debug.WriteLine(this.outputDirectory.FullName);
            ////Debug.WriteLine(this.outputDirectory.GetFiles().Length);
            var producedFiles = this.outputDirectory.GetFiles();

            Assert.AreEqual(6, producedFiles.Length);
        }
    }
}