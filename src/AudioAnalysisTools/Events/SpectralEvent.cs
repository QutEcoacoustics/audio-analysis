// <copyright file="SpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class SpectralEvent : EventCommon, ISpectralEvent, ITemporalEvent
    {
        public virtual double EventEndSeconds { get; set; }

        public virtual double HighFrequencyHertz { get; set; }

        public virtual double LowFrequencyHertz { get; set; }

        public override void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // draw a border around this event
            var border = options.Converters.GetPixelRectangle(this);
            graphics.NoAA().DrawRectangle(options.Border, border);

            // draw event title
            // TODO
        }
    }
}