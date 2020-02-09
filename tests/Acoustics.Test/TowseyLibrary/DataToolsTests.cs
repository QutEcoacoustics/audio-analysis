// <copyright file="DataToolsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TowseyLibrary
{
    using System.Collections.Generic;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataToolsTests
    {
        [TestMethod]
        public void TestConcatenateVectors()
        {
            var a = new[] { 1.0, 1.5, 2.0 };
            var b = new[] { 10.0, 15.5, 22.0, 33.3 };
            var c = new[] { -30, double.PositiveInfinity, 0.0 };

            var expected = new[] { 1.0, 1.5, 2.0, 10.0, 15.5, 22.0, 33.3, -30, double.PositiveInfinity, 0.0 };

            var actual = DataTools.ConcatenateVectors(a, b, c);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConcatenateVectorsOverload()
        {
            var a = new[] { 1.0, 1.5, 2.0 };
            var b = new[] { 10.0, 15.5, 22.0, 33.3 };
            var c = new[] { -30, double.PositiveInfinity, 0.0 };

            var expected = DataTools.ConcatenateVectors(new List<double[]> { a, b, c });

            var actual = DataTools.ConcatenateVectors(a, b, c);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
