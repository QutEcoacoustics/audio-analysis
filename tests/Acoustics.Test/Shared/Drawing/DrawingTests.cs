// <copyright file="DrawingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class DrawingTests : OutputDirectoryTest
    {
        // one pixel should fill the line below the coordinate
        private static readonly Pen TestPenOne = new Pen(Color.Red, 1f);

        // two pixels should straddle coordinates
        private static readonly Pen TestPenTwo = new Pen(Color.Red, 2f);

        // three pixels should fill 2px line below, 1px above
        private static readonly Pen TestPenThree = new Pen(Color.Red, 3f);

        private Image<Rgb24> testImage;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestSetup()
        {
            this.testImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var path = this.outputDirectory.CombineFile(this.TestContext.TestName + ".png").FullName;
            this.testImage.Save(path);
            this.TestContext.AddResultFile(path);
        }

        [TestMethod]
        public void TestNoAADrawLine1Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 0, 100, 0));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 1), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 1, 100, 99), Color.Black, this.testImage);
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle1Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 50, 100, 50));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 50, 100, 1), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 50), Color.Black, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 51, 100, 49), Color.Black, this.testImage);
        }

        [TestMethod]
        public void TestNoAADrawLine2Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 0, 100, 0));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 1), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 1, 100, 99), Color.Black, this.testImage);
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle2Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 50, 100, 50));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 49, 100, 2), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 49), Color.Black, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 51, 100, 49), Color.Black, this.testImage);
        }

        [TestMethod]
        public void TestNoAADrawLine3Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 0, 100, 0));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 2), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 2, 100, 98), Color.Black, this.testImage);
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle3Px()
        {
            this.testImage.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 50, 100, 50));

            Assert.That.ImageRegionIsColor(new Rectangle(0, 49, 100, 3), Color.Red, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, 100, 49), Color.Black, this.testImage);
            Assert.That.ImageRegionIsColor(new Rectangle(0, 52, 100, 48), Color.Black, this.testImage);
        }
    }
}
