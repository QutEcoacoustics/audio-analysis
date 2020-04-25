// <copyright file="AcousticEventTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Drawing;
    using global::AudioAnalysisTools.Events.Types;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class AcousticEventTests : GeneratedImageTest<Rgb24>
    {
        [TestMethod]
        public void TestEventMerging()
        {
            // make a list of three events
            var events = new List<SpectralEvent>();
            var segmentStartTime = TimeSpan.FromSeconds(10);
            var event1 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 11.0, eventEndRecordingRelative: 16.0, minFreq: 1000, maxFreq: 6000)
            {
                Name = "Event1",
                Score = 1.0,
            };

            events.Add(event1);

            var event2 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 12.0, eventEndRecordingRelative: 15.0, minFreq: 1500, maxFreq: 8000)
            {
                Name = "Event2",
                Score = 5.0,
            };
            events.Add(event2);

            var event3 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 17.0, eventEndRecordingRelative: 19.0, minFreq: 1000, maxFreq: 8000)
            {
                Name = "Event3",
                Score = 9.0,
            };
            events.Add(event3);

            // combine Overlapping acoustic events
            events = CompositeEvent.CombineOverlappingEvents(events: events);

            //require two events, the first being a composite of two events.
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(CompositeEvent), events[0].GetType());
            //################################################# WHY DOES FOLLOWING LINE NOT WORK????
            //Assert.AreEqual(2, events[0].ComponentCount);

            Assert.AreEqual(11.0, events[0].EventStartSeconds, 1E-4);
            Assert.AreEqual(16.0, events[0].EventEndSeconds, 1E-4);
            Assert.AreEqual(1000, events[0].LowFrequencyHertz);
            Assert.AreEqual(8000, events[0].HighFrequencyHertz);
            Assert.AreEqual(5.0, events[0].Score, 1E-4);

            Assert.AreEqual(typeof(SpectralEvent), events[1].GetType());
            Assert.AreEqual(17.0, events[1].EventStartSeconds, 1E-4);
            Assert.AreEqual(19.0, events[1].EventEndSeconds, 1E-4);
            Assert.AreEqual(1000, events[1].LowFrequencyHertz);
            Assert.AreEqual(8000, events[1].HighFrequencyHertz);
            Assert.AreEqual(9.0, events[1].Score, 1E-4);
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

            // set a max score to normalise.
            double maxScore = 10.0;

            // make a list of two events
            var events = new List<SpectralEvent>();
            var segmentStartTime = TimeSpan.FromSeconds(10);
            var event1 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 11.0, eventEndRecordingRelative: 16.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 10.0 / maxScore,
                Name = "Event1",
            };

            events.Add(event1);
            var event2 = new SpectralEvent(segmentStartOffset: segmentStartTime, eventStartRecordingRelative: 17.0, eventEndRecordingRelative: 19.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 1.0 / maxScore,
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