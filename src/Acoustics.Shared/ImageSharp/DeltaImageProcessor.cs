// <copyright file="DeltaImageProcessor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.Advanced.ParallelUtils;
    using SixLabors.ImageSharp.Memory;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing.Processors;

    public class DeltaImageProcessor<TPixelBg, TPixelFg> : ImageProcessor<TPixelBg>
        where TPixelBg : struct, IPixel<TPixelBg>
        where TPixelFg : struct, IPixel<TPixelFg>
    {
        public DeltaImageProcessor(
            Configuration configuration,
            Image<TPixelFg> image,
            Image<TPixelBg> source,
            Rectangle sourceRectangle)
            : base(configuration, source, sourceRectangle)
        {
            this.Image = image;
            this.Blender = new DeltaPixelBlender<TPixelBg>();
        }

        public DeltaPixelBlender<TPixelBg> Blender { get; }

        public Image<TPixelFg> Image { get; }

        protected override void OnFrameApply(ImageFrame<TPixelBg> source)
        {
            var targetBounds = this.Image.Bounds();
            var sourceBounds = this.Source.Bounds();
            var maxWidth = Math.Min(targetBounds.Width, sourceBounds.Width);

            void Apply(RowInterval rows)
            {
                for (int min = rows.Min; min < rows.Max; ++min)
                {
                    Span<TPixelBg> destination = source.GetPixelRowSpan<TPixelBg>(min).Slice(0, maxWidth);
                    Span<TPixelFg> span = this.Image.GetPixelRowSpan<TPixelFg>(min).Slice(0, maxWidth);
                    this.Blender.Blend<TPixelFg>(
                        this.Configuration,
                        destination,
                        (ReadOnlySpan<TPixelBg>)destination,
                        (ReadOnlySpan<TPixelFg>)span,
                        1f);
                }
            }

            ParallelHelper.IterateRows(this.SourceRectangle, this.Configuration, Apply);
        }
    }
}