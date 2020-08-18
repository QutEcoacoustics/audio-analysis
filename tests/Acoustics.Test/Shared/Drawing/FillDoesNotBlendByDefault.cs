// <copyright file="FillDoesNotBlendByDefault.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class FillDoesNotBlendByDefault
    {
        private static readonly IBrush Red50Percent = Brushes.Solid(new Rgba32(255, 0, 0, 128));

        // 50% blended with black
        private static readonly Rgb24 ExpectedColor = new Rgb24(128, 0, 0);

        [TestMethod]
        [TestCategory("smoketest")]
        public void Test()
        {
            using var image = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            image.Mutate(x => { x.Fill(Red50Percent); });

            // expected result
            //Assert.AreEqual(expectedColor, image[50, 50]);
            // fails with:     Assert.AreEqual failed. Expected:<Rgb24(128, 0, 0)>. Actual:<Rgb24(255, 0, 0)>.

            Assert.AreEqual(new Rgb24(255, 0, 0), image[50, 50]);

            // BUG: Blending does not occur with fill https://github.com/SixLabors/ImageSharp.Drawing/issues/38

            // thus we need our own method. see below
        }

        [TestMethod]
        public void TestFillWithBlend()
        {
            using var workaround = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            workaround.Mutate(x => x.FillWithBlend(Red50Percent));

            Assert.That.ImageRegionIsColor(workaround.Bounds(), ExpectedColor, workaround);
        }

        [TestMethod]
        public void TestFillWithBlendRect()
        {
            using var workaround = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            workaround.Mutate(x => x.FillWithBlend(Red50Percent, new RectangleF(20, 20, 60, 60)));

            Assert.That.ImageRegionIsColor(new Rectangle(20, 20, 60, 60), ExpectedColor, workaround);
        }

        [TestMethod]
        public void TestFillWithBlendNonOpaqueDelegatesToStandardFill()
        {
            using var workaround = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            workaround.Mutate(x => x.FillWithBlend(Brushes.Solid(Color.Red)));

            Assert.That.ImageRegionIsColor(workaround.Bounds(), Color.Red, workaround);
        }

        [TestMethod]
        public void TestFillWithBlendFailsWithNonSolidBrush()
        {
            using var workaround = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);

            Assert.ThrowsException<NotSupportedException>(
                () => workaround.Mutate(x => x.FillWithBlend(Brushes.Percent10(Color.Red))),
                "Can't handle non-solid brushed"
            );
        }

    }
}
