// <copyright file="CannyEdgeDetector.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// https://raw.githubusercontent.com/mdavid/aforge.net/de003385a06afcbaace9e03961f8d2f2f4f4d178/Sources/Imaging/Filters/Edge%20Detectors/CannyEdgeDetector.cs
// Adapted by Anthony Truskinger to work with an ImageSharp image construct. Not ideal in any way!
//
// AForge Image Processing Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2005-2008
// andrew.kirillov@aforgenet.com
//
// Article by Bill Green was used as the reference
// http://www.pages.drexel.edu/~weg22/can_tut.html
//

// ReSharper disable once CheckNamespace
namespace AForge.Imaging.Filters
{
    using System;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Processing.Processors.Convolution;

    /// <summary>
    /// Base class for filters, which require source image backup to make them applicable to
    /// source image (or its part) directly.
    /// </summary>
    ///
    /// <remarks><para>The base class is used for filters, which can not do
    /// direct manipulations with source image. To make effect of in-place filtering,
    /// these filters create a background copy of the original image (done by this
    /// base class) and then do manipulations with it putting result back to the original
    /// source image.</para>
    ///
    /// <para><note>The background copy of the source image is created only in the case of in-place
    /// filtering. Otherwise background copy is not created - source image is processed and result is
    /// put to destination image.</note></para>
    ///
    /// <para>The base class is for those filters, which support as filtering entire image, as
    /// partial filtering of specified rectangle only.</para>
    /// </remarks>
    ///
    public abstract class BaseUsingCopyPartialFilter
    {
        /// <summary>
        /// Apply filter to an image.
        /// </summary>
        ///
        /// <param name="imageData">Source image to apply filter to.</param>
        ///
        /// <returns>Returns filter's result obtained by applying the filter to
        /// the source image.</returns>
        ///
        /// <remarks>
        /// The filter accepts bitmap data as input and returns the result
        /// of image processing filter as new image. The source image data are kept
        /// unchanged.
        /// </remarks>
        public Image<L8> Apply(Image<Rgb24> imageData)
        {
            // get width and height
            int width = imageData.Width;
            int height = imageData.Height;

            // create new image of required format
            var dstImage = new Image<L8>(width, height);

            // lock destination bitmap data

            // process the filter
            this.ProcessFilter(imageData.Clone(x => x.Grayscale(GrayscaleMode.Bt709)), dstImage, new Rectangle(0, 0, width, height));

            return dstImage;
        }

        /// <summary>
        /// Process the filter on the specified image.
        /// </summary>
        ///
        /// <param name="sourceData">Source image data.</param>
        /// <param name="destinationData">Destination image data.</param>
        /// <param name="rect">Image rectangle for processing by the filter.</param>
        ///
        protected abstract void ProcessFilter(Image<Rgb24> sourceData, Image<L8> destinationData, Rectangle rect);
    }

    /// <summary>
    /// Canny edge detector.
    /// </summary>
    ///
    /// <remarks><para>The filter searches for objects' edges by applying Canny edge detector.
    /// The implementation follows
    /// <a href="http://www.pages.drexel.edu/~weg22/can_tut.html">Bill Green's Canny edge detection tutorial</a>.</para>
    ///
    /// <para><note>The implemented canny edge detector has one difference with the above linked algorithm.
    /// The difference is in hysteresis step, which is a bit simplified (getting faster as a result). On the
    /// hysteresis step each pixel is compared with two threshold values: <see cref="HighThreshold"/> and
    /// <see cref="LowThreshold"/>. If pixel's value is greater or equal to <see cref="HighThreshold"/>, then
    /// it is kept as edge pixel. If pixel's value is greater or equal to <see cref="LowThreshold"/>, then
    /// it is kept as edge pixel only if there is at least one neighbouring pixel (8 neighbours are checked) which
    /// has value greater or equal to <see cref="HighThreshold"/>; otherwise it is none edge pixel. In the case
    /// if pixel's value is less than <see cref="LowThreshold"/>, then it is marked as none edge immediately.
    /// </note></para>
    ///
    /// <para>The filter accepts 8 bpp grayscale images for processing.</para>
    ///
    /// <para>Sample usage:</para>
    /// <code>
    /// // create filter
    /// CannyEdgeDetector filter = new CannyEdgeDetector( );
    /// // apply the filter
    /// filter.ApplyInPlace( image );
    /// </code>
    ///
    /// <para>
    /// See the original AForge code for example images.
    /// </para>
    /// </remarks>
    ///
    public class CannyEdgeDetector : BaseUsingCopyPartialFilter
    {
        private readonly GaussianBlurProcessor gaussianFilter;

        /// <summary>
        /// Gets or sets low threshold.
        /// </summary>
        ///
        /// <remarks><para>Low threshold value used for hysteresis
        /// (see  <a href="http://www.pages.drexel.edu/~weg22/can_tut.html">tutorial</a>
        /// for more information).</para>
        ///
        /// <para>Default value is set to <b>20</b>.</para>
        /// </remarks>
        ///
        public byte LowThreshold { get; set; } = 20;

        /// <summary>
        /// Gets or sets high threshold.
        /// </summary>
        ///
        /// <remarks><para>High threshold value used for hysteresis
        /// (see  <a href="http://www.pages.drexel.edu/~weg22/can_tut.html">tutorial</a>
        /// for more information).</para>
        ///
        /// <para>Default value is set to <b>100</b>.</para>
        /// </remarks>
        ///
        public byte HighThreshold { get; set; } = 100;

        /// <summary>
        /// Gets gaussian sigma.
        /// </summary>
        public float GaussianSigma => this.gaussianFilter.Sigma;

        /// <summary>
        /// Gets gaussian size.
        /// </summary>
        public int GaussianSize => this.gaussianFilter.Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="CannyEdgeDetector"/> class.
        /// </summary>
        ///
        public CannyEdgeDetector(float gaussianSigma = GaussianBlurProcessor.DefaultSigma, int? gaussianRadius = null)
        {
            if (gaussianRadius.NotNull())
            {
                this.gaussianFilter = new GaussianBlurProcessor(gaussianSigma, gaussianRadius.Value);
            }
            else
            {
                this.gaussianFilter = new GaussianBlurProcessor(gaussianSigma);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannyEdgeDetector"/> class.
        /// </summary>
        ///
        /// <param name="lowThreshold">Low threshold.</param>
        /// <param name="highThreshold">High threshold.</param>
        ///
        public CannyEdgeDetector(byte lowThreshold, byte highThreshold)
            : this()
        {
            this.LowThreshold = lowThreshold;
            this.HighThreshold = highThreshold;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CannyEdgeDetector"/> class.
        /// </summary>
        ///
        /// <param name="lowThreshold">Low threshold.</param>
        /// <param name="highThreshold">High threshold.</param>
        /// <param name="sigma">Gaussian sigma.</param>
        ///
        public CannyEdgeDetector(byte lowThreshold, byte highThreshold, float sigma, int radius)
            : this(sigma, radius)
        {
            this.LowThreshold = lowThreshold;
            this.HighThreshold = highThreshold;
        }

        /// <summary>
        /// Process the filter on the specified image.
        /// </summary>
        ///
        /// <param name="source">Source image data.</param>
        /// <param name="destination">Destination image data.</param>
        /// <param name="rect">Image rectangle for processing by the filter.</param>
        ///
        protected override void ProcessFilter(Image<Rgb24> source, Image<L8> destination, Rectangle rect)
        {
            // processing start and stop X,Y positions
            int startX = rect.Left + 1;
            int startY = rect.Top + 1;
            int stopX = startX + rect.Width - 2;
            int stopY = startY + rect.Height - 2;

            int width = rect.Width - 2;
            int height = rect.Height - 2;

            int dstOffset = rect.Width + 2;
            int srcOffset = rect.Width + 2;

            // pixel's value and gradients
            int gx, gy;

            double orientation, toAngle = 180.0 / System.Math.PI;
            float leftPixel = 0, rightPixel = 0;

            // STEP 1 - blur image
            source.Mutate(x => x.ApplyProcessor(this.gaussianFilter));
            var src = source.CloneAs<L8>();

            // orientation array
            byte[] orients = new byte[width * height];

            // gradients array
            float[,] gradients = new float[source.Width, source.Height];
            float maxGradient = float.NegativeInfinity;

            // STEP 2 - calculate magnitude and edge orientation
            int p = 0;

            // for each line
            for (int y = startY; y < stopY; y++)
            {
                // for each pixel
                for (int x = startX; x < stopX; x++, p++)
                {
                    gx = src[y - 1, x + 1].PackedValue + src[y, x + 1].PackedValue
                       - src[y - 1, x - 1].PackedValue - src[y, x - 1].PackedValue
                       + (2 * (src[y + 1, x].PackedValue - src[y - 1, x].PackedValue));

                    gy = src[y - 1, x - 1].PackedValue + src[y - 1, x + 1].PackedValue
                       - src[y, x - 1].PackedValue - src[y, x + 1].PackedValue
                       + (2 * (src[y - 1, x].PackedValue - src[y + 1, x].PackedValue));

                    // get gradient value
                    gradients[x, y] = (float)Math.Sqrt((gx * gx) + (gy * gy));
                    if (gradients[x, y] > maxGradient)
                    {
                        maxGradient = gradients[x, y];
                    }

                    // --- get orientation
                    if (gx == 0)
                    {
                        // can not divide by zero
                        orientation = (gy == 0) ? 0 : 90;
                    }
                    else
                    {
                        double div = (double)gy / gx;

                        // handle angles of the 2nd and 4th quads
                        if (div < 0)
                        {
                            orientation = 180 - (Math.Atan(-div) * toAngle);
                        }

                        // handle angles of the 1st and 3rd quads
                        else
                        {
                            orientation = System.Math.Atan(div) * toAngle;
                        }

                        // get closest angle from 0, 45, 90, 135 set
                        if (orientation < 22.5)
                        {
                            orientation = 0;
                        }
                        else if (orientation < 67.5)
                        {
                            orientation = 45;
                        }
                        else if (orientation < 112.5)
                        {
                            orientation = 90;
                        }
                        else if (orientation < 157.5)
                        {
                            orientation = 135;
                        }
                        else
                        {
                            orientation = 0;
                        }
                    }

                    // save orientation
                    orients[p] = (byte)orientation;
                }
            }

            // STEP 3 - suppres non maximums

            p = 0;

            // for each line
            for (int y = startY; y < stopY; y++)
            {
                // for each pixel
                for (int x = startX; x < stopX; x++, p++)
                {
                    // get two adjacent pixels
                    switch (orients[p])
                    {
                        case 0:
                            leftPixel = gradients[x - 1, y];
                            rightPixel = gradients[x + 1, y];
                            break;
                        case 45:
                            leftPixel = gradients[x - 1, y + 1];
                            rightPixel = gradients[x + 1, y - 1];
                            break;
                        case 90:
                            leftPixel = gradients[x, y + 1];
                            rightPixel = gradients[x, y - 1];
                            break;
                        case 135:
                            leftPixel = gradients[x + 1, y + 1];
                            rightPixel = gradients[x - 1, y - 1];
                            break;
                    }

                    // compare current pixels value with adjacent pixels
                    if ((gradients[x, y] < leftPixel) || (gradients[x, y] < rightPixel))
                    {
                        destination[startY, startX] = new L8(0);
                    }
                    else
                    {
                        var result = (byte)(gradients[x, y] / maxGradient * 255);
                        destination[startY, startX] = new L8(result);
                    }
                }
            }

            // STEP 4 - hysteresis

            // for each line
            for (int y = startY; y < stopY; y++)
            {
                // for each pixel
                for (int x = startX; x < stopX; x++)
                {
                    if (destination[startY, startX].PackedValue < this.HighThreshold)
                    {
                        if (destination[startY, startX].PackedValue < this.LowThreshold)
                        {
                            // non edge
                            destination[startY, startX] = new L8(0);
                        }
                        else
                        {
                            // check 8 neighboring pixels
                            if ((destination[startY - 1, startX].PackedValue < this.HighThreshold) &&
                                (destination[startY + 1, startX].PackedValue < this.HighThreshold) &&
                                (destination[startY - 1, startX - 1].PackedValue < this.HighThreshold) &&
                                (destination[startY - 1, startX].PackedValue < this.HighThreshold) &&
                                (destination[startY - 1, startX + 1].PackedValue < this.HighThreshold) &&
                                (destination[startY + 1, startX - 1].PackedValue < this.HighThreshold) &&
                                (destination[startY + 1, startX].PackedValue < this.HighThreshold) &&
                                (destination[startY + 1, startX + 1].PackedValue < this.HighThreshold))
                            {
                                destination[startY, startX] = new L8(0);
                            }
                        }
                    }
                }
            }

            // STEP 5 - draw black rectangle to remove those pixels, which were not processed
            // (this needs to be done for those cases, when filter is applied "in place" -
            //  source image is modified instead of creating new copy)
            RectangleF r = rect;
            destination.Mutate(x => x.Draw(Pens.Solid(Color.Black, 1), r));
        }
    }
}