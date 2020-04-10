// <copyright file="InstantEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class InstantEvent : EventCommon, IInstantEvent
    {
        public override void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // simply draw a full-height line
            var startPixel = options.Converters.SecondsToPixels(this.EventStartSeconds);
            graphics.NoAA().DrawLine(
                options.Border,
                new PointF(startPixel, 0),
                new PointF(startPixel, graphics.GetCurrentSize().Height));
        }
    }
}
