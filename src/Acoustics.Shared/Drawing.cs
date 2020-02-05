using System;
using System.Collections.Generic;
using System.Text;

namespace Acoustics.Shared
{
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;
    using SixLabors.Shapes;

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

        public static Image<Rgb24> NewImage(int width, int height, Color fill)
        {
            return new Image<Rgb24>(DefaultConfiguration, width, height, fill);
        }

        public static SizeF MeasureString(this IImageProcessingContext _, string text, Font font)
        {
            return TextMeasurer.Measure(text, new RendererOptions(font));
        }

        public static Size ToSize(this SizeF size)
        {
            return (Size)size;
        }

        public static void DrawVerticalText(this IImageProcessingContext context, string text, Font font, Color color, Point location)
        {
            var (width, height) = TextMeasurer.Measure(text, new RendererOptions(font));
            var image = new Image<Rgba32>(Configuration.Default, (int)(width + 1), (int)(height + 1), Rgba32.Transparent);

            image.Mutate(x => x
                .DrawText(new TextGraphicsOptions(true), text, font, color, new PointF(0, 0))
                .Rotate(-90));

            context.DrawImage(image, location, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcAtop, 1);

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
