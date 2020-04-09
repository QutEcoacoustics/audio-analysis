// <copyright file="SpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class SpectralEvent : EventBase, ISpectralEvent, ITemporalEvent, IDrawableEvent
    {
        public double EventEndSeconds { get; set; }

        public double HighFrequencyHertz { get; set; }

        public double LowFrequencyHertz { get; set; }

        public virtual void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
            where T : struct, IPixel<T>
        {
            // draw a border around this event
            var border = options.Converters.GetPixelRectangle(this);
            graphics.NoAA().DrawRectangle(options.Border, border);

            // draw event title
            // TODO
        }

    }
}