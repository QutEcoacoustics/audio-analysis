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
        /// <param name="options">The event rendering optons to use.</param>
        public static void DrawScoreIndicator(this SpectralEvent @event, IImageProcessingContext graphics, EventRenderingOptions options)
        {
            if (!options.DrawScore)
            {
                return;
            }

            var normalisedScore = @event.ScoreNormalized.Clamp(0, 1);

            if (normalisedScore == 0)
            {
                return;
            }

            var rect = options.Converters.GetPixelRectangle(@event);

            var scaledHeight = (float)normalisedScore * rect.Height;

            graphics.NoAA().DrawLines(
                options.Score,
                new PointF(rect.Left, rect.Bottom - 1), // minus one is to bring bottom of score line within event frame.
                new PointF(rect.Left, rect.Bottom - scaledHeight));
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
