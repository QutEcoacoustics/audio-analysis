namespace FELT.Tests
{
    using Microsoft.FSharp.Numerics;

    using Math = MQUTeR.FSharp.Shared.Maths;

    using MQUTeR.FSharp.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This is a test class for MathTest and is intended
    /// to contain all MathTest Unit Tests.
    /// </summary>
    [TestClass]
    public class MathTest
    {
        public const double MinDeltaForDoubleTests = 0.00000000005;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        public void EuclideanDistTest()
        {
            IEnumerable<double> vectorP = new[] { 1.0, 2, 3 };
            IEnumerable<double> vectorQ = new[] { 1.0, 2, 3 };

            double expected = 0.0;
            double actual;
            actual = Math.euclideanDist<double>(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);      
        }

        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        public void EuclideanDistTest2()
        {
            IEnumerable<double> vectorP = new[] { 1.0, 2, 16 };
            IEnumerable<double> vectorQ = new[] { 5.0, -23.3, 3 };


            double expected = 28.724379888867923130;
            double actual;
            actual = Math.euclideanDist<double>(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EuclideanDistTest3()
        {
            IEnumerable<object> vectorP = new object[] { 1.0, 2,  IntegerZ1440.Create(3) };
            IEnumerable<object> vectorQ = new object[] { 1.0, 2, IntegerZ1440.Create(3) };

            double expected = 0.0;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EuclideanDistTest4()
        {
            IEnumerable<object> vectorP = new IComparable[] { 1.0, 2, IntegerZ1440.Create(16) };
            IEnumerable<object> vectorQ = new IComparable[] { 5.0, -23.3, IntegerZ1440.Create(3) };
         
            double expected = 28.724379888867923130;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EuclideanDistTest5()
        {
            IEnumerable<object> vectorP = new IComparable[] { 1000, 9532.6, IntegerZ1440.Create(1430) };
            IEnumerable<object> vectorQ = new IComparable[] { 2000, 8972.3, IntegerZ1440.Create(100) };

            double expected = 1151.53640411409;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);
            Assert.AreEqual(expected, actual, MinDeltaForDoubleTests);
        }

        /// <summary>
        /// A test for euclideanDist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EuclideanDistTest6()
        {
            IEnumerable<object> vectorP = new IComparable[] { IntegerZ1440.Create(1200) };
            IEnumerable<object> vectorQ = new IComparable[] { IntegerZ1440.Create(100) };

            double expected = 340;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);

            Assert.AreEqual(expected, actual);

            IEnumerable<object> vectorU = new IComparable[] { IntegerZ1440.Create(100) };
            IEnumerable<object> vectorV = new IComparable[] { IntegerZ1440.Create(1200) };


            double expected2 = 340;
            double actual2;
            actual2 = Math.euclideanDist(vectorU, vectorV);

            Assert.AreEqual(expected2, actual2);
        }

        /// <summary>
        /// A test for square.
        /// </summary>
        [TestMethod]
        public void SquareTest()
        {
            var input = new[] { -4.5, (8.0 / -3.0), -2, -1, -0.5, 0, 0.5, 1, 2, 8.0 / 3.0, 4.5 };
            var expected = new[] { 20.25, 7 + (1.0 / 9.0), 4, 1, 0.25, 0, 0.25, 1, 4, 7 + (1.0 / 9.0), 20.25 };

            var dotNet = new double[input.Length];
            var mine = new double[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                dotNet[i] = System.Math.Pow(input[i], 2);
                mine[i] = Math.square(input[i]);
            }

            CollectionAssert.AreEqual(expected, mine);
            CollectionAssert.AreEqual(dotNet, mine);
        }


        [TestMethod]
        public void MeanTest()
        {
            var input = new[] { -4.5, (8.0 / -3.0), -2, -1, -0.5, 0, 0.5, 1, 2, 8.0 / 3.0, 100 };
            const double Expected = 8.68181818181818;

            var mine = Math.Array.mean(input);
            Assert.AreEqual(Expected, mine, MinDeltaForDoubleTests);
   
        }
    }
}
