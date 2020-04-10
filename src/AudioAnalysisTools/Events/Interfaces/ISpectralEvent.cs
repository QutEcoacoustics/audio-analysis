// <copyright file="ISpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Interfaces
{
    using AnalysisBase.ResultBases;

    public interface ISpectralEvent : ITemporalEvent, ISpectralBand
    {
    }
}