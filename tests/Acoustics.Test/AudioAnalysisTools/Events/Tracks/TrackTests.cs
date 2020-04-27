// <copyright file="TrackTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events.Tracks
{
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events.Tracks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TrackTests
    {
        /// <summary>
        /// Each frame is 100 Hz, each bin is 0.05 seconds.
        /// </summary>
        public static readonly UnitConverters NiceTestConverter =
            new UnitConverters(
                segmentStartOffset: 60,
                sampleRate: 1000,
                frameSize: 100,
                frameOverlap: 0.5);

#pragma warning disable SA1310 // Field names should not contain underscore
        /// <summary>
        /// Get a track that is diagonal, increasing one unit
        /// both in time and frequency for each subsequent point.
        /// </summary>
        public static readonly Track TestTrack_TimePositive_FrequencyPositive =
            new Track(
                NiceTestConverter,
                TrackType.FowardTrack,
                (5, 5, 1),
                (6, 6, 2),
                (7, 7, 3),
                (8, 8, 4),
                (9, 9, 5));

        public static readonly Track TestTrack_Whistle =
            new Track(
                NiceTestConverter,
                TrackType.OneBinTrack,
                (5, 5, 1),
                (6, 5, 2),
                (7, 5, 3),
                (8, 5, 4),
                (9, 5, 5));

        public static readonly Track TestTrack_ChevronRight =
            new Track(
                NiceTestConverter,
                TrackType.FowardTrack,
                (5, 5, 1),
                (6, 6, 2),
                (7, 7, 3),
                (8, 8, 4),
                (9, 9, 5),
                (8, 10, 6),
                (7, 11, 7),
                (6, 12, 8),
                (5, 13, 9));
#pragma warning restore SA1310 // Field names should not contain underscore

        [TestMethod]
        public void TestTrackProperties()
        {
            var track = TestTrack_TimePositive_FrequencyPositive;

            Assert.AreEqual(5, track.PointCount);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds, 0.01);

            //Last frame = 9
            Assert.AreEqual(60 + 0.55, track.EndTimeSeconds, 0.01);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 9th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(100, track.HighFreqHertz);

            Assert.AreEqual(0.3, track.DurationSeconds, 0.01);
            Assert.AreEqual(50, track.TrackBandWidthHertz);
        }

        [TestMethod]
        public void TestWhistleProperties()
        {
            //create new track with whistle
            var track = new Track(NiceTestConverter, TrackType.OneBinTrack);
            track.SetPoint(5, 5, 1);
            track.SetPoint(6, 5, 2);
            track.SetPoint(7, 5, 3);
            track.SetPoint(8, 5, 4);
            track.SetPoint(9, 5, 5);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds, 0.001);

            //Last frame = 9
            Assert.AreEqual(60 + 0.55, track.EndTimeSeconds, 0.001);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 5th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(60, track.HighFreqHertz);
        }

        [TestMethod]
        public void TestClickProperties()
        {
            //Create new track with click
            var track = new Track(NiceTestConverter, TrackType.OneFrameTrack);
            track.SetPoint(5, 5, 1);
            track.SetPoint(5, 6, 2);
            track.SetPoint(5, 7, 3);
            track.SetPoint(5, 8, 4);
            track.SetPoint(5, 9, 5);

            // frame duration = 0.1 seconds. First frame = 5.
            Assert.AreEqual(60 + 0.25, track.StartTimeSeconds, 0.001);

            //Last frame = 9
            Assert.AreEqual(60 + 0.35, track.EndTimeSeconds, 0.001);

            //This test returns the bottom side of the 5th freq bin.
            Assert.AreEqual(50, track.LowFreqHertz);

            // this test returns the top side of the 9th freq bin.
            // Bin width = 10 Hz.
            Assert.AreEqual(100, track.HighFreqHertz);
        }

        [TestMethod]
        public void TestTrackAsSequenceOfHertzValues()
        {
            // Create new track with flat and vertical parts
            // frame duration = 0.1 seconds.
            // Bin width = 10 Hz.
            var track = new Track(NiceTestConverter, TrackType.MixedTrack);
            track.SetPoint(5, 5, 1);
            track.SetPoint(6, 5, 2);
            track.SetPoint(7, 6, 3);
            track.SetPoint(8, 7, 4);
            track.SetPoint(9, 9, 5);
            track.SetPoint(9, 12, 6);
            track.SetPoint(9, 15, 7);

            var hertzTrack = track.GetTrackFrequencyProfile();
            Assert.AreEqual(4, hertzTrack.Length);
            double[] expectedArray = { 0, 10, 10, 80 };
            CollectionAssert.AreEqual(expectedArray, hertzTrack);

            // Create new track that apparently goes backwards in time!
            track = new Track(NiceTestConverter, TrackType.UpwardTrack);
            track.SetPoint(10, 4, 1);
            track.SetPoint(9, 5, 2);
            track.SetPoint(8, 6, 3);
            track.SetPoint(7, 7, 4);
            track.SetPoint(6, 9, 5);
            track.SetPoint(5, 12, 6);
            track.SetPoint(5, 15, 7);

            hertzTrack = track.GetTrackFrequencyProfile();
            Assert.AreEqual(5, hertzTrack.Length);
            double[] expectedArray2 = { -60, -20, -10, -10, -10 };
            CollectionAssert.AreEqual(expectedArray2, hertzTrack);
        }
    }
}
