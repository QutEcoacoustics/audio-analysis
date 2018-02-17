// <copyright file="UnsafeImage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Acoustics.Unsafe
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class UnsafeImage
    {
        /// <summary>
        /// Get spectrogram image.
        /// </summary>
        /// <param name="audioData">Audio data.</param>
        /// <param name="height">Spectrogram height.</param>
        /// <param name="width">Spectrogram width.</param>
        /// <returns>Spectrogram image.</returns>
        public static Bitmap GetImage(double[,] audioData, int height, int width)
        {
            var managedImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            AForge.Imaging.UnmanagedImage image = AForge.Imaging.UnmanagedImage.FromManagedImage(managedImage);

            int pixelSize = Image.GetPixelFormatSize(image.PixelFormat) / 8;

            // image dimension
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            int stride = image.Stride;

            const int StartX = 0;
            int stopX = imageWidth - 1;

            // spectrogram is drawn from the bottom
            const int StartY = 0;
            int stopY = imageHeight - 1;

            // min, max, range
            double min;
            double max;
            audioData.MinMax(out min, out max);
            double range = max - min;

            int offset = stride - ((stopX - StartX + 1) * pixelSize);

            int heightOffset = imageHeight;

            unsafe
            {
                // do the job
                byte* ptr = (byte*)image.ImageData.ToPointer() + (StartY * stride) + (StartX * pixelSize);

                // height
                for (int y = StartY; y <= stopY; y++)
                {
                    // width
                    for (int x = StartX; x <= stopX; x++, ptr += pixelSize)
                    {
                        // required to render spectrogram correct way up
                        int spectrogramY = heightOffset - 1;

                        // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                        // this is the amplitude
                        double value = (audioData[x, spectrogramY] - min) / range;
                        double colour = 255.0 - Math.Floor(255.0 * value);

                        colour = Math.Min(colour, 255);
                        colour = Math.Max(colour, 0);

                        byte paintColour = Convert.ToByte(colour);

                        // set colour
                        ptr[AForge.Imaging.RGB.R] = paintColour;
                        ptr[AForge.Imaging.RGB.G] = paintColour;
                        ptr[AForge.Imaging.RGB.B] = paintColour;
                    }

                    ptr += offset;

                    heightOffset--;
                }
            }

            return image.ToManagedImage();
        }
    }
}
