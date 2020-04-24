// <copyright file="SpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class SpectralEvent : EventCommon, ISpectralEvent, ITemporalEvent
    {
        public SpectralEvent()
        {
            // empty constructor to prevent obligatory requiredment for arguments.
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
            var border = options.Converters.GetPixelRectangle(this);
            graphics.NoAA().DrawBorderInset(options.Border, border);
        }

        public void Draw(IImageProcessingContext graphics, EventRenderingOptions options, double scoreMax)
        {
            this.Draw(graphics, options);

            //draw the score bar to indicate relative score
            var topBin = options.Converters.HertzToPixels(this.HighFrequencyHertz);
            var bottomBin = (int)Math.Round(options.Converters.HertzToPixels(this.LowFrequencyHertz));
            var eventPixelHeight = bottomBin - topBin + 1;
            var eventPixelStart = (int)Math.Round(options.Converters.SecondsToPixels(this.EventStartSeconds));
            int scoreHt = (int)Math.Round(eventPixelHeight * this.Score / scoreMax);

            var scorePen = new Pen(Color.LimeGreen, 1);

            graphics.NoAA().DrawLine(scorePen, eventPixelStart, bottomBin - scoreHt, eventPixelStart, bottomBin);

            graphics.DrawTextSafe(this.Name, Acoustics.Shared.ImageSharp.Drawing.Tahoma6, Color.Gray, new PointF(eventPixelStart, topBin - 4));
        }
    }
}