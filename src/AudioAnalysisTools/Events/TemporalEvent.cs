// <copyright file="TemporalEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class TemporalEvent : EventCommon, ITemporalEvent
    {
        public double EventEndSeconds { get; set; }

        public double EventDurationSeconds => this.EventEndSeconds - this.EventStartSeconds;

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // simply draw a full-height lines either side of the vent
            var startPixel = options.Converters.SecondsToPixels(this.EventStartSeconds);
            graphics.NoAA().DrawLine(
                options.Border,
                new PointF(startPixel, 0),
                new PointF(startPixel, graphics.GetCurrentSize().Height));

            var endPixel = options.Converters.SecondsToPixels(this.EventEndSeconds);
            graphics.NoAA().DrawLine(
                options.Border,
                new PointF(endPixel, 0),
                new PointF(endPixel, graphics.GetCurrentSize().Height));
        }
    }
}