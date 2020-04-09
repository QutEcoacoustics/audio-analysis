// <copyright file="ISpectralPoint.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using Acoustics.Shared;

    public interface ISpectralPoint : IEquatable<ISpectralPoint>
    {
        Acoustics.Shared.Range<double> Seconds { get; }

        Acoustics.Shared.Range<double> Hertz { get; }

        double Value { get; }
    }
}