// <copyright file="ExtensionsDrawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
    using Globalization;
    using IO;
    using System.Text.RegularExpressions;
    using Acoustics.Shared;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.ColorSpaces;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

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
        public static byte[] ToByteArray(this Image image, IImageEncoder imageFormat)
        {
            if (image == null)
            {
                return new byte[0];
            }

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
        [Obsolete("This shim only exists for compatibility. Not needed when ImageSharp replaced System.Drawing")]
        public static Image Crop(this Image source, Rectangle crop) => source.Clone(x => x.Crop(crop));

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
            var result = $"#{color.ToHex()}";
            return includeAlpha ? result : result[0..6];
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

            var returnSize =  new Size(
                width ?? sourceImage.Width,
                height ?? sourceImage.Height - amountToRemove);

            return sourceImage.Clone(x => x.Crop(sourceRectangle).Resize(returnSize));
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
                image = Image.Load(ms);
            }

            return image;
        }

        /// <summary>
        /// Supports 4 formats:
        /// #RRGGBBAA
        /// RRGGBBAA
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
                throw new ArgumentOutOfRangeException(nameof(color));
            }

            if (!Regex.IsMatch(color, RegExHexColor))
            {
                throw new ArgumentOutOfRangeException(nameof(color), ColorRegExError);
            }

            if (color[0] == '#')
            {
                color = color.Substring(1);
            }

            uint total = uint.Parse(color, NumberStyles.HexNumber);
            byte[] parts = BitConverter.GetBytes(total);

            if (color.Length != 8 || color.Length != 6)
            {
                throw new NotSupportedException("Cannot parse color.");
            }

            var r = byte.Parse(color[0..1], NumberStyles.AllowHexSpecifier);
            var g = byte.Parse(color[2..3], NumberStyles.AllowHexSpecifier);
            var b = byte.Parse(color[4..5], NumberStyles.AllowHexSpecifier);

            var a = color.Length == 8 ? byte.Parse(color[6..7], NumberStyles.AllowHexSpecifier) : (byte)255;
            return Color.FromRgba(r, g, b, a);
        }

        public static Color Gray(byte tone)
        {
            return Color.FromRgb(tone, tone, tone);
        }

        public static void DrawImage(this Image destination, Image source, Rectangle destinationRectangle, Rectangle sourceRectangle)
        {
            destination.Mutate(
                d => d.DrawImage(
                    source.Clone(
                            s => s.Crop(sourceRectangle)
                                .Resize(destinationRectangle.Size)),
                    destinationRectangle.Location,
                    1.0f));
        }
    }
}
