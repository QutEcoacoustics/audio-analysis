// <copyright file="TemporalEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using AnalysisBase.ResultBases;

    public class TemporalEvent : EventBase, ITemporalEvent
    {
        public double EventEndSeconds { get; set; }

    }
}