// <copyright file="InstantEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class InstantEvent : EventBase, IInstantEvent, IDrawableEvent
    {
        public void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
            where T : struct, IPixel<T>
        {
            throw new NotImplementedException();
        }
    }
}
