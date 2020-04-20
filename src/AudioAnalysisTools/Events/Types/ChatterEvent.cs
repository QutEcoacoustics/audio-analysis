// <copyright file="ChatterEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Types
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp.Processing;

    public class ChatterEvent : SpectralEvent, ITracks
    {
        public ChatterEvent(List<ITrack> chitters)
        {
            this.Tracks = chitters;
        }

        public List<ITrack> Tracks { get; }
    }
}
