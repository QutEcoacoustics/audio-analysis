// <copyright file="RangeTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Diagnostics;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void EnsuresMinimumIsLessThanMaximum()
        {
            Assert.ThrowsException<ArgumentException>(
                () =>
                {
                    new Range<int>(10, 3);
                });

            Assert.ThrowsException<ArgumentException>(
                () =>
                {
                    new Range<double>(-10, -40);
                });

            Assert.ThrowsException<ArgumentException>(
                () =>
                {
                    new Range<TimeSpan>(10.Seconds(), 3.Seconds());
                });

            Assert.ThrowsException<ArgumentException>(
                () =>
                {
                    new Range<double>(double.Epsilon, 0.0);
                });
        }

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
            var b = new Range<double>(5, 10, Topology.Open);
            var c = new Range<double>(5, 10.004, Topology.Closed);
            var d = new Range<double>(5, 10.004, Topology.LeftOpenRightClosed);

            Assert.AreEqual("Range: [5, 10)", a.ToString());
            Assert.AreEqual("Range: (5, 10)", b.ToString());
            Assert.AreEqual("Range: [5, 10.004]", c.ToString());
            Assert.AreEqual("Range: (5, 10.004]", d.ToString());
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
        public void TimeSpanCenterWorks()
        {
            var a = new Range<double>(5, 10);

            Assert.AreEqual(7.5, a.Center());
        }

        [TestMethod]
        public void DoubleSizeWorks()
        {
            var a = new Range<double>(5, 10);

            Assert.AreEqual(5.0, a.Size());
        }

        [TestMethod]
        public void TimeSpanSizeWorks()
        {
            var a = new Range<TimeSpan>(5.Seconds(), 10.Seconds());

            Assert.AreEqual(5.0.Seconds(), a.Size());
        }

        [TestMethod]
        public void DoubleShiftWorks()
        {
            var a = new Range<double>(5, 10);

            Assert.AreEqual((20.0, 25.0).AsRange(), a.Shift(15));
            Assert.AreEqual((-310.0, -305.0).AsRange(), a.Shift(-315));
        }

        [TestMethod]
        public void TimeSpanShiftWorks()
        {
            var a = new Range<TimeSpan>(5.Seconds(), 10.Seconds());

            Assert.AreEqual((20.Seconds(), 25.Seconds()).AsRange(), a.Shift(15.Seconds()));
            Assert.AreEqual((-310.Seconds(), -305.Seconds()).AsRange(), a.Shift(-315.Seconds()));
        }

        [TestMethod]
        public void DoubleAddWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(3, 15);
            var c = new Range<double>(15, 100);

            Assert.AreEqual((8.0, 25.0).AsRange(), a.Add(b));
            Assert.AreEqual((20.0, 110.0).AsRange(), a.Add(c));
        }

        [TestMethod]
        public void DoubleSubtractWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(3, 15);
            var c = new Range<double>(15, 100);

            Assert.AreEqual((-10.0, 7.0).AsRange(), a.Subtract(b));
            Assert.AreEqual((-95.0, -5.0).AsRange(), a.Subtract(c));
        }

        [TestMethod]
        public void DoubleMultiplyWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(3, 15);
            var c = new Range<double>(15, 100);

            Assert.AreEqual((15.0, 150.0).AsRange(), a.Multiply(b));
            Assert.AreEqual((75.0, 1000.0).AsRange(), a.Multiply(c));
        }

        [TestMethod]
        public void DoubleDivideWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(3, 15); // [1/15, 1/3]
            var c = new Range<double>(15, 100); // [1/100, 1/15]

            var abActual = a.Divide(b);
            var abExpected = (1 / 3.0, 10 / 3.0).AsRange();
            Assert.AreEqual(abExpected.Minimum, abActual.Minimum, 0.0000000000001);
            Assert.AreEqual(abExpected.Maximum, abActual.Maximum, 0.0000000000001);
            var acActual = a.Divide(c);
            var acExpected = (0.05, 10.0 / 15.0).AsRange();
            Assert.AreEqual(acExpected.Minimum, acActual.Minimum, 0.0000000000001);
            Assert.AreEqual(acExpected.Maximum, acActual.Maximum, 0.0000000000001);
        }

        [TestMethod]
        public void DoubleInvertWorks()
        {
            var a = new Range<double>(5, 10);
            var b = new Range<double>(-3, 0);
            var c = new Range<double>(0, 100);

            Assert.AreEqual((1.0 / 10, 1.0 / 5).AsRange(), a.Invert());
            Assert.AreEqual((double.NegativeInfinity, 1.0 / -3).AsRange(), b.Invert());
            Assert.AreEqual((1.0 / 100.0, double.PositiveInfinity).AsRange(), c.Invert());
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

        // expected magnitude is size (1.232) plus growth, subtracted from end limit.
#pragma warning disable SA1139 // Use literal suffix notation instead of casting
        [DataRow(3593.853, 3595.085, 0.0, 3595, 0,  3535 - (int)1.232, 3595)]
#pragma warning restore SA1139 // Use literal suffix notation instead of casting
        public void DoubleGrowWorks(double a1, double a2, double b1, double b2, int? roundDigits, double c1, double c2)
        {
            var target = a1.To(a2);
            var limit = b1.To(b2);
            var expected = c1.To(c2);

            var actual = target.Grow(limit, 60.0, roundDigits);

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow(100, 300, 150, null, true)]
        [DataRow(100, 300, 700, null, false)]
        [DataRow(100, 300, 100, null, true)]
        [DataRow(100, 300, 300, null, false)]
        [DataRow(100, 300, 100, Topology.Closed, true)]
        [DataRow(100, 300, 300, Topology.Closed, true)]
        [DataRow(100, 300, 100, Topology.Open, false)]
        [DataRow(100, 300, 300, Topology.Open, false)]
        [DataRow(100, 300, 100, Topology.LeftClosedRightOpen, true)]
        [DataRow(100, 300, 300, Topology.LeftClosedRightOpen, false)]
        [DataRow(100, 300, 100, Topology.LeftOpenRightClosed, false)]
        [DataRow(100, 300, 300, Topology.LeftOpenRightClosed, true)]
        public void DoubleContainsWorks(double a1, double a2, double scalar, Topology? type, bool result)
        {
            var range = a1.To(a2, type ?? Topology.Default);

            var actual = type.HasValue ? range.Contains(scalar, type.Value) : range.Contains(scalar);

            Assert.AreEqual(result, actual);
        }

        [DataTestMethod]
        [DataRow(100, 300, 150, 400, null, true)]
        [DataRow(100, 300, 300, 400, null, true)]
        [DataRow(100, 300, 400, 500, null, false)]
        [DataRow(100, 300, -600, 50, null, false)]
        [DataRow(300, 400, double.NegativeInfinity, double.PositiveInfinity, null, true)]
        [DataRow(100, 300, 300, 400, Topology.Closed, true)]
        [DataRow(200, 400, 100, 200, Topology.Closed, true)]
        [DataRow(200, 400, 300, 350, Topology.Closed, true)]
        [DataRow(200, 400, 100, 500, Topology.Closed, true)]
        [DataRow(100, 300, 200, 400, Topology.Open, true)]
        [DataRow(400, 700, 300, 400, Topology.Open, false)]
        [DataRow(100, 300, -50, 100, Topology.LeftClosedRightOpen, true)]
        [DataRow(100, 300, 300, 400, Topology.LeftClosedRightOpen, true)]
        [DataRow(100, 300, -50, 100, Topology.LeftOpenRightClosed, false)]
        [DataRow(100, 300, 300, 400, Topology.LeftOpenRightClosed, true)]
        public void DoubleIntersectsWithWorks(double a1, double a2, double b1, double b2, Topology? type, bool result)
        {
            var a = a1.To(a2, type ?? Topology.Default);
            var b = b1.To(b2);

            var actual = a.IntersectsWith(b);

            Assert.AreEqual(result, actual);

            // intersect is commutative - double check here
            if (type == Topology.Closed)
            {
                var actualCommutative = b.IntersectsWith(a);

                Assert.AreEqual(result, actualCommutative);
            }
        }

        [DataTestMethod]
        [DataRow(100, 300, 150, 400, null, false, false)]
        [DataRow(100, 300, 300, 400, null, false, false)]
        [DataRow(100, 300, 400, 500, null, false, false)]
        [DataRow(100, 300, -600, 50, null, false, false)]
        [DataRow(100, 300, 100, 300, null, true, true)]
        [DataRow(100, 300, 100, 299, null, true, false)]
        [DataRow(300, 400, double.NegativeInfinity, double.PositiveInfinity, null, false, true)]
        [DataRow(100, 300, 300, 400, Topology.Closed, false, false)]
        [DataRow(100, 200, 100, 200, Topology.Closed, true, false)]
        [DataRow(200, 400, 300, 350, Topology.Closed, true, false)]
        [DataRow(100 - double.Epsilon, 500 + double.Epsilon, 100, 500, Topology.Closed, true, false)]
        [DataRow(200, 400, 200, 400, Topology.Open, false, true)]
        [DataRow(199, 401, 200, 400, Topology.Open, true, false)]
        [DataRow(100, 300, 100, 299, Topology.LeftClosedRightOpen, true, false)]
        [DataRow(100, 300, 100, 300, Topology.LeftClosedRightOpen, true, true)]
        [DataRow(100, 300, 100, 300, Topology.LeftOpenRightClosed, false, false)]
        [DataRow(100, 300, 101, 300, Topology.LeftOpenRightClosed, true, false)]
        public void DoubleContainsIntervalWorks(double a1, double a2, double b1, double b2, Topology? type, bool result, bool reverseResult)
        {
            var a = a1.To(a2, type ?? Topology.Default);
            var b = b1.To(b2);

            var actual = a.Contains(b);

            Assert.AreEqual(result, actual);

            // contains should not be communitive unless closed
            bool actualReverseResult = b.Contains(a);
            Assert.AreEqual(reverseResult, actualReverseResult);
        }

        [DataTestMethod]
        [DataRow(100, 300, 150, 400, true,  100, 400)]
        [DataRow(-30, -10, -90, -20, true,  -90, -10)]
        [DataRow(100, 300, 400, 500, false, 0.0, 0.0)]
        [DataRow(0.1, 0.6, 0.61, 1.0, false,  0, 0.0)]
        [DataRow(double.NegativeInfinity, double.PositiveInfinity, 300, 400, true, double.NegativeInfinity, double.PositiveInfinity)]
        [DataRow(300, 400, double.NegativeInfinity, double.PositiveInfinity, true, double.NegativeInfinity, double.PositiveInfinity)]
        [DataRow(200, 400, 400, 600, true,  200, 600)]
        public void DoubleTryGetUnionWithWorks(double a1, double a2, double b1, double b2, bool success, double c1, double c2)
        {
            var a = a1.To(a2);
            var b = b1.To(b2);
            var expected = c1.To(c2);

            var actualSuccess = a.TryGetUnion(b, out var actual);

            Assert.AreEqual(success, actualSuccess);
            Assert.AreEqual(expected, actual);

            // union is commutative - double check here
            var actualSuccessCommutative = a.TryGetUnion(b, out var actualCommutative);

            Assert.AreEqual(success, actualSuccessCommutative);
            Assert.AreEqual(expected, actualCommutative);
        }

        [DataTestMethod]
        [DataRow(0.0, 0.0, true)]
        [DataRow(double.NegativeInfinity, double.NegativeInfinity, true)]
        [DataRow(10.0, 20.0, false)]
        [DataRow(0.0, double.Epsilon, false)]
        public void DoubleIsEmptyWithWorks(double a1, double a2, bool expected)
        {
            var a = a1.To(a2);

            var actual = a.IsEmpty;

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow(0.0, 0.0, true)]
        [DataRow(double.NegativeInfinity, double.NegativeInfinity, false)]
        [DataRow(10.0, 20.0, false)]
        [DataRow(0.0, double.Epsilon, false)]
        public void DoubleIsDefaultWithWorks(double a1, double a2, bool expected)
        {
            var a = a1.To(a2);

            var actual = a.IsDefault;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DefaultTopologyWorks()
        {
            var actual = default(Range<double>);
            Assert.AreEqual(0, actual.Minimum);
            Assert.AreEqual(0, actual.Maximum);
            Assert.AreEqual(Topology.Default, actual.Topology);
            Assert.IsTrue(actual.IsEmpty);
            Assert.IsTrue(actual.IsMinimumInclusive);
            Assert.IsFalse(actual.IsMaximumInclusive);
        }
    }
}
