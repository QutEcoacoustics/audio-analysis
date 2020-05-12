// <copyright file="LinearScale.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Scales
{
    using System;

    /// <summary>
    /// A class that converts between two linear ranges.
    /// </summary>
    public class LinearScale
    {
        private readonly double d1;
        private readonly double d2;
        private readonly double dd;
        private readonly double r1;
        private readonly double r2;
        private readonly double rd;
        private readonly bool clamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearScale"/> class.
        /// </summary>
        /// <remarks>
        /// Should be able to handle mapping a domain where x1 ≤ x &lt; x2
        /// to a range where y1 > y ≥ y2 (an inverted mapping).
        /// </remarks>
        /// <param name="domain">The range to consider the domain (the input).</param>
        /// <param name="range">The range to consider the range (the output).</param>
        public LinearScale((double Low, double High) domain, (double Low, double High) range)
            : this(domain, range, false)
        {
        }

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
        /// <inheritdoc cref="LinearScale.LinearScale((double Low, double High), (double Low, double High))"/>
        /// <remarks>
        /// If <paramref name="clamp"/> is <value>true</value> then round-tripping of values is not supported.
        /// </remarks>
        /// <param name="clamp">Whether or not to clamp the values to the end points.</param>
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
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

        /// <summary>
        /// Converts a value from the domain into its range equivalent.
        /// </summary>
        /// <param name="x">The domain value.</param>
        /// <returns>The equivalent range value.</returns>
        public double To(double x)
        {
            // TODO: optimised implementation is possible
            var normal = (x - this.d1) / this.dd;
            var r = (normal * this.rd) + this.r1;
            return this.clamp ? r.Clamp(this.r1, this.r2) : r;
        }

        /// <summary>
        /// Converts a domain magnitude into a range magnitude.
        /// </summary>
        /// <param name="xMagnitude">The domain magnitude.</param>
        /// <returns>The equivalent range magnitude.</returns>
        public double ToMagnitude(double xMagnitude)
        {
            var normalDelta = xMagnitude / this.dd;
            return Math.Abs(normalDelta * this.rd);
        }

        /// <summary>
        /// Converts a value from the range into its domain equivalent.
        /// </summary>
        /// <param name="y">The range value.</param>
        /// <returns>The equivalent domain value.</returns>
        public double From(double y)
        {
            // TODO: optimized implementation is possible
            var normal = (y - this.r1) / this.rd;
            var d = (normal * this.dd) + this.d1;
            return this.clamp ? d.Clamp(this.d1, this.d2) : d;
        }

        /// <summary>
        /// Converts a range magnitude into a domain magnitude.
        /// </summary>
        /// <param name="yMagnitude">The range magnitude.</param>
        /// <returns>The equivalent domain magnitude.</returns>
        public double FromMagnitude(double yMagnitude)
        {
            var normalDelta = yMagnitude / this.rd;
            return Math.Abs(normalDelta * this.dd);
        }
    }
}
