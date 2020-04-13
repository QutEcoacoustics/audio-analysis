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
                segmentStartOffset: 100,
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
            Assert.AreEqual(100 + 250, track.StartTimeSeconds);
            Assert.AreEqual(100 + 450, track.EndTimeSeconds);
            Assert.AreEqual(500, track.LowFreqHertz);
            Assert.AreEqual(900, track.HighFreqHertz);
        }
    }
}
