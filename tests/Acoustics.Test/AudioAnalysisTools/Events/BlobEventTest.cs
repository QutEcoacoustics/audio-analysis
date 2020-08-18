// <copyright file="SpectralEventTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using System.Collections.Generic;
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.Shared.Drawing;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class BlobEventTest : GeneratedImageTest<Rgb24>
    {
        public BlobEventTest()
        {
            this.ActualImage = Drawing.NewImage(100, 100, Color.Black);
        }

        [TestMethod]
        public void DerivedPropertiesTest()
        {
            var @event = new BlobEvent()
            {
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 900,
                LowFrequencyHertz = 100,
            };

            @event.Points.Add(new SpectralPoint((5.1, 5.2), (510, 520), 0.9));

            Assert.AreEqual(8, @event.EventDurationSeconds);
            Assert.AreEqual(800, @event.BandWidthHertz);
            Assert.AreEqual(1, @event.Points.Count);
        }

        [TestMethod]
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
            var p = PixelOperations<Rgb24>.Instance.GetPixelBlender(new GraphicsOptions());

            var green = Color.FromRgba(0, 255, 0, 128);
            this.ExpectedImage = new TestImage(100, 100, Color.Black)
                .FillPattern(specification)

                // the point 5.1 seconds and 520 Hz should match 51, 48
                .GoTo(51, 48)
                .Fill(1, 1, p.Blend(Color.Black, green, 0.5f))
                .GoTo(52, 49)
                .Fill(2, 1, p.Blend(Color.Black, green, 0.5f))
                .GoTo(12, 87)
                .Fill(1, 1, p.Blend(Color.Black, green, 0.5f))
                .Finish();

            // BUG: with DrawPointsAsFill: overlaps are painted twice
            this.ExpectedImage[52, 49] = new Rgb24(0, 192, 0);

            var @event = new BlobEvent()
            {
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 900,
                LowFrequencyHertz = 100,
            };

            @event.Points.Add(new SpectralPoint((5.1, 5.2), (510, 520), 0.9));

            @event.Points.Add(new SpectralPoint((5.2, 5.3), (500, 510), 0.9));

            // double wide, overlaps with previous
            @event.Points.Add(new SpectralPoint((5.2, 5.4), (500, 510), 0.9));

            @event.Points.Add(new SpectralPoint((1.2, 1.3), (120, 130), 0.9));

            var options = new EventRenderingOptions(new UnitConverters(0, 10, 1000, 100, 100))
            {
                // disable the default blend to make testing easier
                FillOptions = new GraphicsOptions(),
                Fill = Brushes.Solid(green),
            };

            // act

            this.ActualImage.Mutate(x => @event.Draw(x, options));

            // assert
            this.AssertImagesEqual();
        }
    }
}
