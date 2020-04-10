// <copyright file="LinearScaleTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Scales
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using global::AudioAnalysisTools.Scales;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LinearScaleTests
    {
        private readonly LinearScale scale = new LinearScale((500, 1000), (0.5, 1.5));
        private readonly LinearScale clampedScale = new LinearScale((0, 1.0), (0, 255), clamp: true);
        private readonly LinearScale invertedRangeScale = new LinearScale((0, 11025), (512, 0));

        [DataTestMethod]
        [DataRow(500, 0.5)]
        [DataRow(1000, 1.5)]
        [DataRow(750, 1.0)]
        [DataRow(250, 0.0)]
        [DataRow(1250, 2.0)]
        public void LinearScaleConvertsToAndFrom(double domain, double range)
        {
            var r = this.scale.To(domain);

            Assert.AreEqual(range, r);

            var d = this.scale.From(r);

            Assert.AreEqual(domain, d);
        }

        [DataTestMethod]
        [DataRow(500, 1.0)]
        [DataRow(250, 0.5)]
        [DataRow(1000, 2.0)]
        public void LinearScaleConvertsToAndFromWidths(double domain, double range)
        {
            var r = this.scale.ToDelta(domain);

            Assert.AreEqual(range, r);

            var d = this.scale.FromDelta(r);

            Assert.AreEqual(domain, d);
        }

        [DataTestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(1, 255, 1)]
        [DataRow(0.5, 127.5, 0.5)]
        [DataRow(-1.0, 0.0, 0)]
        [DataRow(2.0, 255, 1.0)]
        public void LinearScaleConvertsToAndFromClamped(double domain, double range, double clampedDomain)
        {
            var r = this.clampedScale.To(domain);

            Assert.AreEqual(range, r);

            var d = this.clampedScale.From(r);

            Assert.AreEqual(clampedDomain, d);
        }

        [DataTestMethod]
        [DataRow(0, 512)]
        [DataRow(11025, 0)]
        [DataRow(5512.5, 256)]
        [DataRow(22050, -512)]
        [DataRow(-11025, 1024)]
        public void LinearScaleConvertsToAndFromInverted(double domain, double range)
        {
            var r = this.invertedRangeScale.To(domain);

            Assert.AreEqual(range, r);

            var d = this.invertedRangeScale.From(r);

            Assert.AreEqual(domain, d);
        }
    }
}
