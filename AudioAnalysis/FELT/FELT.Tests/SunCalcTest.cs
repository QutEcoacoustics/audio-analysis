// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SunCalcTest.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   This is a test class for SunCalcTest and is intended
//   to contain all SunCalcTest Unit Tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FELT.Tests
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using Microsoft.FSharp.Core;
    using Microsoft.FSharp.Math;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using MQUTeR.FSharp.Shared;

    /// <summary>
    /// This is a test class for SunCalcTest and is intended
    ///  to contain all SunCalcTest Unit Tests.
    /// </summary>
    [TestClass]
    public class SunCalcTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void TestJulianDate()
        {
            DateTime dtn = this.Parse("Wed May 02 2012 12:09:00 GMT+1000");
            var expected = 2456049.589583333;
                                 
            double actual = SunCalc.dateToJulianDate(dtn);

            Assert.AreEqual(expected, actual, 0.000000005);
        }

        /// <summary>
        /// A test for the suncalc module.
        ///  See suncalc.net to setup more tests.
        /// </summary>
        [TestMethod]
        public void TestSuncalc()
        {
            var testDate = this.Parse("Mon Apr 30 2012 14:30:00 GMT+1000");
            var lat = -27.461165450724938;
            var lng = 152.9699647827149;

            var dawn = this.Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var dusk = this.Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var mtAstroStart = this.Parse("Mon Apr 30 2012 04:55:00 GMT+1000");
            var mtAstroEnd = this.Parse("Mon Apr 30 2012 05:22:24 GMT+1000");
            var mtNautStart = this.Parse("Mon Apr 30 2012 05:22:24 GMT+1000");
            var mtNautEnd = this.Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var mtCivilStart = this.Parse("Mon Apr 30 2012 05:50:04 GMT+1000");
            var mtCivilEnd = this.Parse("Mon Apr 30 2012 06:14:11 GMT+1000");
            var etAstroStart = this.Parse("Mon Apr 30 2012 18:10:39 GMT+1000");
            var etAstroEnd = this.Parse("Mon Apr 30 2012 18:38:03 GMT+1000");
            var etNautStart = this.Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var etNautEnd = this.Parse("Mon Apr 30 2012 18:10:39 GMT+1000");
            var etCivilStart = this.Parse("Mon Apr 30 2012 17:18:52 GMT+1000");
            var etCivilEnd = this.Parse("Mon Apr 30 2012 17:43:00 GMT+1000");
            var transit = this.Parse("Mon Apr 30 2012 11:46:32 GMT+1000");
            var sunriseStart = this.Parse("Mon Apr 30 2012 06:14:11 GMT+1000");
            var sunriseEnd = this.Parse("Mon Apr 30 2012 06:16:41 GMT+1000");
            var sunsetStart = this.Parse("Mon Apr 30 2012 17:16:23 GMT+1000");
            var sunsetEnd = this.Parse("Mon Apr 30 2012 17:18:52 GMT+1000");




            var expectedPhases = new SunCalc.SunPhases(
                dawn, 
                new Interval<DateTime>(sunriseStart, sunriseEnd), 
                transit, 
                new Interval<DateTime>(sunsetStart, sunsetEnd), 
                dusk, 
                new FSharpOption<SunCalc.Twilights>(
                    new SunCalc.Twilights(
                        new Interval<DateTime>(mtAstroStart, mtAstroEnd), 
                        new Interval<DateTime>(mtNautStart, mtNautEnd), 
                        new Interval<DateTime>(mtCivilStart, mtCivilEnd))), 
                new FSharpOption<SunCalc.Twilights>(
                    new SunCalc.Twilights(
                        new Interval<DateTime>(etAstroStart, etAstroEnd), 
                        new Interval<DateTime>(etNautStart, etNautEnd), 
                        new Interval<DateTime>(etCivilStart, etCivilEnd))));

            var sunPhases = SunCalc.getDayInfo(testDate, lat, lng, true);

            var e = Minimod.PrettyPrint.PrettyPrintMinimod.PrettyPrint(expectedPhases, typeof(SunCalc.SunPhases));
            var a = Minimod.PrettyPrint.PrettyPrintMinimod.PrettyPrint(sunPhases, typeof(SunCalc.SunPhases));
            Debug.WriteLine(e);
            Debug.WriteLine(a);

            Assert.AreEqual(expectedPhases, sunPhases);

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

        // You can use the following additional attributes as you write your tests:
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        // }
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        // }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup()
        // {
        // }

        /// <summary>
        /// A test for ic.
        /// </summary>
        /// <typeparam name="a">
        /// </typeparam>
        public void icTestHelper<a>()
        {
            a a1 = default(a); // TODO: Initialize to an appropriate value
            a b = default(a); // TODO: Initialize to an appropriate value
            Interval<a> expected = null; // TODO: Initialize to an appropriate value
            Interval<a> actual;
            actual = SunCalc.ic(a1, b);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        #endregion

        #region Methods

        private DateTime Parse(string datestr)
        {
            return DateTime.ParseExact(datestr, "ddd MMM dd yyyy HH:mm:ss 'GMT'zz\\0\\0", CultureInfo.GetCultureInfo("en-AU"));
        }

        #endregion
    }
}