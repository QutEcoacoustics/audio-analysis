// <copyright file="ImageSharpBugTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using global::Acoustics.Shared.ImageSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// This class tests that the ImageSharp API fails in a manner that is expected.
    /// If a test fails here the equivalent shim should be removed from
    /// <see cref="Acoustics.Shared.ImageSharp.Drawing"/> or
    /// <see cref="SixLabors.ImageSharp.ImageSharpExtensions"/>.
    /// </summary>
    [TestClass]
    public class ImageSharpBugTests
    {
        private const string TestText = "ABC"; // {FontRectangle [ X=-0.0146484375, Y=0, Width=20.180664, Height=7.4023438 ]}
        private static readonly Font TestFont = Drawing.Arial10;
        private static readonly Color TestColor = Color.Red;
        private static readonly IBrush TestBrush = Brushes.Solid(TestColor);
        private static readonly Pen TestPen = new Pen(Color.Red, 1f);
        private static readonly PointF NoOverlap = new PointF(-100, -100);
        private static readonly PointF PartialOverlap = new PointF(-5, -5);
        private static readonly PointF CompleteOverlap = new PointF(25, 25);
        private static readonly PointF NoOverlapPositive = new PointF(-100, -100);
        private static readonly PointF PartialOverlapPositive = new PointF(95, 95);
        private static readonly Image<Rgb24> OverImage = new Image<Rgb24>(Configuration.Default, 50, 50, Color.Red);
        private static readonly Size TestSize = new Size(150, 150);

        private static readonly Configuration Parallel = new Configuration()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
        };

        private static readonly Configuration NoParallel = new Configuration()
        {
            MaxDegreeOfParallelism = 1,
        };

        private readonly Image<Rgb24> testImage;

        public ImageSharpBugTests()
        {
            this.testImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public void DrawProcessors_NoOverlap()
        {
            // no overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(Parallel, x => x.DrawText(TestText, TestFont, TestBrush, NoOverlap));
            this.testImage.Mutate(Parallel, x => x.DrawText(TestText, TestFont, TestBrush, NoOverlapPositive));

            // no overlap does throw for images
            Assert.ThrowsException<ImageProcessingException>(() =>
                this.testImage.Mutate(Parallel, x => x.DrawImage(OverImage, (Point)NoOverlap, 1f)));
            Assert.ThrowsException<ImageProcessingException>(() =>
            this.testImage.Mutate(Parallel, x => x.DrawImage(OverImage, (Point)NoOverlapPositive, 1f)));

            // no overlap does NOT throw for rects
            this.testImage.Mutate(Parallel, x => x.Draw(TestPen, new Rectangle((Point)NoOverlap, TestSize)));
            this.testImage.Mutate(Parallel, x => x.Draw(TestPen, new Rectangle((Point)NoOverlapPositive, TestSize)));
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public void DrawProcessors_CompleteOverlap()
        {
            // complete overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(Parallel, x => x.DrawText(TestText, TestFont, TestBrush, CompleteOverlap));

            // complete overlap does NOT throw for images
            this.testImage.Mutate(Parallel, x => x.DrawImage(OverImage, (Point)CompleteOverlap, 1f));

            // complete overlap does NOT throw for rects
            this.testImage.Mutate(Parallel, x => x.Draw(TestPen, new Rectangle((Point)CompleteOverlap, TestSize)));
        }

        [TestMethod]
        [TestCategory("Parallel")]
        public void DrawProcessors_PartialOverlap()
        {
            // partial overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(Parallel, x => x.DrawText(TestText, TestFont, TestBrush, PartialOverlap));
            this.testImage.Mutate(Parallel, x => x.DrawText(TestText, TestFont, TestBrush, PartialOverlapPositive));

            // partial overlap does NOT throw for images
            this.testImage.Mutate(Parallel, x => x.DrawImage(OverImage, (Point)PartialOverlap, 1f));
            this.testImage.Mutate(Parallel, x => x.DrawImage(OverImage, (Point)PartialOverlapPositive, 1f));

            // partial overlap does NOT throw for rects
            this.testImage.Mutate(Parallel, x => x.Draw(TestPen, new Rectangle((Point)PartialOverlap, TestSize)));
            this.testImage.Mutate(Parallel, x => x.Draw(TestPen, new Rectangle((Point)PartialOverlapPositive, TestSize)));
        }

        [TestMethod]
        [TestCategory("NoParallel")]
        public void DrawProcessors_NoOverlap_NoParallel()
        {
            // no overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(NoParallel, x => x.DrawText(TestText, TestFont, TestBrush, NoOverlap));
            this.testImage.Mutate(NoParallel, x => x.DrawText(TestText, TestFont, TestBrush, NoOverlapPositive));

            // no overlap does throw for images
            Assert.ThrowsException<ImageProcessingException>(() =>
                this.testImage.Mutate(NoParallel, x => x.DrawImage(OverImage, (Point)NoOverlap, 1f)));
            Assert.ThrowsException<ImageProcessingException>(() =>
            this.testImage.Mutate(NoParallel, x => x.DrawImage(OverImage, (Point)NoOverlapPositive, 1f)));

            // no overlap does NOT throw for rects
            this.testImage.Mutate(NoParallel, x => x.Draw(TestPen, new Rectangle((Point)NoOverlap, TestSize)));
            this.testImage.Mutate(NoParallel, x => x.Draw(TestPen, new Rectangle((Point)NoOverlapPositive, TestSize)));
        }

        [TestMethod]
        [TestCategory("NoParallel")]
        public void DrawProcessors_CompleteOverlap_NoParallel()
        {
            // complete overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(NoParallel, x => x.DrawText(TestText, TestFont, TestBrush, CompleteOverlap));

            // complete overlap does NOT throw for images
            this.testImage.Mutate(NoParallel, x => x.DrawImage(OverImage, (Point)CompleteOverlap, 1f));

            // complete overlap does NOT throw for rects
            this.testImage.Mutate(NoParallel, x => x.Draw(TestPen, new Rectangle((Point)CompleteOverlap, TestSize)));
        }

        [TestMethod]
        [TestCategory("NoParallel")]
        public void DrawProcessors_PartialOverlap_NoParallel()
        {
            // partial overlap does NOT throw for text
            var textSize = TextMeasurer.MeasureBounds(TestText, new RendererOptions(TestFont));
            Assert.IsTrue(textSize.Width < 50 && textSize.Height < 50);
            this.testImage.Mutate(NoParallel, x => x.DrawText(TestText, TestFont, TestBrush, PartialOverlap));
            this.testImage.Mutate(NoParallel, x => x.DrawText(TestText, TestFont, TestBrush, PartialOverlapPositive));

            // partial overlap does NOT throw for images
            this.testImage.Mutate(NoParallel, x => x.DrawImage(OverImage, (Point)PartialOverlap, 1f));
            this.testImage.Mutate(NoParallel, x => x.DrawImage(OverImage, (Point)PartialOverlapPositive, 1f));

            // partial overlap does NOT throw for rects
            this.testImage.Mutate(NoParallel, x => x.Draw(TestPen, new Rectangle((Point)PartialOverlap, TestSize)));
            this.testImage.Mutate(NoParallel, x => x.Draw(TestPen, new Rectangle((Point)PartialOverlapPositive, TestSize)));
        }
    }
}