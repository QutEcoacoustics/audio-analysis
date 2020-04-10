// <copyright file="ITracks.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using AudioAnalysisTools.Events.Interfaces;

    public interface ITracks
    {
        public List<ITrack> Tracks { get; }
    }
}