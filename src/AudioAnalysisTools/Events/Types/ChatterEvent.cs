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
    using AudioAnalysisTools.Events.Tracks;
    using SixLabors.ImageSharp.Processing;

    public class ChatterEvent : SpectralEvent, ITracks
    {
        public ChatterEvent(List<Track> ce)
            : base(ce[0].SegmentStartOffset, ce[0].StartTimeSeconds, ce[0].DurationSeconds, ce[0].LowFreqHertz, ce[0].HighFreqHertz)
        {
            //this.Tracks = ce;
        }

        public List<ITrack> Tracks { get; }
    }
}
