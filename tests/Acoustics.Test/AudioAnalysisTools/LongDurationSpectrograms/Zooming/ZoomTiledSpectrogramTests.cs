// <copyright file="ZoomTiledSpectrogramTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System;

    using global::AudioAnalysisTools.LongDurationSpectrograms.Zooming;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ZoomTiledSpectrogramTests
    {
        /// <summary>
        /// This method tests how far back we have to pad the first tile of a recording.
        /// It's pretty complicated. 
        /// All tiles start from midnight UTC each day. Then a natural number of tiles
        /// fits within the day. The number of tiles with a day changes accoding to scale.
        /// 
        /// And then you add the subtly of timezone offsets (and rolling back past midnight).
        /// 
        /// We're only test a tile width of 180px because that is only what is used in prod
        /// at the moment.
        /// </summary>
        [DataTestMethod]
        [DataRow("2017-11-08T13:33:33.123+10:00", 180, 60.0, "2017-11-08T13:00:00.000+10:00")]
        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 60.0, "2014-05-29T07:00:00.000+10:00")]
        [DataRow("2014-05-29T00:00:00.000+10:00", 180, 60.0, "2014-05-28T22:00:00.000+10:00")]

        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 1.0,  "2014-05-29T08:12:00.000+10:00")]

        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 0.10, "2014-05-29T08:13:48.000+10:00")]

        [DataRow("2012-10-19T00:00:00.000+10:00", 180, 1.6,  "2012-10-18T14:00:00.000+00:00")]
        [DataRow("2012-10-19T04:00:00.000+10:00", 180, 1.6,  "2012-10-18T18:00:00.000+00:00")]
        [DataRow("2012-10-19T08:00:00.000+10:00", 180, 1.6,  "2012-10-18T22:00:00.000+00:00")]
        [DataRow("2012-10-19T12:00:00.000+10:00", 180, 1.6,  "2012-10-19T02:00:00.000+00:00")]
        [DataRow("2012-10-19T16:00:00.000+10:00", 180, 1.6,  "2012-10-19T06:00:00.000+00:00")]
        [DataRow("2012-10-19T20:00:00.000+10:00", 180, 1.6,  "2012-10-19T10:00:00.000+00:00")]

        [DataRow("2012-10-19T10:00:00.000+10:00", 180, 1.6,  "2012-10-19T00:00:00.000+00:00")]

        [DataRow("2012-10-19T13:57:36.000+10:00", 180, 1.6,  "2012-10-19T13:55:12.000+10:00")]
        [DataRow("2012-10-19T13:59:59.000+10:00", 180, 1.6,  "2012-10-19T13:55:12.000+10:00")]

        [DataRow("2014-05-27T02:13:58.000+10:00", 180, 3.2, "2014-05-26T16:09:36.000+00:00")]
        [DataRow("2014-05-27T02:09:58.000+10:00", 180, 3.2, "2014-05-26T16:09:36.000+00:00")]
        [DataRow("2014-05-27T02:30:00.000+10:00", 180, 3.2, "2014-05-26T16:28:48.000+00:00")]
        [DataRow("2014-05-27T03:45:00.000+10:00", 180, 3.2, "2014-05-26T17:36:00.000+00:00")]
        [DataRow("2014-05-27T03:45:36.000+10:00", 180, 3.2, "2014-05-26T17:45:36.000+00:00")]
        [DataRow("2014-05-27T03:45:37.000+10:00", 180, 3.2, "2014-05-26T17:45:36.000+00:00")]

        [DataRow("2014-05-27T05:37:20.000+10:00", 180, 7.5, "2014-05-26T19:30:00.000+00:00")]
        [DataRow("2014-05-27T12:00:00.000+10:00", 180, 7.5, "2014-05-27T01:52:30.000+00:00")]
        [DataRow("2014-05-27T01:00:00.000+10:00", 180, 7.5, "2014-05-26T15:00:00.000+00:00")]

        [DataRow("2012-10-19T14:00:00.000+10:00", 180, 240, "2012-10-19T10:00:00.000+10:00")]
        [DataRow("2012-10-19T03:00:00.000+10:00", 180, 240, "2012-10-18T22:00:00.000+10:00")]
        [DataRow("2012-10-19T04:00:00.000+00:00", 180, 240, "2012-10-19T00:00:00.000+00:00")]

        [DataRow("2012-10-19T12:22:30.000+10:00", 180, 120, "2012-10-19T00:00:00.000+00:00")]
        [DataRow("2012-10-19T17:01:00.000+10:00", 180, 120, "2012-10-19T06:00:00.000+00:00")]
        [DataRow("2012-10-19T03:59:59.999+10:00", 180, 120, "2012-10-18T12:00:00.000+00:00")]
        [DataRow("2012-10-19T07:00:00.000+10:00", 180, 120, "2012-10-18T18:00:00.000+00:00")]

        // special test cases
        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 120, "2014-05-28T18:00:00.000+00:00")]
        public void TestGetPreviousTileBoundary(string startDate, int tileWidth, double scale, string expectedDate)
        {
            var start = DateTimeOffset.Parse(startDate);

            var actual = ZoomTiledSpectrograms.GetPreviousTileBoundary(tileWidth, scale, start);

            var expected = DateTimeOffset.Parse(expectedDate);
            Assert.AreEqual(expected, actual);

        }
    }
}
