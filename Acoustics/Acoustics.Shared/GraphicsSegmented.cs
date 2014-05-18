namespace Acoustics.Shared
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// Draws an image in small rectangles until the entire image is drawn.
    /// Required due to limitations of System.Drawing.Graphics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Graphics.DrawImage() or GDI cannot draw an image that is too big, typically greater than 40000 pixels.
    /// So call this method which calls itself recursively and draws image in small rectangles until the entire image is drawn.
    /// </para>
    /// <para>
    /// GDI balks (throws an exception) when attempting to draw an image that is too big, typically greater than 40000 pixels.
    /// This method draws in segments to avoid causing an error. The given image will be resized to match the graphics.
    /// </para>
    /// </remarks>
    public static class GraphicsSegmented
    {
        private const int GdiDrawingLimit = 20000;

        /// <summary>
        /// Draw image at original size starting at (0,0).
        /// </summary>
        /// <param name="graphics">
        /// The graphics.
        /// </param>
        /// <param name="sourceImage">
        /// The source image.
        /// </param>
        public static void Draw(Graphics graphics, Image sourceImage)
        {
            graphics.InterpolationMode = InterpolationMode.Bilinear;
            Draw(graphics, sourceImage, 0);
        }

        /// <summary>
        /// Draw image at original size, starting at (<paramref name="accumulatedX"/>, 0).
        /// </summary>
        /// <param name="graphics">Graphics object to use for drawing.</param>
        /// <param name="sourceImage">Image to be drawn.</param>
        /// <param name="accumulatedX">the left most location of current drawing rectangle.</param>
        public static void Draw(Graphics graphics, Image sourceImage, int accumulatedX)
        {
            // how much do we have to draw? Is it narrower than the gdiLimit?
            var writeWidth = Math.Min(GdiDrawingLimit, sourceImage.Width - accumulatedX);

            var writeRectangle = new Rectangle(accumulatedX, 0, writeWidth, sourceImage.Height);
            graphics.DrawImage(sourceImage, writeRectangle, accumulatedX, 0, writeWidth, sourceImage.Height, GraphicsUnit.Pixel);

            accumulatedX += writeWidth;

            if (accumulatedX < sourceImage.Width)
            {
                Draw(graphics, sourceImage, accumulatedX);
            }
        }

        /// <summary>
        /// Draw image segment at specified height and width.
        /// </summary>
        /// <param name="graphics">
        /// The graphics.
        /// </param>
        /// <param name="sourceImage">
        /// The source image.
        /// </param>
        /// <param name="destImageHeight">
        /// The destination image height.
        /// </param>
        /// <param name="destImageWidth">
        /// The destination image width.
        /// </param>
        public static void Draw(Graphics graphics, Image sourceImage, int destImageHeight, int destImageWidth)
        {
            graphics.InterpolationMode = InterpolationMode.Bilinear;
            Draw(graphics, sourceImage, destImageHeight, destImageWidth, 0, 0);
        }

        /// <summary>
        /// Draw image segment at specified height and width.
        /// </summary>
        /// <param name="graphics">
        /// The graphics.
        /// </param>
        /// <param name="sourceImage">
        /// The source image.
        /// </param>
        /// <param name="destImageHeight">
        /// The dest image height.
        /// </param>
        /// <param name="destImageWidth">
        /// The dest image width.
        /// </param>
        /// <param name="accumulatedSourceX">
        /// The accumulated source width.
        /// </param>
        /// <param name="accumulatedDestX">
        /// The accumulated dest width.
        /// </param>
        public static void Draw(Graphics graphics, Image sourceImage, int destImageHeight, int destImageWidth, int accumulatedSourceX, int accumulatedDestX)
        {
            // sourceImage needs to be drawn to gaphics.
            // It should be resized to destImageHeight & destImageWidth overall.
            var destWidth = Math.Min(GdiDrawingLimit, destImageWidth - accumulatedDestX);
            var destHeight = destImageHeight;
            var destX = accumulatedDestX;
            const int DestY = 0;

            var srcWidth = Math.Min(GdiDrawingLimit, sourceImage.Width - accumulatedSourceX);
            var srcHeight = sourceImage.Height;
            var srcX = accumulatedSourceX;
            const int SrcY = 0;

            var widthFactor = (float)destImageWidth / (float)sourceImage.Width;
            if (destWidth > GdiDrawingLimit || srcWidth > GdiDrawingLimit)
            {
                if (widthFactor > 1)
                {
                    // dest is larger than src
                    srcWidth = Convert.ToInt32(destWidth / widthFactor);
                }
                else
                {
                    // dest is smaller or equal to src
                    destWidth = Convert.ToInt32(srcWidth * widthFactor);
                }
            }

            var destRect = new Rectangle(destX, DestY, destWidth, destHeight);
            var srcRect = new Rectangle(srcX, SrcY, srcWidth, srcHeight);

            graphics.DrawImage(sourceImage, destRect, srcRect, GraphicsUnit.Pixel);

            accumulatedSourceX += srcWidth;
            accumulatedDestX += destWidth;

            if (accumulatedSourceX < sourceImage.Width && accumulatedDestX < destImageWidth)
            {
                Draw(
                    graphics,
                    sourceImage,
                    destImageHeight,
                    destImageWidth,
                    accumulatedSourceX,
                    accumulatedDestX);
            }
        }
    }
}
