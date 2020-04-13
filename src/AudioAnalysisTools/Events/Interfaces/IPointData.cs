// <copyright file="IPointData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using Towel.DataStructures;

    public interface IPointData
    {
        public System.Collections.Generic.ISet<ISpectralPoint> Points { get; }

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

            //var tree = new OmnitreeBoundsLinked<ISpectralPoint, double, double>(
            //    (ISpectralPoint value, out double minX, out double maxX, out double minY, out double maxY) =>
            //    {
            //        minX = value.Seconds.Minimum;
            //        maxX = value.Seconds.Maximum;
            //        minY = value.Hertz.Minimum;
            //        maxY = value.Hertz.Maximum;
            //    }
            //    );

            //tree.Add(new SpectralPoint((5.1, 5.2), (510, 520), 0.9));
            //tree.
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