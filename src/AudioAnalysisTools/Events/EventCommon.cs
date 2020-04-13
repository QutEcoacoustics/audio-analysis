// <copyright file="EventCommon.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public abstract class EventCommon : EventBase, IDrawableEvent
    {
        /// <summary>
        /// Draw this event on an image.
        /// </summary>
        /// <param name="graphics">The image prcessing context to draw an event on.</param>
        /// <param name="options">The options associated with this render request.</param>
        public abstract void Draw(IImageProcessingContext graphics, EventRenderingOptions options);
    }
}
