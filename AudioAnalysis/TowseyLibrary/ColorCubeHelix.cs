// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorCubeHelix.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace TowseyLibrary
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>
    ///     Code for CUBEHELIX adapted from JavaScript code obtained from following website:
    ///     <c>https://www.mrao.cam.ac.uk/~dag/CUBEHELIX/</c>
    /// </summary>
    public class ColorCubeHelix
    {
        public const string Default = "default";
        public const string Grayscale = "grayscale";
        public const int MaxPaletteSize = 256;
        private readonly int maxPaletteIndex = MaxPaletteSize - 1;
        private readonly Color[] colorPalette;

        public ColorCubeHelix(string mode)
        {
            if (mode.Equals(Default))
            {
                var c1 = new HslColor(300, 0.5, 0.0);
                var c2 = new HslColor(-240, 0.5, 1.0);
            }
            else if (mode.Equals(Grayscale))
            {
                this.colorPalette = ImageTools.GrayScale();
            }
            else
            {
                LoggedConsole.WriteErrorLine("WARNING: {0} is UNKNOWN COLOUR PALLETTE!", mode);
            }
        }

        public Color GetColor(int colorId)
        {
            return this.colorPalette[colorId];
        }

        /// <summary>
        /// This method assumes that the intensity lies in [0,1]
        /// </summary>
        /// <param name="intensity">
        /// </param>
        /// <returns>
        /// The <see cref="Color"/>.
        /// </returns>
        public Color GetColor(double intensity)
        {
            var colourID = (int)Math.Floor(intensity * this.maxPaletteIndex);
            return this.colorPalette[colourID];
        }

        /// <summary>
        /// Draws matrix without normalizing the values in the matrix.
        ///     Assumes some form of normalization already done.
        /// </summary>
        /// <param name="matrix">
        /// the data
        /// </param>
        /// <returns>
        /// The <see cref="Image"/>.
        /// </returns>
        public Image DrawMatrixWithoutNormalization(double[,] matrix)
        {
            var rows = matrix.GetLength(0); // number of rows
            var cols = matrix.GetLength(1); // number

            var bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var colourId = (int)Math.Floor(matrix[r, c] * this.maxPaletteIndex);

                    if (colourId < 0)
                    {
                        colourId = 0;
                    }
                    else
                    {
                        if (colourId > this.maxPaletteIndex)
                        {
                            colourId = this.maxPaletteIndex;
                        }
                    }

                    bmp.SetPixel(c, r, this.colorPalette[colourId]);
                }
            }

            return bmp;
        }

        public class HslColor
        {
            public HslColor(int H, double S, double L)
            {
                this.Hue = H;
                this.Sat = S;
                this.Lit = L;
            }

            public int Hue { get; set; }

            public double Sat { get; set; }

            public double Lit { get; set; }
        }
    }
}