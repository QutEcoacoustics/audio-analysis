// <copyright file="DateTimeAndTimeSpanExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Accord.Math;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static System.DateTimeAndTimeSpanExtensions;

    [TestClass]
    public class DateTimeAndTimeSpanExtensions
    {
        [DataTestMethod]
        [DataRow("2019-04-08T11:30:00+10:00", RoundingDirection.AwayFromZero, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T12:57:33+10:00", RoundingDirection.AwayFromZero, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T09:57:33+10:00", RoundingDirection.AwayFromZero, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T23:45:33+10:00", RoundingDirection.AwayFromZero, "2019-04-09T11:30:00+10:00")]
        [DataRow("2019-04-07T23:15:33+10:00", RoundingDirection.AwayFromZero, "2019-04-07T11:30:00+10:00")]

        [DataRow("2019-04-08T11:30:00+10:00", RoundingDirection.Floor, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T12:57:33+10:00", RoundingDirection.Floor, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T09:57:33+10:00", RoundingDirection.Floor, "2019-04-07T11:30:00+10:00")]
        [DataRow("2019-04-08T23:45:33+10:00", RoundingDirection.Floor, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-07T23:15:33+10:00", RoundingDirection.Floor, "2019-04-07T11:30:00+10:00")]

        [DataRow("2019-04-08T11:30:00+10:00", RoundingDirection.Ceiling, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T12:57:33+10:00", RoundingDirection.Ceiling, "2019-04-09T11:30:00+10:00")]
        [DataRow("2019-04-08T09:57:33+10:00", RoundingDirection.Ceiling, "2019-04-08T11:30:00+10:00")]
        [DataRow("2019-04-08T23:45:33+10:00", RoundingDirection.Ceiling, "2019-04-09T11:30:00+10:00")]
        [DataRow("2019-04-07T23:15:33+10:00", RoundingDirection.Ceiling, "2019-04-08T11:30:00+10:00")]
        public void TestRoundToTimeOfDay(string test, RoundingDirection direction, string expected)
        {
            var testDate = DateTimeOffset.ParseExact(test, AppConfigHelper.Iso8601FormatNoFractionalSeconds, CultureInfo.InvariantCulture);
            var expectedDate = DateTimeOffset.ParseExact(expected, AppConfigHelper.Iso8601FormatNoFractionalSeconds, CultureInfo.InvariantCulture);

            var actual = testDate.RoundToTimeOfDay(new TimeSpan(11, 30, 0), direction);

            Assert.AreEqual(expectedDate, actual);
        }
    }
}
