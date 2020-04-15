// <copyright file="DrawLineTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using System.Linq;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class DrawLineTest : GeneratedImageTest<Rgb24>
    {
        [TestMethod]
        [TestCategory("smoketest")]
        public void DiagonalLineNotDrawnProperly()
        {
            var color = Color.Red;
            var pen = new Pen(color, 1);

            // a 3-pixel line, bottom left to top right, 1px padding around edge
            // . . . . .
            // . . . R .
            // . . R . .
            // . R . . .
            // . . . . .
            var expected = new Image<Rgb24>(5, 5);
            expected[1, 3] = color;
            expected[2, 2] = color;
            expected[3, 1] = color;

            var actual = new Image<Rgb24>(5, 5);
            actual.Mutate(
                context => context.DrawLines(
                    new GraphicsOptions() { Antialias = false, AntialiasSubpixelDepth = 0 },
                    pen,
                    new PointF(1, 3),
                    new PointF(3, 1)));

            this.ExpectedImage = expected;
            this.ActualImage = actual;

            // this should pass without bug
            //this.AssertImagesEqual();

            /*
                Assert.Fail failed. Images are not equal - total delta 0.00019455253 is not less than tolerance 0.
                Difference are:

                    - at (2,1) in actual the expected color is Rgba32(0, 0, 0, 255) and actual is Rgba32(255, 0, 0, 255)
                    - at (3,1) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)
                    - at (1,2) in actual the expected color is Rgba32(0, 0, 0, 255) and actual is Rgba32(255, 0, 0, 255)
                    - at (2,2) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)
                    - at (1,3) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)

                (and 0  more..)
             */

            // this should not work, but does because of bug. Bug image looks like:
            // . . . . .
            // . . R . .
            // . R . . .
            // . . . . .
            // . . . . .
            Assert.AreEqual(color.ToPixel<Rgb24>(), actual[2, 1]);
            Assert.AreEqual(color.ToPixel<Rgb24>(), actual[1, 2]);
        }

        [TestMethod]
        [TestCategory("smoketest")]
        public void DiagonalLineNotDrawnProperlyCrossCheckBug28()
        {
            // the same as last test but checks that https://github.com/SixLabors/ImageSharp.Drawing/issues/28
            // is not the root of the funkyness

            var color = Color.Red;
            var pen = new Pen(color, 1);

            // a 3-pixel line, bottom left to top right, 1px padding around edge
            // . . . . .
            // . . . R .
            // . . R . .
            // . R . . .
            // . . . . .
            var expected = new Image<Rgb24>(5, 5);
            expected[1, 3] = color;
            expected[2, 2] = color;
            expected[3, 1] = color;

            var actual = new Image<Rgb24>(5, 5);
            actual.Mutate(
                context => context.DrawLines(
                    new GraphicsOptions() { Antialias = false, AntialiasSubpixelDepth = 0 },
                    pen,
                    new PointF(1, 3) + new PointF(0.0f, 0.5f),
                    new PointF(3, 1) + new PointF(0.0f, 0.5f)));

            this.ExpectedImage = expected;
            this.ActualImage = actual;

            // this should pass without bug
            //this.AssertImagesEqual();

            /*
                Assert.Fail failed. Images are not equal - total delta 0.00015564202 is not less than tolerance 0.
                Difference are:

                    - at (2,1) in actual the expected color is Rgba32(0, 0, 0, 255) and actual is Rgba32(255, 0, 0, 255)
                    - at (3,1) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)
                    - at (1,2) in actual the expected color is Rgba32(0, 0, 0, 255) and actual is Rgba32(255, 0, 0, 255)
                    - at (2,2) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)

                (and 0  more..)

             */

            // this should not work, but does because of bug. Bug image looks like:
            // . . . . .
            // . . R . .
            // . R . . .
            // . R . . .
            // . . . . .
            Assert.AreEqual(color.ToPixel<Rgb24>(), actual[2, 1]);
            Assert.AreEqual(color.ToPixel<Rgb24>(), actual[1, 2]);
        }

        [TestMethod]
        [TestCategory("smoketest")]
        public void DiagonalLineNotDrawnProperlyCrossCheckBug28SecondAttempt()
        {
            // the same as last test but checks that https://github.com/SixLabors/ImageSharp.Drawing/issues/28
            // is not the root of the funkyness

            var color = Color.Red;
            var pen = new Pen(color, 1);

            // a 3-pixel line, bottom left to top right, 1px padding around edge
            // . . . . .
            // . . . R .
            // . . R . .
            // . R . . .
            // . . . . .
            var expected = new Image<Rgb24>(5, 5);
            expected[1, 3] = color;
            expected[2, 2] = color;
            expected[3, 1] = color;

            var actual = new Image<Rgb24>(5, 5);
            actual.Mutate(
                context => context.DrawLines(
                    new GraphicsOptions() { Antialias = false, AntialiasSubpixelDepth = 0 },
                    pen,
                    new PointF(1, 3) + new PointF(0.5f, 0.5f),
                    new PointF(3, 1) + new PointF(0.5f, 0.5f)));

            this.ExpectedImage = expected;
            this.ActualImage = actual;

            // this should pass without bug
            //this.AssertImagesEqual();

            /*
                Assert.Fail failed. Images are not equal - total delta 7.782101E-05 is not less than tolerance 0.
                Difference are:

                    - at (1,3) in actual the expected color is Rgba32(255, 0, 0, 255) and actual is Rgba32(0, 0, 0, 255)
                    - at (2,3) in actual the expected color is Rgba32(0, 0, 0, 255) and actual is Rgba32(255, 0, 0, 255)

                (and 0  more..)
             */

            // this should not work, but does because of bug. Bug image looks like:
            // . . . . .
            // . . . R .
            // . . R . .
            // . . R . .
            // . . . . .
            Assert.AreEqual(color.ToPixel<Rgb24>(), actual[2, 3]);
        }

        [TestMethod]
        public void TestOurWrapperMethodDrawsCorrectLine()
        {
            var color = Color.Red;
            var pen = new Pen(color, 1);

            // a 3-pixel line, bottom left to top right, 1px padding around edge
            // . . . . .
            // . . . R .
            // . . R . .
            // . R . . .
            // . . . . .
            var expected = new Image<Rgb24>(5, 5);
            expected[1, 3] = color;
            expected[2, 2] = color;
            expected[3, 1] = color;

            var actual = new Image<Rgb24>(5, 5);

            // our wrapper method correct for Bug28 offset and
            // repeats the first point in the line, which seems to remove artifacts
            actual.Mutate(
                context => context.NoAA().DrawLines(
                    pen,
                    new PointF(1, 3),
                    new PointF(3, 1)));

            this.ExpectedImage = expected;
            this.ActualImage = actual;

            // this should pass without bug
            this.AssertImagesEqual();

        }

        [TestMethod]
        public void TestNoAADrawLineDiagonalMultiplePoints()
        {
            // red line, 1px per row, diagonal top left to bottom right
            // but top left and bottom right pixels are empty
            string specification = ".100\n" + Enumerable
                .Range(1, 98)
                .Select(x => new string('.', x) + "R")
                .Join("\n");
            this.ExpectedImage = new TestImage(100, 100, Color.Black)
                .FillPattern(specification, Color.Black)
                .Finish();

            var path = Enumerable
                .Range(1, 99)
                .Select(x => new PointF(x, x))
                .ToArray();

            this.ActualImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            this.ActualImage.Mutate(x => x.NoAA().DrawLines(Pens.Solid(Color.Red, 1f), path));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineDiagonalFewerPoints()
        {
            // red line, 1px per row, diagonal top left to bottom right
            // but bottom left and top right pixels are empty
            string specification = ".100\n" + Enumerable
                .Range(1, 98)
                .Select(x => new string('.', 100 - x - 1) + "R")
                .Join("\n");
            this.ExpectedImage = new TestImage(100, 100, Color.Black)
                .FillPattern(specification, Color.Black)
                .Finish();

            var path = new[]
            {
                new PointF(1, 98),
                new PointF(1, 98),
                new PointF(98, 1),
            };

            this.ActualImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            this.ActualImage.Mutate(x => x.NoAA().DrawLines(Pens.Solid(Color.Red, 1f), path));

            this.AssertImagesEqual();
        }
    }
}
