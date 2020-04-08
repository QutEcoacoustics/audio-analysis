// <copyright file="EnumerableExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Extensions
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void TestJoin()
        {
            var items = new[] { 0, 1, 2, 3, 4 };
            var actual = items.Join();

            Assert.AreEqual("0 1 2 3 4", actual);
        }

        [TestMethod]
        public void TestJoinCustomDelimiter()
        {
            var items = new[] { 0, 1, 2, 3, 4 };
            var actual = items.Join(",-,");

            Assert.AreEqual("0,-,1,-,2,-,3,-,4", actual);
        }

        [TestMethod]
        public void TestJoinNonGeneric()
        {
            var items = Enum.GetValues(typeof(MidpointRounding));
            var actual = items.Join();

            Assert.AreEqual("ToEven AwayFromZero ToZero ToNegativeInfinity ToPositiveInfinity", actual);
        }

        [TestMethod]
        public void TestJoinNonGenericCustomDelimiter()
        {
            var items = Enum.GetValues(typeof(MidpointRounding));
            var actual = items.Join("|");

            Assert.AreEqual("ToEven|AwayFromZero|ToZero|ToNegativeInfinity|ToPositiveInfinity", actual);
        }
    }
}