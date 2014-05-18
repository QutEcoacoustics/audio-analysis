using FELT.Transformers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using QuickGraph;
using QuickGraph.Algorithms;
using MQUTeR.FSharp.Shared;
using Microsoft.FSharp.Collections;
using System.Linq;

namespace FELT.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Minimod.PrettyPrint;

    using QuickGraph.Algorithms.Observers;

    /// <summary>
    ///This is a test class for TimeOfDayTest and is intended
    ///to contain all TimeOfDayTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TimeOfDayTest
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


        [TestMethod]
        public void TestPhasesGraph()
        {
            UndirectedGraph<string, UndirectedEdge<string>> todg = TimeOfDay.timeOfDayGraph;

            Assert.IsTrue(todg.AllowParallelEdges);

            var testPhase = SunCalc.DawnAstronomicalTwilight;
            
            var distances = todg.ShortestPathsDijkstra(x => 1.0, testPhase);

            IEnumerable<UndirectedEdge<string>> dist1;
            Assert.IsTrue(distances(SunCalc.Sunrise, out dist1)); // TimeOfDay.phases[3]
            Assert.AreEqual(3, dist1.Count());

            IEnumerable<UndirectedEdge<string>> dist2;
            Assert.IsTrue(distances(SunCalc.EveningCivilTwilight, out dist2));
            Assert.AreEqual(4, dist2.Count());



            // todo - equAL LENGTH
            ////Assert.AreEqual(2, algo.ComputedShortestPathCount);

        }


        [TestMethod]
        public void TestPhaseOfDay()
        {
            const string PhaseColName = "PhaseOfDay";

            var testDates = new[]
                {
                    SunCalcTest.Parse("Mon Apr 30 2012 02:30:00 GMT+1000"),
                    SunCalcTest.Parse("Mon Apr 30 2012 06:15:00 GMT+1000"),
                    SunCalcTest.Parse("Mon Apr 30 2012 14:30:00 GMT+1000"),
                    SunCalcTest.Parse("Mon Apr 30 2012 22:30:00 GMT+1000"),
                    SunCalcTest.Parse("Mon Apr 30 2012 17:30:00 GMT+1000")
                };
            var expectedPhases = new[]
                {
                    SunCalc.Night,
                    SunCalc.Sunrise,
                    SunCalc.Afternoon,
                    SunCalc.Night,
                    SunCalc.EveningCivilTwilight
                };

            var lat = new Number(-27.461165450724938);
            var lng = new Number(152.9699647827149);

            var hdrs =
                new FSharpMap<string, DataType>(
                    new[]
                        {
                            new Tuple<string, DataType>("createdDate", DataType.Date),
                            new Tuple<string, DataType>("Latitude", DataType.Number),
                            new Tuple<string, DataType>("Longitude", DataType.Number)
                        });

            var col1 = new Tuple<string, Value[]>(
                "createdDate", testDates.Select(x => new Date(x)).ToArray());

            var col2 = new Tuple<string, Value[]>("Latitude", new Value[] { lat, lat, lat, lat, lat });

            var col3 = new Tuple<string, Value[]>("Longitude", new Value[] { lng, lng, lng, lng, lng });

            var instances = new FSharpMap<string, Value[]>(new[] { col1, col2, col3 });

            var trainingData = new Data(
                DataSet.Training, hdrs, instances, "Class", new[] { "A", "B", "C", "D", "E" });
            var testData = new Data(
                DataSet.Test, hdrs, instances, "Class", (new[] { "A", "B", "C", "D", "E" }));

            // epected
            var hdrs2 =
                new FSharpMap<string, DataType>(
                    new[]
                        {
                            new Tuple<string, DataType>(PhaseColName, DataType.Text)
                        });

            var col12 = new Tuple<string, Value[]>(
                PhaseColName, expectedPhases.Select(x => new Text(x)).ToArray());


            var instances2 = new FSharpMap<string, Value[]>(new[] { col12 });

            var expectedTraining = new Data(
                DataSet.Training, hdrs2, instances2, "Class", new[] { "A", "B", "C", "D", "E" });
            var expectedTest = new Data(
                DataSet.Test, hdrs2, instances2, "Class", new[] { "A", "B", "C", "D", "E" });


            // tests
            var tf = new TimeOfDay.DayPhaseTransformer("Latitude", "Longitude", "createdDate", PhaseColName);

            var results = tf.Transform(trainingData, testData);

            Debug.WriteLine(results.PrettyPrint());
            Debug.WriteLine(expectedTraining.PrettyPrint());

//            Assert.AreEqual(expectedTest, results.Item1);
//            Assert.AreEqual(expectedTraining, results.Item2);

            var tr = results.Item1;
            var te = results.Item2;
            Assert.AreEqual(DataSet.Training, tr.DataSet);
            Assert.AreEqual(DataSet.Test, te.DataSet);

            CollectionAssert.AreEqual(Map.keys(expectedTraining.Headers), Map.keys(tr.Headers));
            CollectionAssert.AreEqual(Map.keys(expectedTest.Headers), Map.keys(te.Headers));


            CollectionAssert.AreEqual(expectedTest.Instances[PhaseColName], te.Instances[PhaseColName]);
            CollectionAssert.AreEqual(expectedTraining.Instances[PhaseColName], tr.Instances[PhaseColName]);


            CollectionAssert.AreEqual(expectedTraining.Classes, tr.Classes);
            CollectionAssert.AreEqual(expectedTest.Classes, te.Classes);
        }
    }
}
