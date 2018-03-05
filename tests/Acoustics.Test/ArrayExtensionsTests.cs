// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Summary description for ArrayExtensionsTests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ArrayExtensionsTests
    {
        private const int TestSize = 1000000;
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }

            set
            {
                this.testContextInstance = value;
            }
        }

        [TestMethod]
        public void TestArrayFill()
        {
            var testArray = new double[TestSize];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int n = 0; n < 10; n++)
            {
                testArray.Fill(4.123456);
            }

            stopwatch.Stop();
            Debug.WriteLine("Time taken:" + stopwatch.Elapsed);

            Assert.IsTrue(testArray.All(value => value == 4.123456));
        }

        [TestMethod]
        public void TestArrayFastFill()
        {
            var testArray = new double[TestSize];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int n = 0; n < 10; n++)
            {
                testArray.FastFill(4.123456);
            }

            stopwatch.Stop();
            Debug.WriteLine("Time taken:" + stopwatch.Elapsed);

            Assert.IsTrue(testArray.All(value => value == 4.123456));
        }
    }
}
