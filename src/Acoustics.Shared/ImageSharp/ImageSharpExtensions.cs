// <copyright file="ImageSharpExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

// ReSharper disable once CheckNamespace
namespace SixLabors.ImageSharp
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Numerics;
    using System.Text.RegularExpressions;
    using Acoustics.Shared.ImageSharp;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Image extension methods.
    /// </summary>
    public static class ImageSharpExtensions
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
        /// <returns>
        /// Modified image.
        /// </returns>
        [Obsolete("This shim only exists for compatibility. Not needed when ImageSharp replaced System.Drawing")]
        public static Image ModifySpectrogram(this Image sourceImage, int? height, int? width, bool removeBottomRow)
        {
            var amountToRemove = removeBottomRow ? 1 : 0;

            var sourceRectangle = new Rectangle(
                0, 0, sourceImage.Width, sourceImage.Height - amountToRemove);

            var returnSize = new Size(
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

        public static void DrawImage(
            this Image destination,
            Image source,
            Rectangle destinationRectangle,
            Rectangle sourceRectangle)
        {
            destination.Mutate(
                d => d.DrawImage(
                    source.Clone(
                        s => s.Crop(sourceRectangle)
                            .Resize(destinationRectangle.Size)),
                    destinationRectangle.Location,
                    1.0f));
        }

        public static Drawing.NoAA NoAA(this IImageProcessingContext context)
        {
            return new Drawing.NoAA(context);
        }

        public static void DrawLine(this IImageProcessingContext context, Pen pen, int x1, int y1, int x2, int y2)
        {
            context.DrawLines(pen, new PointF(x1, y1), new PointF(x2, y2));
        }

        public static void DrawRectangle(this IImageProcessingContext context, Pen pen, int x, int y, int width, int height)
        {
            var r = new RectangleF(x, y, width, height);
            context.Draw(pen, r);
        }

        public static void FillRectangle(this IImageProcessingContext context, IBrush brush, int x, int y, int width, int height)
        {
            var r = new RectangleF(x, y, width, height);
            context.Fill(brush, r);
        }

        public static void Clear(this IImageProcessingContext context, Color color)
        {
            context.Fill(color);
        }

        /// <summary>
        /// Fills a rectangle with color that blends with the background.
        /// If the given <paramref name="brush"/> contains an alpha component,
        /// that component will be used as the <c>BlendPercentage</c> value.
        /// </summary>
        /// <remarks>
        /// Apparently blending pixels with transparency is not supported for Rgb24 images.
        /// See the FillDoesNotBlendByDefault.Test smoke test.
        /// </remarks>
        /// <param name="context">The drawing context.</param>
        /// <param name="brush">The brush to fill with.</param>
        /// <param name="paths">If specified, a collection of regions to fill.</param>
        public static void FillWithBlend(this IImageProcessingContext context, IBrush brush, params IPath[] paths)
        {
            const float Opaque = 1f;
            var options = new GraphicsOptions();

            if (brush is SolidBrush s)
            {
                var alpha = ((Vector4)s.Color).W;
                if (alpha != Opaque)
                {
                    // move opacity from color to graphics layer
                    options.BlendPercentage = alpha;
                    brush = new SolidBrush(s.Color.WithAlpha(1));
                }
            }
            else
            {
                throw new NotSupportedException("Can't handle non-solid brushed");
            }

            if (paths != null && paths.Length > 0)
            {
                context.Fill(options, brush, new PathCollection(paths));
            }
            else
            {
                context.Fill(options, brush);
            }
        }

        /// <inheritdoc cref="FillWithBlend(IImageProcessingContext, IBrush, IPath[])"/>
        /// <param name="region">A rectangular region to fill.</param>
        public static void FillWithBlend(this IImageProcessingContext context, IBrush brush, RectangleF region)
        {
            FillWithBlend(context, brush, new RectangularPolygon(region));
        }

        public static int Area(this Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        /// <summary>
        ///       Returns the Hue-Saturation-Lightness (HSL) lightness
        ///       for this <see cref='System.Drawing.Color'/> .
        /// </summary>
        /// <remarks>
        /// Implementation from https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs,23adaaa39209cc1f.
        /// </remarks>
        public static float GetBrightness(this Rgb24 pixel)
        {
            float r = pixel.R / 255.0f;
            float g = pixel.G / 255.0f;
            float b = pixel.B / 255.0f;

            var max = r;
            var min = r;

            if (g > max)
            {
                max = g;
            }

            if (b > max)
            {
                max = b;
            }

            if (g < min)
            {
                min = g;
            }

            if (b < min)
            {
                min = b;
            }

            return (max + min) / 2;
        }

        public static void Save<T>(this Image<T> image, FileInfo path)
            where T : struct, IPixel<T>
        {
            image.Save(path.ToString());
        }

        public static SizeF ToSizeF(this FontRectangle rectangle)
        {
            return new SizeF(rectangle.Width, rectangle.Height);
        }

        public static SizeF MeasureString(this IImageProcessingContext _, string text, Font font)
        {
            return TextMeasurer.MeasureBounds(text, new RendererOptions(font)).ToSizeF();
        }

        public static Size ToSize(this SizeF size)
        {
            return (Size)size;
        }

        public static Pen ToPen(this Color color, float width = 1)
        {
            return new Pen(color, width);
        }

        public static Rectangle AsRect(this (int X, int Y, int Width, int Height) rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static RectangleF AsRect(this PointF point, SizeF size)
        {
            return new RectangleF(point, size);
        }

        public static RectangleF AsRect(this FontRectangle rectangle)
        {
            return new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static void DrawTextSafe(this IImageProcessingContext context, string text, Font font, Color color, PointF location)
        {
            if (text.IsNullOrEmpty())
            {
                return;
            }

            // check to see if text overlaps with image
            var destArea = new RectangleF(PointF.Empty, context.GetCurrentSize());
            var rendererOptions = new RendererOptions(font, location);
            var textArea = TextMeasurer.MeasureBounds(text, rendererOptions);
            if (destArea.IntersectsWith(textArea.AsRect()))
            {
                if (textArea.X < 0)
                {
                    // TODO BUG: see https://github.com/SixLabors/ImageSharp.Drawing/issues/30
                    // to get around the bug, we measure each character, and then trim them from the
                    // start of the text, move the location right by the width of the trimmed character
                    // and continue until we're in a spot that does not trigger the bug;
                    int trim = 0;
                    float trimOffset = 0;
                    if (TextMeasurer.TryMeasureCharacterBounds(text, rendererOptions, out var characterBounds))
                    {
                        foreach (var characterBound in characterBounds)
                        {
                            // magic value found empirically, does not seem to trigger bug when first char less than offset equal to char size
                            if (characterBound.Bounds.X > -(font.Size - 2))
                            {
                                break;
                            }

                            trim++;
                            trimOffset += characterBound.Bounds.Width;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Due to a bug with ImageSharp this text cannot be rendered");
                    }

                    location.Offset(trimOffset, 0);
                    context.DrawText(Drawing.TextOptions, text[trim..], font, color, location);
                }
                else
                {
                    context.DrawText(Drawing.TextOptions, text, font, color, location);
                }
            }
        }

        public static void DrawVerticalText(this IImageProcessingContext context, string text, Font font, Color color, Point location)
        {
            var (width, height) = TextMeasurer.Measure(text, new RendererOptions(font)).ToSizeF();
            var image = new Image<Rgba32>(Configuration.Default, (int)(width + 1), (int)(height + 1), Color.Transparent);

            image.Mutate(x => x
                .DrawText(Drawing.TextOptions, text, font, color, new PointF(0, 0))
                .Rotate(-90));

            context.DrawImage(image, location, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcAtop, 1);
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
        public static Image<T> Crop<T>(this Image<T> source, Rectangle crop)
            where T : struct, IPixel<T> => source.Clone(x => x.Crop(crop));

        /// <summary>
        /// Crop an image using a <paramref name="crop"/> Rectangle.
        /// If <paramref name="crop"/> spills over <paramref name="source"/> only intersecting areas are returned.
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
        public static Image<T> CropIntersection<T>(this Image<T> source, Rectangle crop)
            where T : struct, IPixel<T>
        {
            var intersection = Rectangle.Intersect(crop, source.Bounds());

            return source.Clone(x => x.Crop(intersection));
        }

        /// <summary>
        /// Draw a crop of an image onto a rectangle surface. Unlike crop, it treats the rectangle coordinates
        /// as the source of truth and returns a new image, with a section of <paramref name="source"/> drawn on top.
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
        public static Image<T> CropInverse<T>(this Image<T> source, Rectangle crop)
            where T : struct, IPixel<T>
        {
            var result = new Image<T>(crop.Width, crop.Height);

            var intersection = source.Bounds();
            intersection.Intersect(crop);

            result.Mutate(x => x.DrawImage(
                source.CropIntersection(intersection),
                new Point(intersection.X - crop.X, intersection.Y - crop.Y),
                1));

            return result;
        }

        public static void RotateFlip<T>(this Image<T> image, RotateFlipType operation)
            where T : struct, IPixel<T>
        {
            RotateMode r;
            FlipMode f;
            switch (operation)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    f = FlipMode.None;
                    r = RotateMode.None;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    f = FlipMode.None;
                    r = RotateMode.Rotate90;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                    f = FlipMode.None;
                    r = RotateMode.Rotate180;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    f = FlipMode.None;
                    r = RotateMode.Rotate270;
                    break;
                case RotateFlipType.RotateNoneFlipX:
                    f = FlipMode.Horizontal;
                    r = RotateMode.None;
                    break;
                case RotateFlipType.Rotate90FlipX:
                    f = FlipMode.Horizontal;
                    r = RotateMode.Rotate90;
                    break;
                case RotateFlipType.Rotate180FlipX:
                    f = FlipMode.Horizontal;
                    r = RotateMode.Rotate180;
                    break;
                case RotateFlipType.Rotate270FlipX:
                    f = FlipMode.Horizontal;
                    r = RotateMode.Rotate270;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }

            image.Mutate(x => x.RotateFlip(r, f));
        }


    }
}