// <copyright file="MatrixToolsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TowseyLibrary
{
    using System.Collections.Generic;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MatrixToolsTests
    {
        [TestMethod]
        public void ConcatenateMatrixRows()
        {
            double[,] matrix1 =
            {
                { 1.0, 4.0 },
                { 2.0, 5.0 },
                { 3.0, 6.0 },
            };

            double[,] matrix2 =
            {
                { 7.0, 10.0 },
                { 8.0, 11.0 },
                { 9.0, 12.0 },
            };

            double[,] expected =
            {
                { 1.0, 4.0 },
                { 2.0, 5.0 },
                { 3.0, 6.0 },
                { 7.0, 10.0 },
                { 8.0, 11.0 },
                { 9.0, 12.0 },
            };

            var actual = MatrixTools.ConcatenateMatrixRows(matrix1, matrix2);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConcatenateTwoMatrices()
        {
            double[,] matrix1 =
            {
                { 1.0, 3.0, 5.0 },
                { 2.0, 4.0, 6.0 },
            };

            double[,] matrix2 =
            {
                { 7.0, 9.0 },
                { 8.0, 10.0 },
            };

            double[,] expected =
            {
                { 1.0, 3.0, 5.0, 7.0, 9.0 },
                { 2.0, 4.0, 6.0, 8.0, 10.0 },
            };

            var actual = MatrixTools.ConcatenateTwoMatrices(matrix1, matrix2);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}