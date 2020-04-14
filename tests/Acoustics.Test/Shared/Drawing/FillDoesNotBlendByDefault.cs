// <copyright file="FillDoesNotBlendByDefault.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
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
            Assert.AreEqual(new Rgb24(255, 0, 0), image[50, 50]);

            // fails with:     Assert.AreEqual failed. Expected:<Rgb24(128, 0, 0)>. Actual:<Rgb24(255, 0, 0)>.

            // Actual (buggy?) result
            // According to @JimBobSquarePants this is the expected bahviour
            /*

            > @atruskie You're expecting the wrong thing.
            >
            > You have a 24bit pixel image. Adjusting the opacity of the color you are blending will not make a difference since
            > it will be converted to Rgb24. By  default the GraphicsOptions used by filling will have a color blending mode of
            > PixelColorBlendingMode.Normal and alpha composition mode of PixelAlphaCompositionMode.SrcOverwhich will simply paint
            > the color over the background since there is no alpha component.
            >
            > To get your expected result the background should have an alpha component. So use Rgba32.
            >
            > You should also dispose of your images once you have finished with them.

            @JimBobSquarePants, thanks for the response. I really don't need an Rgba32 image. The image itself doesn't need any transparency.

            Given that IImageProcessingContext is meant to be a pixel agnostic drawing interface, my question becomes, how do I fill a region with a 50% opacity colour? So setting BlendPercentage = 0.5f in graphics options does a fill with a blend. I admittedly don't understand the effects of PixelColorBlendingMode and PixelAlphaCompositionMode but I tried various values and it did not seem to have any effect.

            However, may I suggest what I was expecting should work? Here's why:

            System.Drawing and SkiaSharp act how I expect (caveat: I am no expert on anything). Example: https://gist.github.com/atruskie/02f6fb35d6967c57ed543d109e83736e
            If a Rgb24 pixel blender encounters an Rgba32 pixel, it blends with the rgba32's opacity as I expect, so why doesn't that symmetry hold for image&fill? Example: https://gist.github.com/atruskie/f12f22830f79ba42943433edee964a53
            */

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
