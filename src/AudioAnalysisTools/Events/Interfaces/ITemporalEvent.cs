// <copyright file="ITemporalEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.ResultBases
{
    using AudioAnalysisTools.Events.Interfaces;

    public interface ITemporalEvent : IInstantEvent
    {
        double EventEndSeconds { get; }

        double EventDurationSeconds { get; }
    }
}