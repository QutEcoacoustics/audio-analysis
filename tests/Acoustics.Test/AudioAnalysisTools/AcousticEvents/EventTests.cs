// <copyright file="EventTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class EventTests : GeneratedImageTest<Rgb24>
    {
        [TestMethod]
        public void TestEventMerging()
        {
            // make a list of events
            var events = new List<AcousticEvent>();

            double maxPossibleScore = 10.0;
            var event1 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 1.0, eventDuration: 5.0, minFreq: 1000, maxFreq: 8000)
            {
                Name = "Event1",
                Score = 1.0,
                ScoreNormalised = 1.0 / maxPossibleScore,
            };

            events.Add(event1);

            var event2 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 4.5, eventDuration: 2.0, minFreq: 1500, maxFreq: 6000)
            {
                Name = "Event2",
                Score = 5.0,
                ScoreNormalised = 5.0 / maxPossibleScore,
            };
            events.Add(event2);

            var event3 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 7.0, eventDuration: 2.0, minFreq: 1000, maxFreq: 8000)
            {
                Name = "Event3",
                Score = 9.0,
                ScoreNormalised = 9.0 / maxPossibleScore,
            };
            events.Add(event3);

            // combine adjacent acoustic events
            events = AcousticEvent.CombineOverlappingEvents(events: events, segmentStartOffset: TimeSpan.Zero);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(1.0, events[0].EventStartSeconds, 1E-4);
            Assert.AreEqual(6.5, events[0].EventEndSeconds, 1E-4);
            Assert.AreEqual(7.0, events[1].EventStartSeconds, 1E-4);
            Assert.AreEqual(9.0, events[1].EventEndSeconds, 1E-4);

            Assert.AreEqual(1000, events[0].LowFrequencyHertz);
            Assert.AreEqual(8000, events[0].HighFrequencyHertz);
            Assert.AreEqual(1000, events[1].LowFrequencyHertz);
            Assert.AreEqual(8000, events[1].HighFrequencyHertz);

            Assert.AreEqual(5.0, events[0].Score, 1E-4);
            Assert.AreEqual(9.0, events[1].Score, 1E-4);
            Assert.AreEqual(0.5, events[0].ScoreNormalised, 1E-4);
            Assert.AreEqual(0.9, events[1].ScoreNormalised, 1E-4);
        }

        [TestMethod]
        public void TestSonogramWithEventsOverlay()
        {
            // make a substitute sonogram image
            int width = 100;
            int height = 256;
            var substituteSonogram = new Image<Rgb24>(width, height);

            //substituteSonogram.Mutate(x => x.Pad(width, height, Color.Gray));
            // image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            // make a list of events
            var framesPerSecond = 10.0;
            var freqBinWidth = 43.0664;
            double maxPossibleScore = 10.0;

            var events = new List<AcousticEvent>();
            var event1 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 1.0, eventDuration: 5.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 10.0,
                Name = "Event1",
                ScoreNormalised = 10.0 / maxPossibleScore,
            };

            events.Add(event1);
            var event2 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 7.0, eventDuration: 2.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 1.0,
                Name = "Event2",
                ScoreNormalised = 1.0 / maxPossibleScore,
            };
            events.Add(event2);

            // now add events into the spectrogram image.
            // set colour for the events
            foreach (AcousticEvent ev in events)
            {
                // do not set an event name because text is not drawing anit-aliased at present time.
                ev.Name = string.Empty;
                ev.BorderColour = AcousticEvent.DefaultBorderColor;
                ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                ev.DrawEvent(substituteSonogram, framesPerSecond, freqBinWidth, height);
            }

            substituteSonogram.Save("C:\\temp\\EventTests_SuperimposeEventsOnImage.png");

            this.Actual = substituteSonogram;
            /*
            var pattern = @"
⬇150
E100R50
48×E100RE48R
E100R50
";
            this.Expected = TestImage.Create(width: 100, height: 100, Color.Black, pattern);
            */

            this.Expected = Image.Load<Rgb24>(PathHelper.ResolveAssetPath("EventTests_SuperimposeEventsOnImage.png"));
            this.AssertImagesEqual();
        }
    }
}
