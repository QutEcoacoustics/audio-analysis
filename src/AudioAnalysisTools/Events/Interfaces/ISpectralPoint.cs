// <copyright file="ISpectralPoint.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;

    public interface ISpectralPoint : IEquatable<ISpectralPoint>, IComparable, IInterval2<double, double>
    {
        Interval<double> Seconds { get; }

        Interval<double> Hertz { get; }

        double Value { get; }

        Interval<double> IInterval2<double, double>.X => this.Seconds;

        Interval<double> IInterval2<double, double>.Y => this.Hertz;
    }
}