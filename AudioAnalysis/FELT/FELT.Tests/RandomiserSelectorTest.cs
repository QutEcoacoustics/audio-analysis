using FELT.Selectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FELT.Tests
{
    using System.Diagnostics;

    using System.Linq;

    using MQUTeR.FSharp.Shared;

    /// <summary>
    ///This is a test class for RandomiserSelectorTest and is intended
    ///to contain all RandomiserSelectorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RandomiserSelectorTest
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
        ///A test for RandomiserSelector Constructor
        ///</summary>
        [TestMethod()]
        public void RandomiserSelectorConstructorTest()
        {
            RandomiserSelector target = new RandomiserSelector();

            var seed = new[] { "Hello", "I'm", "not", "sure", "what", "I", "want", "to", "wear", "today" };
            CollectionAssert.AllItemsAreUnique(seed);

            var d = new Data(DataSet.Training, null, null, null, seed);
            var result = target.Pick(d);

            foreach (string s in result.Classes)
            {
                Debug.Print(s);
            }
            CollectionAssert.AllItemsAreUnique(result.Classes);
            CollectionAssert.AreEquivalent(seed, result.Classes);

            CollectionAssert.AreNotEqual(seed, result.Classes);
        }

        [TestMethod]
        public void FuzzyBitTest()
        {
            var bit = new FuzzyBit(0.0);

            var bit1 = new FuzzyBit(0.5);

            var bit2 = new FuzzyBit(1.0);
            
        }


        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void FuzzyBitTestEx()
        {
            var bit = new FuzzyBit(-0.5);

        }
    }
}
