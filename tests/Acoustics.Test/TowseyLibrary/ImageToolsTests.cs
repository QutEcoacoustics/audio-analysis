// <copyright file="ImageToolsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TowseyLibrary
{
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;

    [TestClass]
    public class ImageToolsTests
    {
        [TestMethod]
        public void TestCombineImagesVertically()
        {
            var actual = ImageTools.CombineImagesVertically(
                null,
                Drawing.NewImage(100, 100, Color.Red),
                Drawing.NewImage(100, 20, Color.Red),
                Drawing.NewImage(100, 200, Color.Red),
                null,
                Drawing.NewImage(100, 20, Color.Red),
                null);

            Assert.That.ImageIsSize(100, 340, actual);
            Assert.That.ImageRegionIsColor(actual.Bounds(), Color.Red, actual);
        }

        [TestMethod]
        public void TestCombineImagesInLine()
        {
            var actual = ImageTools.CombineImagesInLine(
                null,
                Drawing.NewImage(10, 100, Color.Red),
                Drawing.NewImage(100, 100, Color.Red),
                Drawing.NewImage(20, 100, Color.Red),
                null,
                Drawing.NewImage(200, 100, Color.Red),
                null);

            Assert.That.ImageIsSize(330, 100, actual);
            Assert.That.ImageRegionIsColor(actual.Bounds(), Color.Red, actual);
        }

        [TestMethod]
        public void TestCombineImagesVerticallyDefaultFill()
        {
            var actual = ImageTools.CombineImagesVertically(
                null,
                Drawing.NewImage(100, 100, Color.Red),
                Drawing.NewImage(80, 100, Color.Red),
                Drawing.NewImage(100, 100, Color.Red),
                null,
                Drawing.NewImage(80, 100, Color.Red),
                null);

            Assert.That.ImageIsSize(100, 400, actual);
            Assert.That.ImageRegionIsColor((0, 0, 80, 400).AsRect(), Color.Red, actual);

            Assert.That.ImageRegionIsColor((80, 0, 20, 100).AsRect(), Color.Red, actual);
            Assert.That.ImageRegionIsColor((80, 100, 20, 100).AsRect(), Color.DarkGray, actual);
            Assert.That.ImageRegionIsColor((80, 200, 20, 100).AsRect(), Color.Red, actual);
            Assert.That.ImageRegionIsColor((80, 300, 20, 100).AsRect(), Color.DarkGray, actual);
        }

        [TestMethod]
        public void TestCombineImagesInLineDefaultFill()
        {
            var actual = ImageTools.CombineImagesInLine(
                null,
                Drawing.NewImage(100, 80, Color.Red),
                Drawing.NewImage(100, 100, Color.Red),
                Drawing.NewImage(100, 80, Color.Red),
                null,
                Drawing.NewImage(100, 100, Color.Red),
                null);

            Assert.That.ImageIsSize(400, 100, actual);
            Assert.That.ImageRegionIsColor((0, 0, 400, 80).AsRect(), Color.Red, actual);

            Assert.That.ImageRegionIsColor((0, 80, 100, 20).AsRect(), Color.Black, actual);
            Assert.That.ImageRegionIsColor((100, 80, 100, 20).AsRect(), Color.Red, actual);
            Assert.That.ImageRegionIsColor((200, 80, 100, 20).AsRect(), Color.Black, actual);
            Assert.That.ImageRegionIsColor((300, 80, 100, 20).AsRect(), Color.Red, actual);
        }
    }
}