namespace System.Drawing
{
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    using Acoustics.Shared;

    /// <summary>
    /// Image extension methods.
    /// </summary>
    public static class ExtensionsDrawing
    {
        #region Constants and Fields

        /// <summary>
        /// The color reg ex error.
        /// </summary>
        public const string ColorRegExError = "Must adhere to a standard hex color code (#00000000)";

        /// <summary>
        /// The reg ex hex color.
        /// </summary>
        public const string RegExHexColor = "^#?([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$";

        #endregion


        /// <summary>
        /// Convert an image to a byte array.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// The image Format.
        /// </param>
        /// <returns>
        /// Byte array representing image.
        /// </returns>
        public static byte[] ToByteArray(this Image image, ImageFormat imageFormat)
        {
            if (image == null) return new byte[0];

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                bytes = ms.GetBuffer();
            }

            return bytes;
        }

        /// <summary>
        /// Crop an image using a <paramref name="crop"/> Rectangle.
        /// </summary>
        /// <param name="source">Source image.</param>
        /// <param name="crop">Crop rectangle.</param>
        /// <returns>Cropped image.</returns>
        /// <remarks>
        /// Use Graphics.DrawImage() to copy the selection portion of the source image. 
        /// You'll need the overload that takes a source and a destination Rectangle. 
        /// Create the Graphics instance from Graphics.FromImage() on a new bitmap that 
        /// has the same size as the rectangle.
        /// from: http://stackoverflow.com/questions/2405261/how-to-clip-a-rectangle-from-a-tiff
        /// </remarks>
        public static Bitmap Crop(this Image source, Rectangle crop)
        {
            if (source == null) return null;

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }

            return bmp;
        }

        /// <summary>
        /// The to hex string.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <param name="includeAlpha">
        /// The include alpha.
        /// </param>
        /// <returns>
        /// Color as hex string.
        /// </returns>
        public static string ToHexString(this Color color, bool includeAlpha)
        {
            ////Contract.Ensures(Contract.Result<string>().Length == 9 || Contract.Result<string>().Length == 7);

            string result = "#";

            if (includeAlpha)
            {
                result += color.A.ToString("X2");
            }

            result += color.R.ToString("X2");
            result += color.G.ToString("X2");
            result += color.B.ToString("X2");

            return result;
        }

        /// <summary>
        /// Crop <paramref name="originalImage"/>.
        /// </summary>
        /// <param name="originalImage">
        /// The original image.
        /// </param>
        /// <param name="originalStart">
        /// The original Start.
        /// </param>
        /// <param name="originalEnd">
        /// The original End.
        /// </param>
        /// <param name="originalMimeType">
        /// The original Mime Type.
        /// </param>
        /// <param name="desiredStart">
        /// The desired Start.
        /// </param>
        /// <param name="desiredEnd">
        /// The desired End.
        /// </param>
        /// <param name="desiredMimeType">
        /// The desired Mime Type.
        /// </param>
        /// <returns>
        /// <paramref name="originalImage"/> cropped to match <paramref name="desiredRequest"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        public static Image Crop(this Image originalImage, long? originalStart, long? originalEnd, string originalMimeType, long? desiredStart, long? desiredEnd, string desiredMimeType)
        {
            if (originalImage == null)
            {
                return null;
            }

            if (originalStart == null || originalEnd == null)
            {
                return null;
            }

            /*
             * Image retrieved from cache must be modified to match request.
             */

            var availableStart = Convert.ToInt32(originalStart);
            var availableEnd = Convert.ToInt32(originalEnd);

            var requestStart = Convert.ToInt32(desiredStart.HasValue ? desiredStart.Value : availableStart);
            var requestEnd = Convert.ToInt32(desiredEnd.HasValue ? desiredEnd.Value : availableEnd);

            if (availableStart > requestStart)
            {
                throw new InvalidOperationException("Request start (" + requestStart + ") must be less than or equal to available start (" + availableStart + ").");
            }

            if (availableEnd < requestEnd)
            {
                throw new InvalidOperationException("Request end (" + requestEnd + ") must be greater than or equal to available start (" + availableEnd + ").");
            }

            Image image = originalImage;

            if (availableStart == requestStart && availableEnd == requestEnd && originalMimeType == desiredMimeType)
            {
                return image;
            }

            // pixels per millisecond
            double ppms = (double)image.Width / (double)(availableEnd - availableStart);

            var pixelsToRemoveFromStart = Convert.ToInt32((requestStart - availableStart) * ppms);
            var pixelsToRemoveFromEnd = Convert.ToInt32((availableEnd - requestEnd) * ppms);

            var targetWidth = image.Width - pixelsToRemoveFromStart - pixelsToRemoveFromEnd;

            var cropRect = new Rectangle(pixelsToRemoveFromStart, 0, targetWidth, image.Height);
            var resultImage = image.Crop(cropRect);

            return resultImage;
        }

        /// <summary>
        /// Ensure <paramref name="sourceImage"/> conforms to <paramref name="height"/> and <paramref name="width"/>.
        /// Removes DC value if required.
        /// </summary>
        /// <param name="sourceImage">
        /// The source Image.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="removeBottomRow">
        /// True to remove the DC Value (bottom row of pixels).
        /// </param>
        /// <exception cref="ImageTooLargeForGraphicsException">
        /// Graphics error drawing spectrogram.
        /// </exception>
        /// <returns>
        /// Modified image.
        /// </returns>
        public static Image Modify(this Image sourceImage, int? height, int? width, bool removeBottomRow)
        {
            /*
             * Resize the single image.
             * Remove the DC value (bottom 1px) if necessary.
             */

            Bitmap returnImage = null;
            try
            {
                var returnImageWidth = width.HasValue ? width.Value : sourceImage.Width;
                var returnImageHeight = height.HasValue ? height.Value :
                    (sourceImage.Height - (removeBottomRow ? 1 : 0));

                returnImage = new Bitmap(returnImageWidth, returnImageHeight);

                using (var graphics = Graphics.FromImage(returnImage))
                {
                    GraphicsSegmented.Draw(graphics, sourceImage, returnImage.Height, returnImage.Width);
                }
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("Parameter is not valid"))
                {
                    throw new ImageTooLargeForGraphicsException(
                        returnImage == null ? new int?() : returnImage.Width,
                        returnImage == null ? new int?() : returnImage.Height,
                        null,
                        "Graphics error drawing spectrogram.",
                        ex);
                }

                throw;
            }

            return returnImage;
        }

        /// <summary>
        /// Get Image from byte array.
        /// </summary>
        /// <param name="bytes">
        /// The byte array.
        /// </param>
        /// <returns>
        /// Image from byte array.
        /// </returns>
        public static Image ToImage(this byte[] bytes)
        {
            Image image;
            using (var ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }

        /// <summary>
        /// Supports 4 formats:
        /// #AARRGGBB
        /// AARRGGBB
        /// #RRGGBB
        /// RRGGBB.
        /// </summary>
        /// <param name="color">
        /// A textual representation of a color.
        /// </param>
        /// <returns>
        /// The <c>Color</c> parsed from the input.
        /// </returns>
        public static Color ColorFromHexString(this string color)
        {
            if (color.Length < 6 || color.Length > 9)
            {
                throw new ArgumentOutOfRangeException("color");
            }

            if (!Regex.IsMatch(color, RegExHexColor))
            {
                throw new ArgumentOutOfRangeException("color", ColorRegExError);
            }

            if (color[0] == '#')
            {
                color = color.Substring(1);
            }

            uint total = uint.Parse(color, NumberStyles.HexNumber);
            byte[] parts = BitConverter.GetBytes(total);

            if (parts.Length == 4)
            {
                return Color.FromArgb(parts[3], parts[2], parts[1], parts[0]);
            }

            return Color.FromArgb(255, parts[2], parts[1], parts[0]);
        }

    }
}
