// <copyright file="SpectralPointTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using Acoustics.Shared;
    using global::AudioAnalysisTools.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class SpectralPointTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            var point = new SpectralPoint(
                0.23.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            Assert.AreEqual((0, 0.23), point.Seconds);
            Assert.AreEqual((0, 43.0), point.Hertz);
            Assert.AreEqual(0.5, point.Value);
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = new SpectralPoint(
                0.23.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            var b = new SpectralPoint(
                0.23.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void TestHashCode()
        {
            var a = new SpectralPoint(
                0.23.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            var b = new SpectralPoint(
                0.23.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            var c = new SpectralPoint(
                0.231.AsIntervalFromZero(),
                43.0.AsIntervalFromZero(),
                0.5);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), c.GetHashCode());
        }

        [DataTestMethod]
        [DataRow(0.1, 0.2, 1000, 2000, 3, -1)]
        [DataRow(5.0, 5.2, 1000, 2000, 3,  1)]
        [DataRow(1.0, 5.0, 1000, 2000, 3, -1)]
        [DataRow(1.0, 5.0, 7000, 9000, 3,  1)]
        [DataRow(1.0, 5.0, 5000, 6000, 1, -1)]
        [DataRow(1.0, 5.0, 5000, 6000, 4,  1)]
        [DataRow(1.0, 5.0, 5000, 6000, 3,  0)]
        public void TestComparer(double t1, double t2, double h1, double h2, double v, int expected)
        {
            var other = new SpectralPoint(
                (1, 5),
                (5000, 6000),
                3);

            var test = new SpectralPoint(
                (t1, t2),
                (h1, h2),
                v);

            var actual = test.CompareTo(other);
            var actualInverse = other.CompareTo(test);

            Assert.AreEqual(expected, actual);

            var inverseExpected = expected switch
            {
                -1 => 1,
                0 => 0,
                1 => -1,
                _ => throw new NotSupportedException(),
            };
            Assert.AreEqual(inverseExpected, actualInverse);
        }

        [TestMethod]
        public void TestToString()
        {
            Interval<double> seconds = (1, 5);
            Interval<double> hertz = (5000, 6000);

            var test = new SpectralPoint(
               seconds,
               hertz,
               3);

            var actual = test.ToString();

            Assert.AreEqual(
                $"SpectralPoint: [1, 5) s, [5000, 6000) Hz, 3 value",
                actual);
        }
    }
}
