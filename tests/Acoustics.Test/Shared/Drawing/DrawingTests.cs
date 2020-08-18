// <copyright file="DrawingTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
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
            this.ActualImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);
            this.blankExpected = new TestImage(100, 100, Color.Black);
        }

        [TestMethod]
        public void TestDrawingTextWithRoboto()
        {
            this.ExpectedImage = Image.Load<Rgb24>(PathHelper.ResolveAssetPath("roboto_font_test.png"));

            this.ActualImage.Mutate(x => x.DrawTextSafe("Hello World", Drawing.GetFont(Drawing.Roboto, 18f), Color.White, new PointF(1f, 50f)));

            //this.ActualImage.Save(PathHelper.ResolveAssetPath("roboto_font_test.png"));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestDrawingTextMissingArialFallsbackToRoboto()
        {
            // only some platforms don't have Arial
            if (SystemFonts.TryFind(Drawing.Arial, out var _))
            {
                Assert.Inconclusive("Can't test font fallback when Arial is available");
                return;
            }

            this.ExpectedImage = Image.Load<Rgb24>(PathHelper.ResolveAssetPath("roboto_font_test.png"));

            this.ActualImage.Mutate(x => x.DrawTextSafe("Hello World", Drawing.GetFont(Drawing.Arial, 18f), Color.White, new PointF(1f, 50f)));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestDrawingTextMissingTahomaFallsbackToRoboto()
        {
            // only some platforms don't have Tahoma
            if (SystemFonts.TryFind(Drawing.Tahoma, out var _))
            {
                Assert.Inconclusive("Can't test font fallback when Arial is available");
                return;
            }

            this.ExpectedImage = Image.Load<Rgb24>(PathHelper.ResolveAssetPath("roboto_font_test.png"));

            this.ActualImage.Mutate(x => x.DrawTextSafe("Hello World", Drawing.GetFont(Drawing.Tahoma, 18f), Color.White, new PointF(1f, 50f)));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLine1Px()
        {
            // red line at top, 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("R100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle1Px()
        {
            // red line, 50px down, 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("⬇50\nR100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenOne, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLine2Px()
        {
            // red line at top, 1px high (because line spills over top border, 1px above, 1 below), 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("R100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle2Px()
        {
            // red line, 49px down, 2px high, 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("⬇49\n2×R100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenTwo, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLine3Px()
        {
            // red line at top, 2px high (because line spills over top border, 1px above, 2 below), 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("2×R100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 0, 100, 0));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawLineMiddle3Px()
        {
            // red line, 49px down, 2px high, 100px wide
            this.ExpectedImage = this.blankExpected.FillPattern("⬇49\n3×R100").Finish();

            this.ActualImage.Mutate(x => x.NoAA().DrawLine(TestPenThree, 0, 50, 100, 50));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawBorderInset1Px()
        {
            var specification = @"
⬇1
1×ER98E
96×ERE96RE
1×ER98E
⬇1
";
            this.ExpectedImage = this.blankExpected.FillPattern(specification).Finish();

            var rect = new Rectangle(1, 1, 98, 98);
            this.ActualImage.Mutate(x => x.NoAA().DrawBorderInset(TestPenOne, rect));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawBorderInset2Px()
        {
            var specification = @"
⬇1
2×ERRR94RRE
94×ERRE94RRE
2×ERRR94RRE
⬇1
";
            this.ExpectedImage = this.blankExpected.FillPattern(specification).Finish();

            var rect = new Rectangle(1, 1, 98, 98);
            this.ActualImage.Mutate(x => x.NoAA().DrawBorderInset(TestPenTwo, rect));

            this.AssertImagesEqual();
        }

        [TestMethod]
        public void TestNoAADrawBorderInset3Px()
        {
            var specification = @"
⬇1
3×ERRRR92RRRE
92×ERRRE92RRRE
3×ERRRR92RRRE
⬇1
";
            this.ExpectedImage = this.blankExpected.FillPattern(specification).Finish();

            var rect = new Rectangle(1, 1, 98, 98);
            this.ActualImage.Mutate(x => x.NoAA().DrawBorderInset(TestPenThree, rect));

            this.AssertImagesEqual();
        }
    }
}