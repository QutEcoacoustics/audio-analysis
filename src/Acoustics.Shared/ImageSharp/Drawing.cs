// <copyright file="Drawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ImageSharp
{
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

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

        private const string Tahoma = "Tahoma";
        private const string Arial = "Arial";

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
    }
}
