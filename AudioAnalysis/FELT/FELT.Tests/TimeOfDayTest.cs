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

            var testPhase = TimeOfDay.phases[1];
            
            var distances = todg.ShortestPathsDijkstra(x => 1.0, testPhase);

            IEnumerable<UndirectedEdge<string>> dist1;
            Assert.IsTrue(distances(TimeOfDay.phases[3], out dist1));
            Assert.AreEqual(2, dist1.Count());

            IEnumerable<UndirectedEdge<string>> dist2;
            Assert.IsTrue(distances(TimeOfDay.phases.Reverse().Skip(2).First(), out dist2));
            Assert.AreEqual(4, dist2.Count());



            // todo - equAL LENGTH
            ////Assert.AreEqual(2, algo.ComputedShortestPathCount);

        }
    }
}
