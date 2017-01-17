// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayExtensionsTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Summary description for ArrayExtensionsTests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Summary description for ArrayExtensionsTests
    /// </summary>
    [TestClass]
    public class ArrayExtensionsTests
    {
        public ArrayExtensionsTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private const int TestSize = 1000000;

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
