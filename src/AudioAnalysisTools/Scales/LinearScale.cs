// <copyright file="LinearScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Scales
{
    using System;

    public class LinearScale
    {
        private readonly double d1;
        private readonly double d2;
        private readonly double dd;
        private readonly double r1;
        private readonly double r2;
        private readonly double rd;
        private readonly bool clamp;

        public LinearScale((double Low, double High) domain, (double Low, double High) range)
            : this(domain, range, false)
        {
        }

        public LinearScale((double Low, double High) domain, (double Low, double High) range, bool clamp)
        {
            this.d1 = domain.Low;
            this.d2 = domain.High;
            this.dd = domain.High - domain.Low;
            this.r1 = range.Low;
            this.r2 = range.High;
            this.rd = range.High - range.Low;
            this.clamp = clamp;
        }

        // TODO: optimised implementation is possible
        public double To(double x)
        {
            var normal = (x - this.d1) / this.dd;
            var r = (normal * this.rd) + this.r1;
            return this.clamp ? r.Clamp(this.r1, this.r2) : r;
        }

        public double ToDelta(double xDelta)
        {
            var normalDelta = xDelta / this.dd;
            return normalDelta * this.rd;
        }

        // TODO: optimised implementation is possible
        public double From(double y)
        {
            var normal = (y - this.r1) / this.rd;
            return (normal * this.dd) + this.d1;
        }

        public double FromDelta(double yDelta)
        {
            var normalDelta = yDelta / this.rd;
            var d = normalDelta * this.dd;
            return this.clamp ? d.Clamp(this.d1, this.d2) : d;
        }
    }
}
