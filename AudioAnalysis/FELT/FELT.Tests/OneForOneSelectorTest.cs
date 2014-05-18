using FELT.Selectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FELT.Tests
{
    using MQUTeR.FSharp.Shared;

    using Microsoft.FSharp.Collections;

    /// <summary>
    ///This is a test class for OneForOneSelectorTest and is intended
    ///to contain all OneForOneSelectorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OneForOneSelectorTest
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
        ///A test for OneForOneSelector Constructor
        ///</summary>
        [TestMethod()]
        public void OneForOneSelectorConstructorTest()
        {
            OneForOneSelector target = new OneForOneSelector();

            var hdrs = new FSharpMap<string, DataType>(new[] { new Tuple<string, DataType>("name", DataType.Text), new Tuple<string, DataType>("age", DataType.Number) });

            var col1 = new Tuple<string, Value[]>(
                "name", new Value[] { new Text("billy"), new Text("billy"), new Text("ann") });

            var col2 = new Tuple<string, Value[]>(
                "age", new Value[] { new Number(3.0), new Number(4.0), new Number(5.0) });



            var instances = new FSharpMap<string, Value[]>(new[] { col1, col2 });

            var data = new Data(DataSet.Training, hdrs, instances, "Gender", new[] { "Boy", "Boy", "Girl" });

            var result = target.Pick(data);

            Assert.AreEqual(data, result);

            foreach (var kvp in data.Instances)
            {
                var trKey = kvp.Key;

                var testValues = result.Instances[trKey];
                CollectionAssert.AreEqual(kvp.Value, testValues);
            }
        }
    }
}
