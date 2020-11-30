// <copyright file="EventDrawer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Drawing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using static Acoustics.Shared.ImageSharp.Drawing;

    public static class EventDrawer
    {
        /// <summary>
        /// Draws a "score" indicator on the left edge of an event.
        /// </summary>
        /// <param name="event">The event for which to draw the score indicator.</param>
        /// <param name="graphics">The image context to draw to.</param>
        /// <param name="options">The event rendering options to use.</param>
        public static void DrawScoreIndicator(this SpectralEvent @event, IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawScore)
            {
                return;
            }

            var normalizedScore = @event.ScoreNormalized.Clamp(0, 1);

            if (normalizedScore == 0 || double.IsNaN(normalizedScore))
            {
                return;
            }

            var rect = options.Converters.GetPixelRectangle(@event);

            // we'drawing events with an inset rectangle border, which means bottom and right borders are one pixel closer than normal
            // we need to account for this difference here
            var insetBorderOffset = options.Border.StrokeWidth; // usually 1px

            // truncate score bar to neatest whole pixel after scaling by height
            var scaledHeight = (int)((float)normalizedScore * rect.Height);

            var top = new PointF(rect.Left, rect.Bottom - scaledHeight);
            var bottom = new PointF(rect.Left, rect.Bottom - insetBorderOffset);

            // the order of the supplied points is important!
            // DO NOT CHANGE
            graphics.NoAA().DrawLines(options.Score, top, bottom);
        }

        public static void DrawEventLabel(this SpectralEvent @event, IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawLabel)
            {
                return;
            }

            var text = @event.Name;

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var bounds = TextMeasurer.MeasureBounds(text, new RendererOptions(Roboto6));
            var topLeft = options.Converters.GetPoint(@event);

            topLeft.Offset(0, -bounds.Height);

            graphics.DrawTextSafe(
                @event.Name,
                Roboto6,
                options.Label,
                topLeft);
        }
    }
}
