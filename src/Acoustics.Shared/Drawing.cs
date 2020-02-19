using System;

namespace Acoustics.Shared
{
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Helpers for drawing images.
    /// </summary>
    /// <remarks>
    /// <seealso cref="ExtensionsDrawing"/>
    /// </remarks>
    public static class Drawing
    {
        private const string Tahoma = "Tahoma";
        private const string Arial = "Arial";

        public static readonly Configuration DefaultConfiguration = Configuration.Default;

        public static readonly Font Tahoma6 = SystemFonts.CreateFont(Tahoma, 6f);
        public static readonly Font Tahoma9 = SystemFonts.CreateFont(Tahoma, 9f);
        public static readonly Font Tahoma8 = SystemFonts.CreateFont(Tahoma, 8f);
        public static readonly Font Tahoma12 = SystemFonts.CreateFont(Tahoma, 12f);
        public static readonly Font Arial8 = SystemFonts.CreateFont(Arial, 8f);
        public static readonly Font Arial8Bold = SystemFonts.CreateFont(Arial, 8f, FontStyle.Bold);
        public static readonly Font Arial9 = SystemFonts.CreateFont(Arial, 9f);
        public static readonly Font Arial9Bold = SystemFonts.CreateFont(Arial, 9f, FontStyle.Bold);
        public static readonly Font Arial10 = SystemFonts.CreateFont(Arial, 10f);
        public static readonly Font Arial12 = SystemFonts.CreateFont(Arial, 12f);
        public static readonly Font Arial12Bold = SystemFonts.CreateFont(Arial, 12f, FontStyle.Bold);
        public static readonly Font Arial14 = SystemFonts.CreateFont(Arial, 14f);

        public static Font GetArial(float size)
        {
            // TODO: cache?
            return SystemFonts.CreateFont(Arial, size);
        }

        public static readonly Configuration NoParallelConfiguration = new Configuration() {
            MaxDegreeOfParallelism = 1,
        };

        public static Image<Rgb24> NewImage(int width, int height, Color fill)
        {
            return new Image<Rgb24>(DefaultConfiguration, width, height, fill);
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

        public static void DrawTextSafe(this IImageProcessingContext context, string text, Font font,
            Color color, PointF location)
            {
            if (text.IsNullOrEmpty())
            {
                return;
            }

            context.DrawText(text, font, color, location);

        }


        public static void DrawVerticalText(this IImageProcessingContext context, string text, Font font, Color color, Point location)
        {
            var (width, height) = TextMeasurer.Measure(text, new RendererOptions(font)).ToSizeF();
            var image = new Image<Rgba32>(Configuration.Default, (int)(width + 1), (int)(height + 1), Color.Transparent);

            image.Mutate(x => x
                .DrawText(new TextGraphicsOptions(), text, font, color, new PointF(0, 0))
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
            var result  = new Image<T>(crop.Width, crop.Height);

            var intersection = source.Bounds();
            intersection.Intersect(crop);

            result.Mutate(x => x.DrawImage(
                source.CropIntersection(intersection),
                new Point(intersection.X - crop.X, intersection.Y - crop.Y),
                1));

            return result;
        }

        public static void RotateFlip<T>(this Image<T> image, RotateFlipType operation) where T : struct, IPixel<T>
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

    public enum RotateFlipType
    {

        RotateNoneFlipNone = 0,

        Rotate90FlipNone = 1,

        Rotate180FlipNone = 2,

        Rotate270FlipNone = 3,

        RotateNoneFlipX = 4,

        Rotate90FlipX = 5,

        Rotate180FlipX = 6,

        Rotate270FlipX = 7,

        RotateNoneFlipY = Rotate180FlipX,

        Rotate90FlipY = Rotate270FlipX,

        Rotate180FlipY = RotateNoneFlipX,

        Rotate270FlipY = Rotate90FlipX,

        RotateNoneFlipXY = Rotate180FlipNone,

        Rotate90FlipXY = Rotate270FlipNone,

        Rotate180FlipXY = RotateNoneFlipNone,

        Rotate270FlipXY = Rotate90FlipNone
    }
}
