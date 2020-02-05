// <copyright file="TernaryPlots.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;

    public static class TernaryPlots
    {
        /// <summary>
        /// Draws a ternary plot from the passed data.
        /// Assumes that the passed matrix values are all normalised in [0,1].
        /// </summary>
        /// <param name="matrixDictionary">dicitonary of matrices - each matrix is one index</param>
        /// <param name="keys">The names of the three indices/attributes. Also used as keys to dicitonary of matrices.</param>
        /// <returns></returns>
        public static Image DrawTernaryPlot(Dictionary<string, double[,]> matrixDictionary, string[] keys)
        {
            int rowCount = matrixDictionary[keys[0]].GetLength(0);
            int colCount = matrixDictionary[keys[0]].GetLength(1);
            int scale = 500;
            double factor = Math.Sqrt(3.0) / 2.0;
            int height = (int)Math.Ceiling(500 * factor);
            var histo1 = new double[scale + 1, scale + 1];
            var histo2 = new double[scale + 1, scale + 1];
            var histo3 = new double[scale + 1, scale + 1];

            double[] triplet = new double[3];
            string label1 = "Ternary Plot: ";
            string label2 = "";
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    triplet[0] = matrixDictionary[keys[0]][r, c];
                    triplet[1] = matrixDictionary[keys[1]][r, c];
                    triplet[2] = matrixDictionary[keys[2]][r, c];
                    double tripletSum = triplet.Sum();

                    // ignore points with low values
                    if (tripletSum < 0.5)
                    {
                        continue;
                    }

                    triplet = DataTools.Normalise2Probabilites(triplet);

                    // for testing purposes
                    //triplet[0] = 0.0;
                    //triplet[1] = 0.0;
                    //triplet[2] = 0.95;

                    // in converting to ternary plot, a+b+c = 1.0 because triplet has already been normalised
                    double x = triplet[0] * factor;
                    double y = (triplet[0] + (2 * triplet[1])) / 2;
                    int xCoord = scale - (int)Math.Round(x * scale);
                    int yCoord = scale - (int)Math.Round(y * scale);

                    //int yCoord = (int)Math.Round(y * scale);

                    // time of day
                    if (r < 360 || r > 1053)
                    {
                        histo3[xCoord, yCoord]++; // night blue
                    }
                    else
                    if (r > 360 && r < 480)
                    {
                        histo1[xCoord, yCoord]++; // morning chorus red
                    }
                    else
                    {
                        histo2[xCoord, yCoord]++;
                    }

                    label2 = "Time of Day >> RGB=(dawnChorus, day, night)";

                    // frequency band
                    //if (c > 128) histo3[xCoord, yCoord]++; // high frequency
                    //else
                    //if (c > 32) histo2[xCoord, yCoord]++; // mid frequency
                    //else histo1[xCoord, yCoord]++; // low frequency
                    //label2 = "Freq Band >> RGB=(low, mid, hi)";

                    // sum of index values
                    //if (tripletSum > 1.2) histo1[xCoord, yCoord]++;
                    //else
                    //if (tripletSum > 0.6) histo2[xCoord, yCoord]++;
                    //else                  histo3[xCoord, yCoord]++;
                    //label2 = "Index Sum >> RGB=(hi, mid, low)";
                }
            }

            //histo = MatrixTools.Matrix2LogValues(histo);
            //histo = MatrixTools.SquareRootOfValues(histo);
            histo1 = MatrixTools.LogTransform(histo1);
            histo2 = MatrixTools.LogTransform(histo2);
            histo3 = MatrixTools.LogTransform(histo3);
            double[,] norm1 = DataTools.normalise(histo1);
            double[,] norm2 = DataTools.normalise(histo2);
            double[,] norm3 = DataTools.normalise(histo3);
            var stringFont = Drawing.Arial12;

            //Image bmp = ImageTools.DrawMatrixWithoutNormalisationGreenScale(norm);
            var bmp = ImageTools.DrawRGBMatrix(norm1, norm2, norm3); // RGB
            bmp.Mutate(g =>
            {
                int halfWidth = scale / 2;

                // draw plot outline
                g.DrawLine(new Pen(Color.White, 1), 0, scale, halfWidth, scale - height);
                g.DrawLine(new Pen(Color.White, 1), scale, scale, halfWidth, scale - height);

                //label vertices
                g.DrawText(keys[0], stringFont, Color.Wheat, new PointF(halfWidth - 15, scale - height - 15));
                g.DrawText(keys[1], stringFont, Color.Wheat, new PointF(0, scale - 20));
                g.DrawText(keys[2], stringFont, Color.Wheat, new PointF(scale - 40, scale - 20));

                // draw label
                string label = label1 + label2;
                g.DrawText(label, stringFont, Color.Wheat, new PointF(30, scale - height - 40));
            });
            return bmp;
        }
    }
}
