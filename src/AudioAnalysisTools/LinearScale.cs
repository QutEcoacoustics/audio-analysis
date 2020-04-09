// <copyright file="LinearScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    public class LinearScale
    {
        private readonly double d1;
        private readonly double d2;
        private readonly double dd;
        private readonly double r1;
        private readonly double r2;
        private readonly double rd;

        public LinearScale((double Low, double High) domain, (double Low, double High) range)
        {
            this.d1 = domain.Low;
            this.d2 = domain.High;
            this.dd = domain.High - domain.Low;
            this.r1 = range.Low;
            this.r2 = range.High;
            this.rd = range.High - range.Low;
        }

        // TODO: optimised implementation is possible
        public double To(double x)
        {
            var normal = (x - this.d1) / this.dd;
            return (normal * this.rd) + this.r1;
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
            var normalDelta = yDelta / (this.r2 - this.r1);
            return normalDelta * this.dd;
        }
    }
}
