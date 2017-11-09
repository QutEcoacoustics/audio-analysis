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
        [DataTestMethod]
        [DataRow("2017-11-08T13:33:33.123+10:00", 180, 60.0, "2017-11-08T12:00:00.000+10:00")]
        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 60.0, "2014-05-29T06:00:00.000+10:00")]
        [DataRow("2014-05-29T08:13:58.000+10:00", 180, 01.0, "2014-05-29T08:12:00.000+10:00")]
        public void TestGetPreviousTileBoundary(string startDate, int tileWidth, double scale, string expectedDate)
        {
            var start = DateTimeOffset.Parse(startDate);

            var actual = ZoomTiledSpectrograms.GetPreviousTileBoundary(tileWidth, scale, start);

            var expected = DateTimeOffset.Parse(expectedDate);
            Assert.AreEqual(expected, actual);
        }
    }
}
