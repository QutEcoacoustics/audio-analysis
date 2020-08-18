// <copyright file="ImageAssertTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using Acoustics.Shared.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class ImageAssertTests
    {
        [TestMethod]
        public void TestColorAssertEmpty()
        {
            var bitmap = new Image<Rgb24>(200, 200);
            Assert.That.ImageColorsWellDistributed(bitmap);
        }

        [TestMethod]
        public void TestColorAssertFails()
        {
            var bitmap = new Image<Rgb24>(200, 200);
            bitmap.Mutate(g =>
            {
                g.DrawRectangle(Pens.Solid(Color.Aqua, 1), 50, 50, 50, 50);
                g.DrawRectangle(Pens.Solid(Color.Red, 1), 60, 60, 140, 140);
            });

            Assert.ThrowsException<AssertFailedException>(() => Assert.That.ImageColorsWellDistributed(bitmap));
        }

        [TestMethod]
        public void TestColorAssertFails2()
        {
            var bitmap = new Image<Rgb24>(200, 200);
            bitmap.Mutate(g => { g.DrawRectangle(Pens.Solid(Color.Red, 1), 20, 0, 180, 200); });

            Assert.ThrowsException<AssertFailedException>(() => Assert.That.ImageColorsWellDistributed(bitmap));
        }

        [TestMethod]
        public void TestColorAssertRandomImage()
        {
            var random = Random.GetRandom();
            var bitmap = new Image<Rgb24>(200, 200);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap[i, j] = random.NextColor();
                }
            }

            Assert.That.ImageColorsWellDistributed(bitmap);
        }
    }
}