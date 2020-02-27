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
    public class DrawingTests : GeneratedImageTest<Rgb24>
    {
        // one pixel should fill the line below the coordinate
        private static readonly Pen TestPenOne = new Pen(Color.Red, 1f);

        // two pixels should straddle coordinates
        private static readonly Pen TestPenTwo = new Pen(Color.Red, 2f);

        // three pixels should fill 2px line below, 1px above
        private static readonly Pen TestPenThree = new Pen(Color.Red, 3f);
        private readonly TestImage blankExpected;

        public DrawingTests()
            : base()
        {
            this.blankExpected = new TestImage(100, 100, Color.Black);
        }

        [TestMethod]
        public void TestNoAADrawLine1Px()
        {
            // red line at top, 100px wide
            this.Expected = this.blankExpected.FillPattern("R100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle1Px()
        {
            // red line, 50px down, 100px wide
            this.Expected = this.blankExpected.FillPattern("⬇50\nR100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLine2Px()
        {
            // red line at top, 1px high (because line spills over top border, 1px above, 1 below), 100px wide
            this.Expected = this.blankExpected.FillPattern("R100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle2Px()
        {
            // red line, 49px down, 2px high, 100px wide
            this.Expected = this.blankExpected.FillPattern("⬇49\n2×R100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLine3Px()
        {
            // red line at top, 2px high (because line spills over top border, 1px above, 2 below), 100px wide
            this.Expected = this.blankExpected.FillPattern("2×R100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle3Px()
        {
            // red line, 49px down, 2px high, 100px wide
            this.Expected = this.blankExpected.FillPattern("⬇49\n3×R100").Finish();

            this.Actual.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }
    }
}
