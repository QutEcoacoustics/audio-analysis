using MQUTeR.FSharp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FELT.Tests
{
    using Math = MQUTeR.FSharp.Shared.Maths;

    /// <summary>
    ///This is a test class for MathTest and is intended
    ///to contain all MathTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MathTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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
        ///A test for euclideanDist
        ///</summary>
        [TestMethod()]
        public void euclideanDistTest()
        {
            IEnumerable<double> vectorP = new [] { 1.0 , 2 , 3};
            IEnumerable<double> vectorQ = new[] { 1.0, 2, 3 };


            double expected = 0.0;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);
            
        }

        [TestMethod()]
        public void euclideanDistTest2()
        {
            IEnumerable<double> vectorP = new[] { 1.0, 2, 16 };
            IEnumerable<double> vectorQ = new[] { 5.0, -23.3, 3 };


            double expected = 28.724379888867923130;
            double actual;
            actual = Math.euclideanDist(vectorP, vectorQ);
            Assert.AreEqual(expected, actual);

        }


        /// <summary>
        ///A test for square
        ///</summary>
        [TestMethod()]
        public void squareTest()
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
    }
}
