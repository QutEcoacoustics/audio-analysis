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
        /// TODO BUG: see https://github.com/SixLabors/ImageSharp.Drawing/issues/30.
        /// </summary>
        [TestMethod]
        [TestCategory("SpecialCase")]
        public void TextFailsToRender()
        {
            var text = "2016-12-10";

            // first make sure the text rectangle actually overlaps
            var destArea = new RectangleF(PointF.Empty, this.ActualImage.Size());
            var textArea = TextMeasurer.MeasureBounds(text, new RendererOptions(Drawing.Arial10, new Point(-10, 3)));
            Assert.IsTrue(destArea.IntersectsWith(textArea.AsRect()));

            // THIS SHOULD IDEALLY NOT THROW
            Assert.ThrowsException<ImageProcessingException>(
                () =>
                    this.ActualImage.Mutate(
                        x => { x.DrawText(text, Drawing.Arial10, Color.White, new PointF(-10, 3)); }));

            // but our custom safe method side steps the problem
            this.ActualImage.Mutate(x => { x.DrawTextSafe(text, Drawing.Arial10, Color.White, new PointF(-10, 3)); });

            this.ExpectedImage = Drawing.NewImage(100, 100, Color.Black);
            this.ExpectedImage.Mutate(
                x => x.DrawText("016-12-10", Drawing.Arial10, Color.White, new PointF((float)(-10f + 4.741211f), 3)));

            // if we're on a system where Arial isn't installed, we fall back to roboto font,
            // thus we allow a slight tolerance on the image
            var tolerance = SystemFonts.TryFind(Drawing.Arial, out _) ? 0.0 : 1.5E-06;
            this.AssertImagesEqual(tolerance);
        }
    }
}