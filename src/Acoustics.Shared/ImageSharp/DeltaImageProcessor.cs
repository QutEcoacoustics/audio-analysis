// <copyright file="DeltaImageProcessor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using System.Runtime.CompilerServices;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.Memory;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing.Processors;

    public class DeltaImageProcessor<TPixelBg, TPixelFg> : ImageProcessor<TPixelBg>
        where TPixelBg : struct, IPixel<TPixelBg>
        where TPixelFg : struct, IPixel<TPixelFg>
    {
        public DeltaImageProcessor(
            Configuration configuration,
            Image<TPixelFg> targetImage,
            Image<TPixelBg> source,
            Rectangle sourceRectangle)
            : base(configuration, source, sourceRectangle)
        {
            this.TargetImage = targetImage;
            this.Blender = new DeltaPixelBlender<TPixelBg>();
        }

        public DeltaPixelBlender<TPixelBg> Blender { get; }

        public Image<TPixelFg> TargetImage { get; }

        protected override void OnFrameApply(ImageFrame<TPixelBg> source)
        {
            var targetBounds = this.TargetImage.Bounds();
            var sourceBounds = this.Source.Bounds();
            var maxWidth = Math.Min(targetBounds.Width, sourceBounds.Width);

            var operation = new RowIntervalOperation(
                source,
                this.TargetImage,
                this.Blender,
                this.Configuration,
                maxWidth);

            ParallelRowIterator.IterateRows(this.Configuration, this.SourceRectangle, operation);
        }

        /// <summary>
        /// A <see langword="struct"/> implementing the draw logic for <see cref="DeltaImageProcessor{TPixelBg,TPixelFg}"/>.
        /// </summary>
        private readonly struct RowIntervalOperation : IRowIntervalOperation
        {
            private readonly ImageFrame<TPixelBg> sourceFrame;
            private readonly Image<TPixelFg> targetImage;
            private readonly PixelBlender<TPixelBg> blender;
            private readonly Configuration configuration;
            private readonly int width;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RowIntervalOperation(
                ImageFrame<TPixelBg> sourceFrame,
                Image<TPixelFg> targetImage,
                PixelBlender<TPixelBg> blender,
                Configuration configuration,
                int width)
            {
                this.sourceFrame = sourceFrame;
                this.targetImage = targetImage;
                this.blender = blender;
                this.configuration = configuration;
                this.width = width;
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke(in RowInterval rows)
            {
                for (int y = rows.Min; y < rows.Max; y++)
                {
                    Span<TPixelBg> background = this.sourceFrame.GetPixelRowSpan(y).Slice(0, this.width);
                    Span<TPixelFg> foreground = this.targetImage.GetPixelRowSpan(y).Slice(0, this.width);
                    this.blender.Blend(
                        this.configuration,
                        background,
                        background,
                        (ReadOnlySpan<TPixelFg>)foreground,
                        1f);
                }
            }
        }
    }
}