using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TowseyLibrary
{

    /// <summary>
    ///  Code for CUBEHELIX adapted from javascript code obtained from following website: https://www.mrao.cam.ac.uk/~dag/CUBEHELIX/
    /// </summary>
    public class ColourCubeHelix
    {
        public const string DEFAULT = "default";
        public const string GRAYSCALE = "grayscale";

        public class HSLColour 
        {
            public int Hue { get; set; }
            public double Sat { get; set; }
            public double Lit { get; set; }

            public HSLColour(int H, double S, double L)
            {
                Hue = H;
                Sat = S;
                Lit = L;
            }
        }

        public Color[] ColourPallette;

        public const int maxPalletteSize = 256;
        int maxPalletteIndex = maxPalletteSize - 1;


        public ColourCubeHelix(string mode)
        {
            if (mode.Equals(ColourCubeHelix.DEFAULT))
            {
                HSLColour c1 = new HSLColour( 300, 0.5, 0.0);
                HSLColour c2 = new HSLColour(-240, 0.5, 1.0);
            }
            else
                if (mode.Equals(ColourCubeHelix.GRAYSCALE))
                {
                    ColourPallette = ImageTools.GrayScale();
                }
                else
                {
                    LoggedConsole.WriteErrorLine("WARNING: {0} is UNKNOWN COLOUR PALLETTE!", mode);
                }
        }


        public Color GetColour(int colourID)
        {
            return ColourPallette[colourID];
        }

        /// <summary>
        /// This method assumes that the intensity lies in [0,1]
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public Color GetColour(double intensity)
        {
            int colourID = (int)Math.Floor(intensity * maxPalletteIndex);
            return ColourPallette[colourID];
        }


        /// <summary>
        /// Draws matrix without normalising the values in the matrix.
        /// Assumes some form of normalisation already done.
        /// </summary>
        /// <param name="matrix">the data</param>
        /// <param name="pathName"></param>
        public Image DrawMatrixWithoutNormalisation(double[,] matrix)
        {
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int colourID = (int)Math.Floor(matrix[r, c] * maxPalletteIndex);

                    if (colourID < 0) { colourID = 0; }
                    else
                    { if (colourID > maxPalletteIndex) colourID = maxPalletteIndex; }

                    bmp.SetPixel(c, r, this.ColourPallette[colourID]);
                }//end all columns
            }//end all rows
            return bmp;
        }



    }
}
