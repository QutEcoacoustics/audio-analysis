// <copyright file="Drawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using System.IO;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Helpers for drawing images.
    /// </summary>
    /// <remarks>
    /// <seealso cref="ImageSharpExtensions"/>
    /// </remarks>
    public static class Drawing
    {
        /// <summary>
        /// An open source sans-serif font produced by Google that is hopefully a good fallback for missing fonts.
        /// </summary>
        public const string Roboto = "Roboto";
        public static readonly Configuration DefaultConfiguration = Configuration.Default;

        public static readonly Configuration NoParallelConfiguration = new Configuration()
        {
            MaxDegreeOfParallelism = 1,
        };

        public static readonly ShapeGraphicsOptions NoAntiAlias = new ShapeGraphicsOptions()
        {
            BlendPercentage = 1,
            Antialias = false,
            ColorBlendingMode = PixelColorBlendingMode.Normal,
            AntialiasSubpixelDepth = 0,

            //IntersectionRule = IntersectionRule.OddEven,
        };

        public static readonly TextGraphicsOptions TextOptions = new TextGraphicsOptions()
        {
            // noop currently
        };

        internal const string Tahoma = "Tahoma";

        internal const string Arial = "Arial";

        /// <summary>
        /// Fonts bundled with AP.exe.
        /// </summary>
        private static readonly Lazy<FontCollection> BundledFontCollection = new Lazy<FontCollection>(() =>
            {
                var collection = new FontCollection();
                var fontDirectory = System.IO.Path.Combine(AppConfigHelper.ExecutingAssemblyDirectory, "fonts", Roboto);
                var fonts = Directory.EnumerateFiles(fontDirectory, "*.ttf");
                foreach (var font in fonts)
                {
                    collection.Install(font);
                }

                return collection;
            });

        static Drawing()
        {
        }

        public static Font Tahoma6 => GetFont(Tahoma, 6f);

        public static Font Tahoma9 => GetFont(Tahoma, 9f);

        public static Font Tahoma8 => GetFont(Tahoma, 8f);

        public static Font Tahoma12 => GetFont(Tahoma, 12f);

        public static Font Arial8 => GetFont(Arial, 8f);

        public static Font Arial8Bold => GetFont(Arial, 8f, FontStyle.Bold);

        public static Font Arial9 => GetFont(Arial, 9f);

        public static Font Arial9Bold => GetFont(Arial, 9f, FontStyle.Bold);

        public static Font Arial10 => GetFont(Arial, 10f);

        public static Font Arial12 => GetFont(Arial, 12f);

        public static Font Arial12Bold => GetFont(Arial, 12f, FontStyle.Bold);

        public static Font Arial14 => GetFont(Arial, 14f);

        public static Font Arial16 => GetFont(Arial, 16f);

        /// <summary>
        /// Gets (or initializes) fonts bundled with AP.exe.
        /// </summary>
        public static FontCollection BundledFonts => BundledFontCollection.Value;

        /// <summary>
        /// Gets the requested font family or falls back to using <see cref="Roboto"/>.
        /// </summary>
        /// <param name="fontFamily">The name of the font family to get.</param>
        /// <param name="size">The requested size of the returned font.</param>
        /// <param name="style">The requested style of the returned font.</param>
        /// <returns>Thr requested font, or the Roboto font if the requested font cannot be found.</returns>
        public static Font GetFont(string fontFamily, float size, FontStyle style = FontStyle.Regular)
        {
            // TODO: cache?
            if (fontFamily == Roboto)
            {
                // shortcut case
                return BundledFonts.CreateFont(fontFamily, size, style);
            }
            else if (SystemFonts.TryFind(fontFamily, out var family))
            {
                // default case
                return family.CreateFont(size, style);
            }
            else
            {
                // fallback case
                return BundledFonts.CreateFont(Roboto, size, style);
            }
        }

        public static Image<Rgb24> NewImage(int width, int height, Color fill)
        {
            return new Image<Rgb24>(DefaultConfiguration, width, height, fill);
        }

        public static Image<T> NewImage<T>(int width, int height, Color fill)
            where T : struct, IPixel<T>
        {
            return new Image<T>(DefaultConfiguration, width, height, fill.ToPixel<T>());
        }

        /// <summary>
        /// A specialized class the deals with drawing graphics without anti-aliasing.
        /// It deal with two issues:
        /// - Lines in ImageSharp are drawn on the centre pixel. Without AA they're drawn a pixel
        ///   off. This class draws all lines with +0.0,+0.5 coordinates.
        ///   See https://github.com/SixLabors/ImageSharp.Drawing/issues/28
        /// - It also applies the NoAntiAliasing profile by default to all operations.
        /// </summary>
        public class NoAA
        {
            private static readonly PointF Offset = new PointF(0.0f, 0.5f);
            private readonly IImageProcessingContext context;

            public NoAA(IImageProcessingContext context)
            {
                this.context = context;
            }

            public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
            {
                var a = new PointF(x1, y1) + Offset;
                var b = new PointF(x2, y2) + Offset;

                this.context.DrawLines(
                    Drawing.NoAntiAlias,
                    pen,
                    a,
                    b);
            }

            public void DrawLine(IPen pen, params PointF[] points)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].Offset(Offset);
                }

                this.context.DrawLines(
                    NoAntiAlias,
                    pen,
                    points);
            }

            public void DrawLine(Color color, float thickness, params PointF[] points)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].Offset(Offset);
                }

                this.context.DrawLines(
                    NoAntiAlias,
                    color,
                    thickness,
                    points);
            }

            public void DrawRectangle(Pen pen, int x1, int y1, int x2, int y2)
            {
                var r = RectangleF.FromLTRB(x1, y1, x2, y2);
                r.Offset(Offset);
                this.context.Draw(Drawing.NoAntiAlias, pen, r);
            }

            public void FillRectangle(IBrush brush, int x1, int y1, int x2, int y2)
            {
                var r = RectangleF.FromLTRB(x1, y1, x2, y2);
                r.Offset(Offset);
                this.context.Fill(Drawing.NoAntiAlias, brush, r);
            }
        }
    }
}
