// <copyright file="SpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class SpectralEvent : EventCommon, ISpectralEvent, ITemporalEvent
    {
        public SpectralEvent()
        {
            // empty constructor to prevent obligatory requirement for arguments.
        }

        public SpectralEvent(TimeSpan segmentStartOffset, double eventStartRecordingRelative, double eventEndRecordingRelative, double minFreq, double maxFreq)
        {
            this.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
            this.EventStartSeconds = eventStartRecordingRelative;
            this.EventEndSeconds = eventEndRecordingRelative;
            this.LowFrequencyHertz = minFreq;
            this.HighFrequencyHertz = maxFreq;
        }

        public virtual double EventEndSeconds { get; set; }

        public virtual double HighFrequencyHertz { get; set; }

        public virtual double LowFrequencyHertz { get; set; }

        //public double Duration => base.Duration;

        /// DIMENSIONS OF THE EVENT
        /// <summary>Gets the event duration in seconds.</summary>
        public double EventDurationSeconds => this.EventEndSeconds - this.EventStartSeconds;

        public double BandWidthHertz => this.HighFrequencyHertz - this.LowFrequencyHertz;

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // draw a border around this event
            if (options.DrawBorder)
            {
                var border = options.Converters.GetPixelRectangle(this);
                graphics.NoAA().DrawBorderInset(options.Border, border);
            }

            this.DrawScoreIndicator(graphics, options);

            this.DrawEventLabel(graphics, options);
        }
    }
}
