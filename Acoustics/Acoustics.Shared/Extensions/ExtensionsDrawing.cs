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
        /// <summary>
        /// The color reg ex error.
        /// </summary>
        public const string ColorRegExError = "Must adhere to a standard hex color code (#00000000)";

        /// <summary>
        /// The reg ex hex color.
        /// </summary>
        public const string RegExHexColor = "^#?([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$";

        /// <summary>
        /// Convert an image to a byte array.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <param name="imageFormat">
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
        /// <param name="source">
        /// Source image.
        /// </param>
        /// <param name="crop">
        /// Crop rectangle.
        /// </param>
        /// <returns>
        /// Cropped image.
        /// </returns>
        /// <remarks>
        /// Use Graphics.DrawImage() to copy the selection portion of the source image. 
        /// You'll need the overload that takes a source and a destination Rectangle. 
        /// Create the Graphics instance from Graphics.FromImage() on a new bitmap that 
        /// has the same size as the rectangle.
        /// from: http://stackoverflow.com/questions/2405261/how-to-clip-a-rectangle-from-a-tiff.
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
        /// Resize <paramref name="sourceImage"/> to match <paramref name="height"/> and <paramref name="width"/>.
        /// Removes DC value if <paramref name="removeBottomRow"/> is true.
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
            var amountToRemove = removeBottomRow ? 1 : 0;

            var sourceRectangle = new Rectangle(
                0, 0, sourceImage.Width, sourceImage.Height - amountToRemove);

            var returnImage = new Bitmap(
                width.HasValue ? width.Value : sourceImage.Width,
                height.HasValue ? height.Value : (sourceImage.Height - amountToRemove));

            var destRectangle = new Rectangle(0, 0, returnImage.Width, returnImage.Height);

            try
            {
                using (var graphics = Graphics.FromImage(returnImage))
                {
                    graphics.DrawImage(sourceImage, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    //GraphicsSegmented.Draw(graphics, sourceImage, returnImage.Height, returnImage.Width);
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
