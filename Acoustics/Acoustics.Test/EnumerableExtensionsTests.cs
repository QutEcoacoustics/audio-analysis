// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the EnumerableExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumerableExtensionsTests
    {

        public class DummyData
        {
            public double[] Field1 { get; set; }

            public double[] Field2 { get; set; }
        }

        private IList<DummyData> dummyDatas;

        private readonly Dictionary<string, Func<DummyData, double[]>> selectors =
            new Dictionary<string, Func<DummyData, double[]>>()
                {
                    { "Field1", (dd => dd.Field1) },
                    { "Field2", (dd => dd.Field2) }
                };


        [TestInitialize]
        public void SetupData()
        {
            this.dummyDatas = new[]
                                  {
                                      new DummyData()
                                          {
                                              Field1 = new[] { 0.0, 1, 2, 3, 4, },
                                              Field2 = new[] { 5.0, 6, 7, 8, 9, }
                                          },
                                      new DummyData()
                                          {
                                              Field1 = new[] { 10.0, 11, 12, 13, 14 },
                                              Field2 = new[] { 15.0, 16, 17, 18, 19 }
                                          },
                                  };
        }


        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 0.0, 1, 2, 3, 4, 10.0, 11, 12, 13, 14 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 5.0, 6, 7, 8, 9, 15.0, 16, 17, 18, 19 };
            Test2DArray(field2, expected2);

            TestDims(field1, 2, 5);
        }

        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest_ColumnMajor()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors, TwoDimensionalArray.ColumnMajor);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 0.0, 10.0, 1, 11, 2, 12, 3, 13, 4, 14 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 5.0, 15.0, 6, 16, 7, 17, 8, 18, 9, 19 };
            Test2DArray(field2, expected2);

            TestDims(field1, 5, 2);

        }

        [TestMethod]
        public void EnumerableToDictionaryOfMatriciesTest_ColumnMajorFlipped()
        {
            var result = this.dummyDatas.ToTwoDimensionalArray(this.selectors, TwoDimensionalArray.ColumnMajorFlipped);

            Assert.AreEqual(2, result.Count);

            var field1 = result["Field1"];
            var expected = new[] { 4, 14, 3, 13, 2, 12, 1, 11, 0.0, 10.0 };
            Test2DArray(field1, expected);

            var field2 = result["Field2"];
            var expected2 = new[] { 9, 19, 8, 18, 7, 17, 6, 16, 5.0, 15.0 };
            Test2DArray(field2, expected2);

            TestDims(field1, 5, 2);

        }

        private static void TestDims(double[,] matrix, int rows, int columns)
        {
            Assert.AreEqual(rows, matrix.GetLength(0));
            Assert.AreEqual(columns, matrix.GetLength(1));
        }

        private static void Test2DArray(double[,] matrix, double[] expected)
        {
            Assert.IsNotNull(matrix);
            Assert.AreEqual(2, matrix.Rank);

            var index = 0;
            foreach (var d in matrix)
            {
                Assert.AreEqual(expected[index], d);
                index++;
            }
        }
    }
}
