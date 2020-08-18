// <copyright file="NegativeTextBug.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class NegativeTextBug : GeneratedImageTest<Rgb24>
    {
        public NegativeTextBug()
        {
            this.ActualImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
        }

        /// <summary>
        /// <see cref="ImageSharpExtensions.DrawTextSafe"/>.
        /// see https://github.com/SixLabors/ImageSharp.Drawing/issues/30.
        /// issue is now fixed. Leaving test cases but have gutted DrawTextSafe.
        /// </summary>
        [TestMethod]
        public void TextRendersWithoutIssue()
        {
            var text = "2016-12-10";

            // first make sure the text rectangle actually overlaps
            var destArea = new RectangleF(PointF.Empty, this.ActualImage.Size());
            var textArea = TextMeasurer.MeasureBounds(text, new RendererOptions(Drawing.Roboto10, new Point(-10, 3)));
            Assert.IsTrue(destArea.IntersectsWith(textArea.AsRect()));

            // THIS SHOULD IDEALLY NOT THROW
            this.ActualImage.Mutate(
                        x => { x.DrawTextSafe(text, Drawing.Roboto10, Color.White, new PointF(-10, 3)); });

            this.ExpectedImage = Drawing.NewImage(100, 100, Color.Black);
            this.ExpectedImage.Mutate(
                x => x.DrawText(text, Drawing.Roboto10, Color.White, new PointF(-10, 3)));

            var tolerance = 0;
            this.AssertImagesEqual(tolerance);
        }

        [TestMethod]
        public void MakeSureWeAccountForKerning()
        {
            var text = "i1i1i1i1i1i1i1i1i1";

            var textArea = TextMeasurer.MeasureBounds(text, new RendererOptions(Drawing.Roboto10, new Point(-70, 3)));

            this.ActualImage.Mutate(x => { x.DrawTextSafe(text, Drawing.Roboto10, Color.White, new PointF(-70, 3)); });

            this.ExpectedImage = Drawing.NewImage(100, 100, Color.Black);
            this.ExpectedImage.Mutate(
                x => x.DrawText(text, Drawing.Roboto10, Color.White, new PointF(-70, 3)));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void AnotherCaseThatCausedAFailure()
        {
            if (!SystemFonts.TryFind(Drawing.Arial, out _))
            {
                Assert.Inconclusive("Skipping test because the Font Arial is not available");
            }

            var text = "2/08/2018";

            var textArea = TextMeasurer.MeasureBounds(text, new RendererOptions(Drawing.Arial10, new Point(-13, 3)));

            this.ActualImage.Mutate(x => { x.DrawTextSafe(text, Drawing.Arial10, Color.White, new PointF(-13, 3)); });

            this.ExpectedImage = Drawing.NewImage(100, 100, Color.Black);
            this.ExpectedImage.Mutate(
                x => x.DrawText(text, Drawing.Arial10, Color.White, new PointF(-13, 3)));

            this.AssertImagesEqual();
        }
    }
}