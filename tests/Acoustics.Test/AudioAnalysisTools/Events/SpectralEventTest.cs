// <copyright file="SpectralEventTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
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

        [TestMethod]
        public void TestSonogramWithEventsOverlay()
        {
            // make a substitute sonogram image
            var imageWidth = 100;
            var imageHeight = 256;
            var substituteSonogram = Drawing.NewImage(imageWidth, imageHeight, Color.Black);

            // set the time/freq scales
            var segmentDuration = 10.0; //seconds
            var nyquist = 11025; //Hertz

            // set a max score to normalize.
            double maxScore = 10.0;

            // make a list of two events
            var events = new List<SpectralEvent>();
            var segmentStartTime = TimeSpan.FromSeconds(10);
            var event1 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 11.0, eventEndRecordingRelative: 16.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 10.0,
                ScoreRange = (0, maxScore),
                Name = "Event1",
            };

            events.Add(event1);
            var event2 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 17.0, eventEndRecordingRelative: 19.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 1.0,
                ScoreRange = (0, maxScore),
                Name = "Event2",
            };
            events.Add(event2);

            // now add events into the spectrogram image with score.
            var options = new EventRenderingOptions(new UnitConverters(segmentStartTime.TotalSeconds, segmentDuration, nyquist, imageWidth, imageHeight));
            foreach (var ev in events)
            {
                // because we are testing placement of box not text.
                ev.Name = string.Empty;
                substituteSonogram.Mutate(x => ev.Draw(x, options));
            }

            this.ActualImage = substituteSonogram;
            var path = PathHelper.ResolveAssetPath("EventTests_SuperimposeEventsOnImage.png");
            this.ExpectedImage = Image.Load<Rgb24>(path);
            this.AssertImagesEqual();
        }
    }
}
