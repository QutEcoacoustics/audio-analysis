// <copyright file="IPointData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public interface IPointData
    {
        public ISet<ISpectralPoint> Points { get; }

        public void DrawPointsAsFill(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // overlay point data on image with 50% opacity
            // TODO: a much more efficient implementation exists if we derive from Region and convert
            // our set<points> to a region.
            foreach (var point in this.Points)
            {
                var area = options.Converters.GetPixelRectangle(point);
                graphics.Fill(options.Fill, area);
            }
        }

        public void DrawPointsAsPath(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // visits each point once
            // assumes each point describes a line
            // assumes a SortedSet is used (and that iteration order is signficant, unlike with HashSet)
            var path = this.Points.Select(x => options.Converters.GetPoint(x)).ToArray();

            // note not using AA here
            // note could base pen thickness off ISpectralPoint thickness for a more accurate representation
            graphics.DrawLines(
                options.Border,
                path);
        }
    }
}