// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SunCalcTest.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FELT.Tests
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using Microsoft.FSharp.Math;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MQUTeR.FSharp.Shared;

    /// <summary>
    /// This is a test class for SunCalcTest and is intended to contain all SunCalcTest Unit Tests.
    /// </summary>
    [TestClass]
    public class SunCalcTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void TestJulianDate()
        {
            DateTimeOffset dtn = SunCalcTest.Parse("Wed May 02 2012 12:09:00 GMT+1000");
            const double Expected = 2456049.589583333;

            double actual = SunCalc.dateToJulianDate(dtn);

            Assert.AreEqual(Expected, actual, 0.000000005);
        }

        [TestMethod]
        public void TestPhasesAsMap()
        {
            
        }


        [TestMethod]
        public void TestPhaseForTime()
        {

        }

        /// <summary>
        /// A test for the suncalc module. See suncalc.net to setup more tests.
        /// </summary>
        [TestMethod]
        public void TestSuncalc()
        {
            var testDate = Parse("Mon Apr 30 2012 14:30:00 GMT+1000");
            var lat = -27.461165450724938;
            var lng = 152.9699647827149;

            // note: tests for morning / golden hours / afternoon not complete
            // althogh boundaries and noon are tested... only untested bit is the split in goldenhour/morning & afternoon/goldenhour
            var dawn = Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var dusk = Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var mtAstroStart = Parse("Mon Apr 30 2012 04:55:00 GMT+1000");
            var mtAstroEnd = Parse("Mon Apr 30 2012 05:22:24 GMT+1000");
            var mtNautStart = Parse("Mon Apr 30 2012 05:22:24 GMT+1000");
            var mtNautEnd = Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var mtCivilStart = Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var mtCivilEnd = Parse("Mon Apr 30 2012 06:14:11 GMT+1000");
            var etAstroStart = Parse("Mon Apr 30 2012 18:10:39 GMT+1000");
            var etAstroEnd = Parse("Mon Apr 30 2012 18:38:03 GMT+1000");
            var etNautStart = Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var etNautEnd = Parse("Mon Apr 30 2012 18:10:39 GMT+1000");
            var etCivilStart = Parse("Mon Apr 30 2012 17:18:52 GMT+1000");
            var etCivilEnd = Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var transit = Parse("Mon Apr 30 2012 11:46:32 GMT+1000");
            var sunriseStart = Parse("Mon Apr 30 2012 06:14:11 GMT+1000");
            var sunriseEnd = Parse("Mon Apr 30 2012 06:16:41 GMT+1000");
            var sunsetStart = Parse("Mon Apr 30 2012 17:16:23 GMT+1000");
            var sunsetEnd = Parse("Mon Apr 30 2012 17:18:52 GMT+1000");

            // I can't test like this because the calculated datetimes include fractions of a second.
            // I did manually check them, and all values were second accurate... oh well
            /*
            var expectedPhases = new SunCalc.SunPhases(
                dawn,
                new Interval<DateTimeOffset>(sunriseStart, sunriseEnd), 
                transit,
                new Interval<DateTimeOffset>(sunsetStart, sunsetEnd), 
                dusk, 
                new FSharpOption<SunCalc.Twilights>(
                    new SunCalc.Twilights(
                        new Interval<DateTimeOffset>(mtAstroStart, mtAstroEnd),
                        new Interval<DateTimeOffset>(mtNautStart, mtNautEnd),
                        new Interval<DateTimeOffset>(mtCivilStart, mtCivilEnd))), 
                new FSharpOption<SunCalc.Twilights>(
                    new SunCalc.Twilights(
                        new Interval<DateTimeOffset>(etAstroStart, etAstroEnd),
                        new Interval<DateTimeOffset>(etNautStart, etNautEnd),
                        new Interval<DateTimeOffset>(etCivilStart, etCivilEnd))));*/
            var sunPhases = SunCalc.getDayInfo(testDate, lat, lng);

            var a = Minimod.PrettyPrint.PrettyPrintMinimod.PrettyPrint(sunPhases, typeof(Tuple<string, Interval<DateTimeOffset>>[]));
            Debug.WriteLine(a);
            /*
            var e = Minimod.PrettyPrint.PrettyPrintMinimod.PrettyPrint(expectedPhases, typeof(SunCalc.SunPhases));
            
            Debug.WriteLine(e);
            

            Assert.AreEqual(expectedPhases, sunPhases);*/


            Assert.AreEqual(dawn.Ticks, SunCalc.Dawn(sunPhases).Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(dusk.Ticks, SunCalc.Dusk(sunPhases).Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(transit.Ticks, SunCalc.SolarNoon(sunPhases).Ticks, TimeSpan.TicksPerSecond - 1);

            Assert.AreEqual(mtAstroStart.Ticks, sunPhases[SunCalc.DawnAstronomicalTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(mtAstroEnd.Ticks,   sunPhases[SunCalc.DawnAstronomicalTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(mtNautStart.Ticks,  sunPhases[SunCalc.DawnNauticalTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(mtNautEnd.Ticks, sunPhases[SunCalc.DawnNauticalTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(mtCivilStart.Ticks, sunPhases[SunCalc.DawnCivilTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(mtCivilEnd.Ticks, sunPhases[SunCalc.DawnCivilTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etAstroStart.Ticks, sunPhases[SunCalc.EveningAstronomicalTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etAstroEnd.Ticks, sunPhases[SunCalc.EveningAstronomicalTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etNautStart.Ticks,  sunPhases[SunCalc.EveningNauticalTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etNautEnd.Ticks, sunPhases[SunCalc.EveningNauticalTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etCivilStart.Ticks, sunPhases[SunCalc.EveningCivilTwilight].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(etCivilEnd.Ticks, sunPhases[SunCalc.EveningCivilTwilight].Upper.Ticks, TimeSpan.TicksPerSecond - 1);

            Assert.AreEqual(sunriseStart.Ticks, sunPhases[SunCalc.Sunrise].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunriseEnd.Ticks, sunPhases[SunCalc.Sunrise].Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunsetStart.Ticks, sunPhases[SunCalc.Sunset].Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunsetEnd.Ticks, sunPhases[SunCalc.Sunset].Upper.Ticks, TimeSpan.TicksPerSecond - 1);

            /*
            Assert.AreEqual(dawn.Ticks, sunPhases.dawn.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(dusk.Ticks, sunPhases.dusk.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtAstroStart.Ticks,
                sunPhases.morningTwilight.Value.astronomical.Lower.Ticks,
                TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtAstroEnd.Ticks, sunPhases.morningTwilight.Value.astronomical.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtNautStart.Ticks, sunPhases.morningTwilight.Value.nautical.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtNautEnd.Ticks, sunPhases.morningTwilight.Value.nautical.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtCivilStart.Ticks, sunPhases.morningTwilight.Value.civil.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                mtCivilEnd.Ticks, sunPhases.morningTwilight.Value.civil.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etAstroStart.Ticks,
                sunPhases.eveningTwilight.Value.astronomical.Lower.Ticks,
                TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etAstroEnd.Ticks, sunPhases.eveningTwilight.Value.astronomical.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etNautStart.Ticks, sunPhases.eveningTwilight.Value.nautical.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etNautEnd.Ticks, sunPhases.eveningTwilight.Value.nautical.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etCivilStart.Ticks, sunPhases.eveningTwilight.Value.civil.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(
                etCivilEnd.Ticks, sunPhases.eveningTwilight.Value.civil.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(transit.Ticks, sunPhases.transit.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunriseStart.Ticks, sunPhases.sunrise.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunriseEnd.Ticks, sunPhases.sunrise.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunsetStart.Ticks, sunPhases.sunset.Lower.Ticks, TimeSpan.TicksPerSecond - 1);
            Assert.AreEqual(sunsetEnd.Ticks, sunPhases.sunset.Upper.Ticks, TimeSpan.TicksPerSecond - 1);
            */
            /*
             * Object
                dawn: Mon Apr 30 2012 05:50:04 GMT+1000 (E. Australia Standard Time)
                dusk: Mon Apr 30 2012 17:43:00 GMT+1000 (E. Australia Standard Time)
                morningTwilight: Object
                astronomical: Object
                end: Mon Apr 30 2012 05:22:24 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 04:55:00 GMT+1000 (E. Australia Standard Time)
                civil: Object
                end: Mon Apr 30 2012 06:14:11 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 05:50:04 GMT+1000 (E. Australia Standard Time)
                nautical: Object
                end: Mon Apr 30 2012 05:50:04 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 05:22:24 GMT+1000 (E. Australia Standard Time)
                nightTwilight: Object
                astronomical: Object
                end: Mon Apr 30 2012 18:38:03 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 18:10:39 GMT+1000 (E. Australia Standard Time)
                civil: Object
                end: Mon Apr 30 2012 17:43:00 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 17:18:52 GMT+1000 (E. Australia Standard Time)
                nautical: Object
                end: Mon Apr 30 2012 18:10:39 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 17:43:00 GMT+1000 (E. Australia Standard Time)
                sunrise: Object
                end: Mon Apr 30 2012 06:16:41 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 06:14:11 GMT+1000 (E. Australia Standard Time)
                sunset: Object
                end: Mon Apr 30 2012 17:18:52 GMT+1000 (E. Australia Standard Time)
                start: Mon Apr 30 2012 17:16:23 GMT+1000 (E. Australia Standard Time)
                transit: Mon Apr 30 2012 11:46:32 GMT+1000 (E. Australia Standard Time)
             * 
             */
        }

/*      You can use the following additional attributes as you write your tests:
        Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
        }
        Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }
        Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }
        Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
 */
    

    #endregion

        #region Methods

        public static DateTimeOffset Parse(string datestr)
        {
            return DateTimeOffset.ParseExact(
                datestr, "ddd MMM dd yyyy HH:mm:ss 'GMT'zz\\0\\0", CultureInfo.GetCultureInfo("en-AU"));
        }

        #endregion
    }
}