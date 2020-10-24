// <copyright file="EventFilterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Types;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EventFilterTests
    {
        [TestMethod]
        public void TestRemoveEnclosedEvent()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 300,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<EventCommon>() { ev1, ev2, ev3 };
            var events = CompositeEvent.RemoveEnclosedEvents(listOfEvents);

            Assert.AreEqual(2, events.Count);
            var @event = (SpectralEvent)events[0];

            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(200, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(300, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev1", @event.Name);

            @event = (SpectralEvent)events[1];
            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(500, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev2", @event.Name);

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 300,
                LowFrequencyHertz = 200,
            };

            var ev5 = new SpectralEvent()
            {
                Name = "ev5",
                EventEndSeconds = 8,
                EventStartSeconds = 2,
                HighFrequencyHertz = 250,
                LowFrequencyHertz = 200,
            };

            // test with pair enclosed events
            listOfEvents = new List<EventCommon>() { ev4, ev5 };
            events = CompositeEvent.RemoveEnclosedEvents(listOfEvents);

            Assert.AreEqual(1, events.Count);
            @event = (SpectralEvent)events[0];
            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(200, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(300, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev4", @event.Name);

            // test with the same pair enclosed events, but in reversed order.
            listOfEvents = new List<EventCommon>() { ev5, ev4 };
            events = CompositeEvent.RemoveEnclosedEvents(listOfEvents);

            Assert.AreEqual(1, events.Count);
            @event = (SpectralEvent)events[0];
            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(200, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(300, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev4", @event.Name);

            // test longer list with multiple ecnlosed events
            listOfEvents = new List<EventCommon>() { ev1, ev2, ev3, ev4, ev5, ev1, ev2, ev3, ev4, ev5, ev5, ev3, ev3, ev2, ev1 };
            events = CompositeEvent.RemoveEnclosedEvents(listOfEvents);

            Assert.AreEqual(2, events.Count);
            @event = (SpectralEvent)events[0];
            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(200, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(300, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev1", @event.Name);

            @event = (SpectralEvent)events[1];
            Assert.AreEqual(1, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(9, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(500, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev2", @event.Name);
        }
    }
}
