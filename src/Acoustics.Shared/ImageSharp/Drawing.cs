// <copyright file="Drawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using System.Runtime.CompilerServices;
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

        //public static readonly ShapeGraphicsOptions NoAntiAlias = new ShapeGraphicsOptions()
        //{
        //    BlendPercentage = 1,
        //    Antialias = false,
        //    ColorBlendingMode = PixelColorBlendingMode.Normal,
        //    AntialiasSubpixelDepth = 0,
        //};

        private const string Tahoma = "Tahoma";
        private const string Arial = "Arial";

        static Drawing()
        {
        }

        public static Font GetArial(float size)
        {
            // TODO: cache?
            return SystemFonts.CreateFont(Arial, size);
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
                    Drawing.NoAntiAlias,
                    pen,
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
