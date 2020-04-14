using AudioAnalysisTools;
using AudioAnalysisTools.Events.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Acoustics.Test.AudioAnalysisTools.Events.Tracks
{
    [TestClass]
    public class TrackTests
    {
        [TestMethod]
        public void TestTrackProperties()
        {
            var converter = new UnitConverters(
                segmentStartOffset: 60,
                sampleRate: 1000,
                frameSize: 100,
                frameOverlap: 0.5);

            var track = new Track(converter);
            track.SetPoint(5, 5, 1);
            track.SetPoint(6, 6, 2);
            track.SetPoint(7, 7, 3);
            track.SetPoint(8, 8, 4);
            track.SetPoint(9, 9, 5);

            Assert.AreEqual(5, track.PointCount);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds);

            //Last frame = 9
            Assert.AreEqual(60 + 0.55, track.EndTimeSeconds);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 9th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(100, track.HighFreqHertz);
        }

        [TestMethod]
        public void TestWhistleProperties()
        {
            var converter = new UnitConverters(
                segmentStartOffset: 60,
                sampleRate: 1000,
                frameSize: 100,
                frameOverlap: 0.5);

            //create new track with whistle
            var track = new Track(converter);
            track.SetPoint(5, 5, 1);
            track.SetPoint(6, 5, 2);
            track.SetPoint(7, 5, 3);
            track.SetPoint(8, 5, 4);
            track.SetPoint(9, 5, 5);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds);

            //Last frame = 9
            Assert.AreEqual(60 + 0.55, track.EndTimeSeconds);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 5th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(60, track.HighFreqHertz);
        }

        [TestMethod]
        public void TestClickProperties()
        {
            var converter = new UnitConverters(
                segmentStartOffset: 60,
                sampleRate: 1000,
                frameSize: 100,
                frameOverlap: 0.5);

            //Create new track with click
            var track = new Track(converter);
            track.SetPoint(5, 5, 1);
            track.SetPoint(5, 6, 2);
            track.SetPoint(5, 7, 3);
            track.SetPoint(5, 8, 4);
            track.SetPoint(5, 9, 5);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds);

            //Last frame = 9
            Assert.AreEqual(60 + 0.35, track.EndTimeSeconds);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 9th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(100, track.HighFreqHertz);
        }
    }
}
