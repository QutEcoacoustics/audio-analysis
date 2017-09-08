// <copyright file="RangeTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void EqualityOperatorWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(5, 10);
            var c = new Range<double>(5, 10.004);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
        }

        [TestMethod]
        public void InequalityOperatorWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(5, 10);
            var c = new Range<double>(5, 10.004);

            Assert.IsTrue(a != c);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        public void GetHashCodeWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(5, 10);
            var c = new Range<double>(5, 10.004);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
        }

        [TestMethod]
        public void ToStringWorks()
        {
            var a = new Range<double>(5, 10);
            var c = new Range<double>(5, 10.004);

            Assert.AreEqual("Range: 5 - 10", a.ToString());
            Assert.AreEqual("Range: 5 - 10.004", c.ToString());
        }

        [DataTestMethod]
        [DataRow(-1, 20, 10, 15, -1)]
        [DataRow(12, 20, 10, 15, 1)]
        [DataRow(-3, -3, -3, -3, 0)]
        [DataRow(5, 15, 5, 30, -1)]
        [DataRow(5, 50, 5, 30, 1)]
        public void CompareToWorks(double a1, double a2, double b1, double b2, int order)
        {
            var a = (a1, a2).AsRange();
            var b = (b1, b2).AsRange();

            Assert.AreEqual(order, a.CompareTo(b));
        }

        [TestMethod]
        public void DoubleCenterWorks()
        {
            var a = new Range<double>(5, 10);

            Assert.AreEqual(7.5, a.Center());
        }

        [TestMethod]
        public void DoubleMagnitudeWorks()
        {
            var a = new Range<double>(5, 10);

            Assert.AreEqual(5.0, a.Magnitude());
        }

        [DataTestMethod]
        [DataRow(57.5, 62.5, 00, 120, null, 27.5, 92.5)]
        [DataRow(10.0, 20.0, 00, 120, null, 0, 70)]
        [DataRow(110, 115.0, 00, 120, null, 55, 120)]
        [DataRow(15.0, 25.0, 00, 060, null, 0, 60)]
        [DataRow(30.0, 40.0, 20, 050, null, 20, 50)]
        [DataRow(60.0, 540,  00, 600, null, 30, 570)]
        [DataRow(37.123, 39.999, 0, 120, 0, 7, 70)]
        [DataRow(37.123, 39.999, 0, 120, 1, 7.1, 70)]
        public void DoubleGrowWorks(double a1, double a2, double b1, double b2, int? roundDigits, double c1, double c2)
        {
            var target = a1.To(a2);
            var limit = b1.To(b2);
            var expected = c1.To(c2);

            var actual = target.Grow(limit, 60.0, roundDigits);

            Assert.AreEqual(expected, actual);
        }
    }
}
