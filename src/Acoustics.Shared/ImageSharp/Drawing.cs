// <copyright file="Drawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System;
    using System.IO;
    using System.Linq;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
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
        public const string Roboto = nameof(Roboto);

        /// <summary>
        /// A predefined set of graphical options. Currently is equivalent to the default.
        /// </summary>
        public static readonly Configuration DefaultConfiguration = Configuration.Default;

        /// <summary>
        /// A predefined set of graphics options with parallelization disabled.
        /// </summary>
        public static readonly Configuration NoParallelConfiguration = new Configuration()
        {
            MaxDegreeOfParallelism = 1,
        };

        /// <summary>
        /// A predefined set of graphics options that have anti-aliasing disabled.
        /// </summary>
        public static readonly ShapeGraphicsOptions NoAntiAlias = new ShapeGraphicsOptions()
        {
            GraphicsOptions = new GraphicsOptions()
            {
                Antialias = false,
                BlendPercentage = 1,
                ColorBlendingMode = PixelColorBlendingMode.Normal,
                AntialiasSubpixelDepth = 0,
            },

            //IntersectionRule = IntersectionRule.OddEven,
        };

        /// <summary>
        /// A predefined set of options for rendering text. Currently is equivalent to the default.
        /// </summary>
        public static readonly TextGraphicsOptions TextOptions = new TextGraphicsOptions()
        {
            // noop currently
        };

        internal const string Tahoma = nameof(Tahoma);

        internal const string Arial = nameof(Arial);

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

        public static Font Roboto6 => GetFont(Roboto, 6f);

        public static Font Roboto10 => GetFont(Roboto, 10f);

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
        /// Gets the requested font family or falls back to using <c>Roboto</c>.
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
            where T : unmanaged, IPixel<T>
        {
            return new Image<T>(DefaultConfiguration, width, height, fill.ToPixel<T>());
        }

        /// <summary>
        /// Measures the placement of each character in a rendered string of text.
        /// </summary>
        public static GlyphMetric[] MeasureCharacters(string text, Font font, PointF location)
        {
            var rendererOptions = new RendererOptions(font, location);

            TextMeasurer.TryMeasureCharacterBounds(text, rendererOptions, out var characterBounds);
            //font.Instance.
            return characterBounds;
        }

        /// <summary>
        /// A specialized class the deals with drawing graphics without anti-aliasing.
        /// It deals with two issues:
        /// - Lines in ImageSharp are drawn on the centre pixel. Without AA they're drawn a pixel
        ///   off. This class draws all lines with +0.0,+0.5 coordinates.
        ///   See https://github.com/SixLabors/ImageSharp.Drawing/issues/28
        /// - It also applies the NoAntiAliasing profile by default to all operations.
        /// </summary>
        public class NoAA
        {
            public static readonly PointF Bug28Offset = new PointF(0.0f, 0.5f);

            private readonly IImageProcessingContext context;

            public NoAA(IImageProcessingContext context)
            {
                this.context = context;
            }

            public void DrawLine(IPen pen, int x1, int y1, int x2, int y2)
            {
                this.DrawLines(pen, new Point(x1, y1), new Point(x2, y2));
            }

            public void DrawLines(IPen pen, params PointF[] points)
            {
                // i've no idea why, but repeating the first point  and last point
                // and adding random offsets in reduces visual errors in line drawing!
                var slope = points[0].Y.CompareTo(points[^1].Y) switch
                {
                    -1 => 0.0f,
                    0 => 0,
                    1 => 0.5f,
                    _ => throw new NotImplementedException(),
                };
                var offset = new PointF(slope, Bug28Offset.Y);
                var modifiedPoints = points
                    .Select(p => p + offset)
                    .Prepend(points[0] + Bug28Offset)
                    .Append(points[^1] + Bug28Offset)
                    .ToArray();

                this.context.DrawLines(
                    NoAntiAlias,
                    pen,
                    modifiedPoints);
            }

            public void DrawLines(Color color, float thickness, params PointF[] points)
            {
                this.DrawLines(new Pen(color, thickness), points);
            }

            public void DrawRectangle(Pen pen, int x1, int y1, int x2, int y2)
            {
                var r = RectangleF.FromLTRB(x1, y1, x2, y2);
                r.Offset(Bug28Offset);
                this.context.Draw(Drawing.NoAntiAlias, pen, r);
            }

            /// <summary>
            /// Draws a <paramref name="thickness"/> thick (1px by default)
            /// bordered rectangle with no fill with anti-aliasing disabled.
            /// </summary>
            public void DrawRectangle(Color color, int x1, int y1, int x2, int y2, float thickness = 1f)
            {
                var r = RectangleF.FromLTRB(x1, y1, x2, y2);
                r.Offset(Bug28Offset);
                this.context.Draw(Drawing.NoAntiAlias, color, thickness, r);
            }

            public void DrawRectangle(Pen border, RectangleF rectangle)
            {
                rectangle.Offset(Bug28Offset);
                this.context.Draw(Drawing.NoAntiAlias, border, rectangle);
            }

            /// <summary>
            /// Draws a border line on the inside perimiter of rectangle.
            /// ImageSharp by default draws lines split either side of the imaginary
            /// center line.
            /// </summary>
            /// <remarks>
            ///  ImageSharp's Draw Rectangle is unpredictable and buggy, especially for
            ///  non-antialiased operations. See <c>Acoustics.Test.Shared.Drawing.RectangleCornerBugTest</c>.
            ///  This method instead draws four lines as the border.
            /// </remarks>
            public void DrawBorderInset(Pen border, RectangleF rectangle)
            {
                // rounder border thickness
                border = new Pen(
                    border.StrokeFill,
                    MathF.Round(border.StrokeWidth),
                    border.StrokePattern.ToArray());

                // first round rectangle to nice coordinates
                var rect = Rectangle.Round(rectangle);

                // construct point coordinats, offset by pen width inset into rectangle.
                var penOffset = MathF.Floor(border.StrokeWidth / 2f);

                // empircally found to satisfy tests - i have no idea why it works
                var widthAdjustment = ((int)border.StrokeWidth % 2) == 0 ? 0.0f : -0.5f;

                float left = rect.Left + penOffset + 0.0f;
                float top = rect.Top + penOffset + +Bug28Offset.Y;
                float right = rect.Right - penOffset + widthAdjustment;
                float bottom = rect.Bottom - penOffset + Bug28Offset.Y + widthAdjustment;

                this.context.DrawPolygon(
                    NoAntiAlias,
                    border,
                    new PointF(left, top),
                    new PointF(right, top),
                    new PointF(right, bottom),
                    new PointF(left, bottom),
                    new PointF(left, top));
            }

            public void FillRectangle(IBrush brush, int x1, int y1, int x2, int y2)
            {
                var r = RectangleF.FromLTRB(x1, y1, x2, y2);
                r.Offset(Bug28Offset);
                this.context.Fill(Drawing.NoAntiAlias, brush, r);
            }
        }
    }
}