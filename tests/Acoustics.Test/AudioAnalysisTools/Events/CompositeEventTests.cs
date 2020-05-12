// <copyright file="CompositeEventTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public class CompositeEventTests : GeneratedImageTest<Rgb24>
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
            var newEvents = CompositeEvent.CombineOverlappingEvents(events: events.Cast<EventCommon>().ToList());
            events = newEvents.Cast<SpectralEvent>().ToList();

            //require two events, the first being a composite of two events.
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(CompositeEvent), events[0].GetType());

            Assert.AreEqual(2, ((CompositeEvent)events[0]).ComponentEvents.Count);

            Assert.AreEqual(11.0, events[0].EventStartSeconds);
            Assert.AreEqual(16.0, events[0].EventEndSeconds);
            Assert.AreEqual(1000, events[0].LowFrequencyHertz);
            Assert.AreEqual(8000, events[0].HighFrequencyHertz);
            Assert.AreEqual(5.0, events[0].Score);

            Assert.AreEqual(typeof(SpectralEvent), events[1].GetType());
            Assert.AreEqual(17.0, events[1].EventStartSeconds);
            Assert.AreEqual(19.0, events[1].EventEndSeconds);
            Assert.AreEqual(1000, events[1].LowFrequencyHertz);
            Assert.AreEqual(8000, events[1].HighFrequencyHertz);
            Assert.AreEqual(9.0, events[1].Score);
        }
    }
}