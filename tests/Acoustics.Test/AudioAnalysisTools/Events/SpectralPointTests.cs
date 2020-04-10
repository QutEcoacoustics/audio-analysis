// <copyright file="SpectralPointTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using Acoustics.Shared;
    using global::AudioAnalysisTools.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
