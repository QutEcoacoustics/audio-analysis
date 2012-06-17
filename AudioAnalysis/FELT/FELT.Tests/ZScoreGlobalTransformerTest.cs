using FELT.Classifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MQUTeR.FSharp.Shared;

namespace FELT.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using FELT.Transformers;

    using Microsoft.FSharp.Collections;
    using Microsoft.FSharp.Core;

    /// <summary>
    ///This is a test class for EuclideanClassifierTest and is intended
    ///to contain all EuclideanClassifierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ZScoreGlobalTransformerTest
    {

        public const double Delta = 0.000000005;

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
        public void ClassifyTestLazy()
        {
            Data transformedTrainingData;
            EuclideanClassifier classifier;
            Tuple<double, int>[][] expected;
            Data groupedData;
            Data testData;
            Data transformedTestData;
            var trainingData = ClassifierSetup(
                true,
                out transformedTrainingData,
                out classifier,
                out expected,
                out testData,
                out groupedData,
                out transformedTestData);


            // set up the z-score transformer
            var transformer = new ZScoreNormalise();
            Tuple<Data, Data, FSharpOption<object>> transformedDataActual = transformer.Transform(trainingData, testData);

            // also group the training data
            var grouper = new Trainers.GroupTrainer();
            var groupedActual = grouper.Train(transformedDataActual.Item1); // <- zscore transformed training data



            // _____________________________ trainingData, testData   
            var actual = classifier.Classify(groupedData, transformedDataActual.Item2);

            FSharpFunc<int, Tuple<double, int>[]> func = ((ClassifierResult.Function)actual).Item;
            var length = testData.Classes.Length;

            Assert.IsTrue(length == testData.Instances.First().Value.Length);

            var rs = new List<Tuple<double, int>[]>(length);
            for (int i = 0; i < length; i++)
            {
                rs.Add(func.Invoke(i));
            }

            List<Tuple<double, int>[]>.Enumerator results = rs.GetEnumerator();


            for (int i = 0; i < expected.Length; i++)
            {
                var erow = expected[i];

                results.MoveNext();
                var arow = results.Current;

                for (int j = 0; j < erow.Length; j++)
                {
                    var ecell = erow[j];
                    var acell = arow[j];
                    Assert.AreEqual(ecell.Item2, acell.Item2);
                    Assert.AreEqual(ecell.Item1, acell.Item1, Delta);
                }
            }



        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void GlobalZScoreTransform_InputTest()
        {
            Data transformedTrainingData;
            EuclideanClassifier classifier;
            Tuple<double, int>[][] expected;
            Data groupedData;
            Data testData;
            Data transformedTestData;
            var trainingData = ClassifierSetup(
                true,
                out transformedTrainingData,
                out classifier,
                out expected,
                out testData,
                out groupedData,
                out transformedTestData);




            // set up the z-score transformer
            var transformer = new ZScoreNormalise();

            Tuple<Data, Data, FSharpOption<object>> transformedDataActual = transformer.Transform(testData, trainingData);
        }


        [TestMethod()]
        public void GlobalZScoreTransform_TrainingAndTest()
        {
            Data transformedTrainingData;
            EuclideanClassifier classifier;
            Tuple<double, int>[][] expected;
            Data groupedData;
            Data testData;
            Data transformedTestData;
            var trainingData = ClassifierSetup(
                true,
                out transformedTrainingData,
                out classifier,
                out expected,
                out testData,
                out groupedData,
                out transformedTestData);



            // set up the z-score transformer
            var transformer = new ZScoreNormalise();

            Tuple<Data, Data, FSharpOption<object>> transformedDataActual = transformer.Transform(trainingData, testData);

            AssertTwoDatasEqual(transformedTrainingData, transformedDataActual.Item1);

            AssertTwoDatasEqual(transformedTestData, transformedDataActual.Item2);

            // also group the training data
            var grouper = new Trainers.GroupTrainer();
            var groupedActual = grouper.Train(transformedDataActual.Item1); // <- zscore transformed training data

            AssertTwoDatasEqual(groupedData, groupedActual);
        }


        public static void AssertTwoDatasEqual(Data e, Data a)
        {
            Assert.AreEqual(e.DataSet, a.DataSet);
            Assert.AreEqual(e.ClassHeader, a.ClassHeader);

            CollectionAssert.AreEqual(e.Classes, a.Classes);

            e.Headers.Zip(a.Headers, Tuple.Create).All(
                (t) =>
                    {
                        Assert.AreEqual(t.Item1.Key, t.Item2.Key);
                        Assert.AreEqual(t.Item1.Value, t.Item2.Value);
                        return true;
                    });

            foreach (var instance in e.Instances)
            {
                var colName = instance.Key;

                var eValues = instance.Value.Select(x => ((Number)x).Value).ToArray();
                var aValues = a.Instances[colName].Select(x => ((Number)x).Value).ToArray();

                for (int i = 0; i < eValues.Length; i++)
                {
                    var ecell = eValues[i];
                    var acell = aValues[i];

                    Assert.AreEqual(ecell, acell, Delta);
                }
            }
        }



        private static Data ClassifierSetup(
            bool lazy,
            out Data transformedData,
            out EuclideanClassifier classifier,
            out Tuple<double, int>[][] expected,
            out Data testData,
            out Data groupedData,
            out Data testDataTransformed)
        {
            #region setup

            // training
            var headers =
                new FSharpMap<string, DataType>(
                    new[]
                        {
                            new Tuple<string, DataType>("health", DataType.Number),
                            new Tuple<string, DataType>("age", DataType.Number),
                            new Tuple<string, DataType>("skill", DataType.Number)
                        });

            var hdrs = headers;

            var col1 = new Tuple<string, Value[]>(
                "health", new Value[] { new Number(0.5), new Number(0.7), new Number(0.8), new Number(0.213) });

            var col2 = new Tuple<string, Value[]>(
                "age", new Value[] { new Number(3.0), new Number(2.8), new Number(4), new Number(5) });

            var col3 = new Tuple<string, Value[]>(
                "skill", new Value[] { new Number(8.0), new Number(8.2), new Number(1.0), new Number(6.75) });

            var instances = new FSharpMap<string, Value[]>(new[] { col1, col2, col3 });

            var trainingData = new Data(
                DataSet.Training,
                hdrs,
                instances,
                "Profession",
                new[] { "Electrician", "Electrician", "Athlete", "Smoker" });

            // transformed training
            var hdrs2 = headers;

            // testvalues
            var t = new[,]
                {
                    { -0.237532304, 0.654607806, 1.100677861, -1.517753363 },
                    { -0.797724035, -1.025645188, 0.341881729, 1.481487494 },
                    { 0.686238102, 0.754435678, -1.700677036, 0.260003256 },
                };

            var col12 = new Tuple<string, Value[]>("health", ArrayToNumbers(t, 0));

            var col22 = new Tuple<string, Value[]>("age", ArrayToNumbers(t, 1));

            var col32 = new Tuple<string, Value[]>("skill", ArrayToNumbers(t, 2));

            var instances2 = new FSharpMap<string, Value[]>(new[] { col12, col22, col32 });

            transformedData = new Data(
                DataSet.Training,
                hdrs2,
                instances2,
                "Profession",
                new[] { "Electrician", "Electrician", "Athlete", "Smoker" });


            // training data-set (after grouping)
            var hdrs3 = headers;

            // grouped values
            var t3 = new[,]
                {
                    { 1.100677861, 0.208537751, -1.517753363 }, { 0.341881729, -0.911684612, 1.481487494 },
                    { -1.700677036, 0.72033689, 0.260003256 },
                };

            var col13 = new Tuple<string, Value[]>("health", ArrayToNumbers(t3, 0));

            var col23 = new Tuple<string, Value[]>("age", ArrayToNumbers(t3, 1));

            var col33 = new Tuple<string, Value[]>("skill", ArrayToNumbers(t3, 2));

            var instances3 = new FSharpMap<string, Value[]>(new[] { col13, col23, col33 });

            groupedData = new Data(
                DataSet.Training, hdrs2, instances3, "Profession", new[] { "Athlete", "Electrician", "Smoker" });



            // test data 
            var hdrs8 = hdrs;

            //      a,s,e
            // h,0.9,0.3,0.6
            // a,3,4.6,2.5
            // s,0.5,4,9

            var nums8 = new[,] { { 0.9, 0.3, 0.6 }, { 3, 4.6, 2.5 }, { 0.5, 4, 9 } };

            var col18 = new Tuple<string, Value[]>("health", ArrayToNumbers(nums8, 0));

            var col28 = new Tuple<string, Value[]>("age", ArrayToNumbers(nums8, 1));

            var col38 = new Tuple<string, Value[]>("skill", ArrayToNumbers(nums8, 2));

            var instances8 = new FSharpMap<string, Value[]>(new[] { col18, col28, col38 });

            testData = new Data(
                DataSet.Test, hdrs2, instances8, "Profession", new[] { "Athlete", "Smoker", "Electrician" });

            // test data (transformed by z-scores kept from training groups
            var hdrs4 = hdrs;

            //      a,s,e
            // h,0.9,0.3,0.6
            // a,3,4.6,2.5
            // s,0.5,4,9

            var zScoreNums = new[,]
                {
                    { 1.546747916, -1.129672415, 0.208537751 }, { -0.797724035, 1.025645188, -1.367526918 },
                    { -1.871170975, -0.677713405, 1.027225979 }
                };



            var col14 = new Tuple<string, Value[]>("health", ArrayToNumbers(zScoreNums, 0));

            var col24 = new Tuple<string, Value[]>("age", ArrayToNumbers(zScoreNums, 1));

            var col34 = new Tuple<string, Value[]>("skill", ArrayToNumbers(zScoreNums, 2));

            var instances4 = new FSharpMap<string, Value[]>(new[] { col14, col24, col34 });

            testDataTransformed = new Data(
                DataSet.Test, hdrs2, instances4, "Profession", new[] { "Athlete", "Smoker", "Electrician" });

            // finally, the results we expect!
            expected = new[]
                {
                    new[]
                        {
                            new Tuple<double, int>(1.235616436, 0), new Tuple<double, int>(2.918853623, 1),
                            new Tuple<double, int>(4.37354283, 2)
                        },
                    new[]
                        {
                            new Tuple<double, int>(1.112524862, 2), new Tuple<double, int>(2.547243493, 0),
                            new Tuple<double, int>(2.738356775, 1)
                        },
                    new[]
                        {
                            new Tuple<double, int>(0.54952081, 1), new Tuple<double, int>(3.340575813, 0),
                            new Tuple<double, int>(3.418419932, 2)
                        }
                };

            #endregion

            classifier = new EuclideanClassifier(new Microsoft.FSharp.Core.FSharpOption<bool>(lazy));
            return trainingData;
        }

        private static Value[] ArrayToNumbers(double[,] arr, int row)
        {
            int length = arr.GetLength(1);
            var result = new Value[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = new Number(arr[row, i]);
            }

            return result;
        }
    }
}
