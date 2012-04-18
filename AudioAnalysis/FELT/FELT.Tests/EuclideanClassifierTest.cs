using FELT.Classifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MQUTeR.FSharp.Shared;

namespace FELT.Tests
{
    using System.Collections.Generic;

    using Microsoft.FSharp.Collections;

    /// <summary>
    ///This is a test class for EuclideanClassifierTest and is intended
    ///to contain all EuclideanClassifierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EuclideanClassifierTest
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
        ///A test for Classify
        ///</summary>
        [TestMethod()]
        public void ClassifyTest()
        {

            #region setup
            
            var hdrs =
                new FSharpMap<string, DataType>(
                    new[]
                        {
                            new Tuple<string, DataType>("health", DataType.Number),
                            new Tuple<string, DataType>("age", DataType.Number),
                            new Tuple<string, DataType>("skill", DataType.Number)
                        });

            var col1 = new Tuple<string, Value[]>(
                "health", new Value[] { new Number(0.5), new Number(0.8), new Number(0.213) });

            var col2 = new Tuple<string, Value[]>(
                "age", new Value[] { new Number(3), new Number(4), new Number(5) });

            var col3 = new Tuple<string, Value[]>(
                "skill", new Value[] { new Number(8.0), new Number(1.0), new Number(6.75) });


            var instances = new FSharpMap<string, Value[]>(new[] { col1, col2, col3 });

            var trainingData = new Data(DataSet.Training, hdrs, instances, "Profession", new[] { "Electrician", "Athlete", "Smoker" });





            var hdrs2 =
    new FSharpMap<string, DataType>(
        new[]
                        {
                            new Tuple<string, DataType>("health", DataType.Number),
                            new Tuple<string, DataType>("age", DataType.Number),
                            new Tuple<string, DataType>("skill", DataType.Number)
                        });

            var col12 = new Tuple<string, Value[]>(
                "health", new Value[] { new Number(0.9), new Number(0.3), new Number(0.6) });

            var col22 = new Tuple<string, Value[]>(
                "age", new Value[] { new Number(3), new Number(4.6), new Number(2.5) });

            var col32 = new Tuple<string, Value[]>(
                "skill", new Value[] { new Number(0.5), new Number(4.0), new Number(9.0) });


            var instances2 = new FSharpMap<string, Value[]>(new[] { col12, col22, col32 });

            var testData = new Data(DataSet.Test, hdrs2, instances2, "Profession", new[] { "Athlete", "Smoker", "Electrician" });


            Tuple<double, int>[][] expected = new[]
                {
                    new[]
                        {
                            new Tuple<double, int>(1.122497216, 1), new Tuple<double, int>(6.59806555, 2),
                            new Tuple<double, int>(7.510659092, 0)
                        },
                    new[]
                        {
                            new Tuple<double, int>(2.780300164, 2), new Tuple<double, int>(3.1, 1),
                            new Tuple<double, int>(4.312771731, 0)
                        },
                    new[]
                        {
                            new Tuple<double, int>(1.122497216, 0), new Tuple<double, int>(3.385597289, 2),
                            new Tuple<double, int>(8.141867108, 1)
                        }
                };

            #endregion

            EuclideanClassifier classifier = new EuclideanClassifier();

            var actual = classifier.Classify(trainingData, testData);
            var actualEnumerator = ((ClassifierResult.ResultSeq)actual).Item.GetEnumerator();

            for (int i = 0; i < expected.Length; i++)
            {
                var erow = expected[i];

                actualEnumerator.MoveNext();
                var arow = actualEnumerator.Current;

                for (int j = 0; j < erow.Length; j++)
                {
                    var ecell = erow[j];
                    var acell = arow[j];
                    Assert.AreEqual(ecell.Item2, acell.Item2); 
                    Assert.AreEqual(ecell.Item1, acell.Item1,  0.000000005);
                }
            }



        }
    }
}
