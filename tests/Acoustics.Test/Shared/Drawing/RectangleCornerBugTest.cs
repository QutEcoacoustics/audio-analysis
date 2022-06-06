// <copyright file="RectangleCornerBugTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Drawing
{
    using System;
    using Acoustics.Test.TestHelpers;
    using global::Acoustics.Shared.ImageSharp;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class RectangleCornerBugTest : OutputDirectoryTest
    {
        // add padding on to actual error delta
        public const double MissingCornerDelta = 9.727626E-08 + 0.00000001;

        [TestMethod]
        [TestCategory("smoketest")]
        public void RectangleHasMissingBottomRightCorner()
        {
            var testImage = new Image<Rgb24>(Configuration.Default, 100, 100, Color.Black);

            var rectangle = new RectangleF(10.0f, 10.0f, 79, 79);
            var pen = new Pen(Color.Red, 1f);

            var options = new DrawingOptions()
            {
                GraphicsOptions = new GraphicsOptions()
                {
                    //BlendPercentage = 1,
                    Antialias = false,
                    //ColorBlendingMode = PixelColorBlendingMode.Normal,
                    AntialiasSubpixelDepth = 0,
                },
            };

            testImage.Mutate(x => x.Draw(options, pen, rectangle));

            testImage.Save(this.TestOutputDirectory.CombinePath("rectangle.png"));

            // expected
            //      88 | 89 | 90
            // 88 | B  | R  | B
            // 89 | R  | R  | B
            // 90 | B  | B  | B

            // AT 2020-11: Update bug seems to be fixed. Leaving the passing test to ensure stable.
            // smoke test: when this test fails, bug in ImageSharp has been fixed
            // should be black
            Assert.AreEqual(
                Color.Black.ToPixel<Rgb24>(),
                testImage[88, 88]);

            // should be red (bug *was* that it is blended)
            Assert.AreEqual(
                new Rgb24(255, 0, 0),
                testImage[89, 89]);

            // should be black
            Assert.AreEqual(
                Color.Black.ToPixel<Rgb24>(),
                testImage[90, 90]);
        }

        [TestMethod]
        [TestCategory("smoketest")]
        public void DrawTest()
        {
            // arrange
            string specification = @"
⬇10
E10R80E10
78×E10RE78RE10
E10R80E10
⬇10
";
            var expected = TestImage.Create(100, 100, Color.Black, specification);

            var pen = new Pen(Color.Red, 1f);
            var actual = Drawing.NewImage(100, 100, Color.Black);

            // act
            actual.Mutate(x =>
            {
                //var border = options.Converters.GetPixelRectangle(@event);
                var border = new RectangleF(10, 10, 79, 79);

                // the following two should be equivalent
                //x.NoAA().DrawRectangle(pen, border);
                //border.Offset(Drawing.NoAA.Bug28Offset);
                x.Draw(Drawing.NoAntiAlias, pen, border);
            });

            // assert - this should pass without the delta if the bug was fixed
            Assert.That.ImageMatches(expected, actual, MissingCornerDelta);

            // AT 2020-11: Update bug seems to be fixed. Leaving the passing test to ensure stable.
            // if there were no bug the expected pixel color is red
            Assert.AreEqual(
                new Rgb24(255, 0, 0),
                actual[89, 89]);
        }
    }
}
