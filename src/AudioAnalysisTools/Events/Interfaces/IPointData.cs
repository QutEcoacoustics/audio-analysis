// <copyright file="IPointData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared.ImageSharp;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public interface IPointData
    {
        /// <summary>
        /// Gets a collection of spectral points.
        /// </summary>
        public ICollection<ISpectralPoint> Points { get; }

        public void DrawPointsAsFill(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawFill)
            {
                return;
            }

            // overlay point data on image with 50% opacity
            // TODO: a much more efficient implementation exists if we derive from Region and convert
            // our set<points> to a region.

            // TODO: overlapping point double-blend - it'll do for now

            // convert to rects
            var rects = this
                .Points
                .Select(p => new RectangularPolygon(options.Converters.GetPixelRectangle(p)))
                .Cast<IPath>()
                .ToArray();

            graphics.FillWithBlend(options.Fill, rects);

            //graphics.Fill(
            //    new GraphicsOptions()
            //    {
            //        BlendPercentage = 0.5f,
            //        ColorBlendingMode = SixLabors.ImageSharp.PixelFormats.PixelColorBlendingMode.Multiply,
            //    },
            //    Color.FromRgb(0, 255, 0),
            //    new RectangleF(0, 1, 1, 1));

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

        public void DrawPointsAsFillExperiment(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawFill)
            {
                return;
            }

            var rects = this
                .Points
                .Select(p => new RectangularPolygon(options.Converters.GetPixelRectangle(p)))
                .Cast<IPath>()
                .ToArray();

            foreach (var rect in rects)
            {
                graphics.Fill(
                    //new GraphicsOptions()
                    //{
                    //    BlendPercentage = 0.8f,

                    //    //ColorBlendingMode = PixelColorBlendingMode.Multiply,
                    //    ColorBlendingMode = PixelColorBlendingMode.Overlay,
                    //},
                    options.FillOptions,
                    options.Fill,
                    rect);
            }
        }

        public void DrawPointsAsPath(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawFill)
            {
                return;
            }
            // visits each point once
            // assumes each point pair describes a line
            // assumes a SortedSet is used (and that iteration order is signficant, unlike with HashSet)
            // TODO: maybe add an orderby?
            var path = this
                .Points
                .OrderBy(x => x)
                .Select(options.Converters.GetPoint)
                .ToArray();

            // note: using AA here
            // note: could base pen thickness off ISpectralPoint thickness for a more accurate representation
            //graphics.Draw()
            graphics.NoAA().DrawLines(
                options.Border,
                path);
        }
    }
}