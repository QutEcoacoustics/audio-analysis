// <copyright file="DeltaImageProcessor{TPixelFg}.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing.Processors;

    public class DeltaImageProcessor<TPixelFg> : IImageProcessor
        where TPixelFg : struct, IPixel<TPixelFg>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaImageProcessor{TPixelFg}"/> class.
        /// </summary>
        /// <param name="image">The image to blend.</param>
        public DeltaImageProcessor(
            Image<TPixelFg> image)
        {
            this.Image = image;
        }

        public Image<TPixelFg> Image { get; }

        public IImageProcessor<TPixelBg> CreatePixelSpecificProcessor<TPixelBg>(Configuration configuration, Image<TPixelBg> source, Rectangle sourceRectangle)
            where TPixelBg : struct, IPixel<TPixelBg>
        {
            return new DeltaImageProcessor<TPixelBg, TPixelFg>(Configuration.Default, this.Image, source, sourceRectangle);
        }
    }
}