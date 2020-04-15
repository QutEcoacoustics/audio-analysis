// <copyright file="SpectralEventTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.Shared.Drawing;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class SpectralEventTest : GeneratedImageTest<Rgb24>
    {
        public SpectralEventTest()
        {
            this.ActualImage = Drawing.NewImage(100, 100, Color.Black);
        }

        [TestMethod]
        public void DerivedPropertiesTest()
        {
            var @event = new SpectralEvent()
            {
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 900,
                LowFrequencyHertz = 100,
            };

            Assert.AreEqual(8, @event.EventDurationSeconds);
            Assert.AreEqual(800, @event.BandWidthHertz);
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
            this.ExpectedImage = TestImage.Create(100, 100, Color.Black, specification);

            var @event = new SpectralEvent()
            {
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 900,
                LowFrequencyHertz = 100,
            };
            var options = new EventRenderingOptions(new UnitConverters(0, 10, 1000, 100, 100));

            // act

            this.ActualImage.Mutate(x => @event.Draw(x, options));

            // assert
            this.AssertImagesEqual();
        }
    }
}
