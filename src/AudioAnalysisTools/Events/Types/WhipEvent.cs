// <copyright file="WhipEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using SixLabors.ImageSharp.Processing;

    public class WhipEvent : SpectralEvent, ITrack
    {
        public WhipEvent(Track wt)
            : base(wt.SegmentStartOffset, wt.StartTimeSeconds, wt.DurationSeconds, wt.LowFreqHertz, wt.HighFreqHertz)
        {
            this.Track = wt;
        }

        public TimeSpan SegmentStartOffset => this.Track.SegmentStartOffset;

        public ISet<ISpectralPoint> Points => this.Track.Points;

        public Track Track { get; private set; }

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // foreach (var track in tracks) {
            // track.Draw(...)
            // }

            this.Track.Draw(graphics, options);

            //  base drawing (border)
            // TODO: unless border is disabled
            base.Draw(graphics, options);
        }
    }
}