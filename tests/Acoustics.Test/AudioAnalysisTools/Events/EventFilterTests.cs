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
        public void TestRemoveEnclosedEvents()
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

        [TestMethod]
        public void TestFilterEventsOnBandwidth1()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 2000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
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

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<EventCommon>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterOnBandwidth(listOfEvents, minBandwidth: 100, maxBandwidth: 200);

            Assert.AreEqual(2, events.Count);
            var @event = (SpectralEvent)events[0];

            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(600, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev2", @event.Name);

            @event = (SpectralEvent)events[1];
            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(500, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev3", @event.Name);
        }

        [TestMethod]
        public void TestFilterEventsOnBandwidth2()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 2000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
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

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<EventCommon>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterOnBandwidth(listOfEvents, average: 150, sd: 20, sigmaThreshold: 3.0);

            Assert.AreEqual(2, events.Count);
            var @event = (SpectralEvent)events[0];

            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(600, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev2", @event.Name);

            @event = (SpectralEvent)events[1];
            Assert.AreEqual(400, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(500, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("ev3", @event.Name);
        }

        [TestMethod]
        public void TestFilterShortEvents()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 6.001,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 3000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9.5,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
                LowFrequencyHertz = 400,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 6.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 4.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<SpectralEvent>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterShortEvents(listOfEvents, minimumDurationSeconds: 5.0);

            Assert.AreEqual(3, events.Count);
            var @event = (SpectralEvent)events[2];
            Assert.AreEqual(1.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual("ev3", @event.Name);
        }

        [TestMethod]
        public void TestFilterLongEvents()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 6.001,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 3000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9.5,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
                LowFrequencyHertz = 400,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 6.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 4.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<SpectralEvent>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterLongEvents(listOfEvents, maximumDurationSeconds: 5.0);

            Assert.AreEqual(2, events.Count);
            var @event = (SpectralEvent)events[0];
            Assert.AreEqual(1.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual("ev3", @event.Name);
        }

        [TestMethod]
        public void TestFilterOnDuration1()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 6.001,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 3000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9.5,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
                LowFrequencyHertz = 400,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 6.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 4.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<EventCommon>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterOnDuration(listOfEvents, minimumDurationSeconds: 4.0, maximumDurationSeconds: 5.0);

            Assert.AreEqual(1, events.Count);
            var @event = (SpectralEvent)events[0];
            Assert.AreEqual(1.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual("ev3", @event.Name);
        }

        [TestMethod]
        public void TestFilterOnDuration2()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 6.001,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 3000,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 9.5,
                EventStartSeconds = 1,
                HighFrequencyHertz = 600,
                LowFrequencyHertz = 400,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 6.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 500,
                LowFrequencyHertz = 400,
            };

            var ev4 = new SpectralEvent()
            {
                Name = "ev4",
                EventEndSeconds = 4.0,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 450,
                LowFrequencyHertz = 400,
            };

            // test with combination of events
            var listOfEvents = new List<EventCommon>() { ev1, ev2, ev3, ev4 };
            var events = EventFilters.FilterOnDuration(listOfEvents, average: 5.0, sd: 0.1, sigmaThreshold: 3.0);

            Assert.AreEqual(2, events.Count);
            var @event = (SpectralEvent)events[0];
            Assert.AreEqual(1.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual("ev1", @event.Name);
        }

        [TestMethod]
        public void TestTemporalFootprintOfCompositeEvent()
        {
            var ev1 = new SpectralEvent()
            {
                Name = "ev1",
                EventEndSeconds = 1.5,
                EventStartSeconds = 1.0,
                HighFrequencyHertz = 300,
                LowFrequencyHertz = 200,
            };

            var ev2 = new SpectralEvent()
            {
                Name = "ev2",
                EventEndSeconds = 2.5,
                EventStartSeconds = 2.0,
                HighFrequencyHertz = 310,
                LowFrequencyHertz = 210,
            };

            var ev3 = new SpectralEvent()
            {
                Name = "ev3",
                EventEndSeconds = 3.5,
                EventStartSeconds = 3.0,
                HighFrequencyHertz = 320,
                LowFrequencyHertz = 220,
            };

            // test with combination of events
            var events = new List<SpectralEvent>() { ev1, ev2, ev3 };
            var compositeEvents = CompositeEvent.CombineProximalEvents(events: events, startDifference: TimeSpan.FromSeconds(1.1), hertzDifference: 50);
            var compositeEvent = compositeEvents[0];
            (bool[] footprint, double timesScale) = EventFilters.GetTemporalFootprint(compositeEvent);

            Assert.AreEqual(100, footprint.Length);
            Assert.AreEqual(0.025, timesScale);
            Assert.AreEqual(true, footprint[10]);
            Assert.AreEqual(false, footprint[20]);
            Assert.AreEqual(false, footprint[30]);
            Assert.AreEqual(true, footprint[40]);
            Assert.AreEqual(true, footprint[50]);
            Assert.AreEqual(false, footprint[60]);
            Assert.AreEqual(false, footprint[70]);
            Assert.AreEqual(true, footprint[80]);
            Assert.AreEqual(true, footprint[90]);
            Assert.AreEqual(true, footprint[99]);
        }
    }
}
