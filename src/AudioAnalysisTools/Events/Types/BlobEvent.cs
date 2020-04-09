// <copyright file="BlobEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class BlobEvent : SpectralEvent, IPointData
    {
        public BlobEvent()
        {
        }

        public ISet<ISpectralPoint> Points { get; } = new HashSet<ISpectralPoint>();

        public override void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // overlay point data on image with 50% opacity
            // TODO: a much more efficient implementation exists if we derive from Region and convert
            // our set<points> to a region.
            foreach (var point in this.Points)
            {
                var area = options.Converters.GetPixelRectangle(point);
                graphics.Fill(options.Fill, area);
            }

            //  base drawing (border)
            base.Draw<T>(graphics, options);
        }
    }
}