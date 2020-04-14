// <copyright file="SpectralPoint.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Acoustics.Shared;

    public readonly struct SpectralPoint : ISpectralPoint
    {
        public SpectralPoint(Interval<double> seconds, Interval<double> hertz, double value)
        {
            this.Seconds = seconds;
            this.Hertz = hertz;
            this.Value = value;
        }

        public Interval<double> Seconds { get; }

        public Interval<double> Hertz { get; }

        public double Value { get; }

        public bool Equals([AllowNull] ISpectralPoint other)
        {
            return this.Seconds.Equals(other?.Seconds)
                && this.Hertz.Equals(other?.Hertz)
                && this.Value.Equals(other?.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Seconds, this.Hertz, this.Value);
        }

        /// <summary>
        /// Compare all seconds and frequency values.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj.Equals(null))
            {
                return 1;
            }

            var otherPoint = (SpectralPoint)obj;

            int value1 = this.Seconds.CompareTo(otherPoint.Seconds);
            int value2 = this.Hertz.CompareTo(otherPoint.Hertz);
            if (value1 != 0 && value2 != 0)
            {
                return 1;
            }

            return 0;
        }
    }
}
