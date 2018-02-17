// <copyright file="GaussianTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class GaussianTools
    {
        /// <summary>
        /// returns a 2-D Gaussian filter with side corresponding to 2 sigma.
        /// This formula derived from code of LeCun.
        /// See python code at the bottom of the class LocalContrastNormalisation.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static double[,] Gaussian_filter(int side)
        {
            double sigma = 2.0;
            int halfSide = side / 2;
            double[,] x = new double[side, side];
            double sum = 0.0;

            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    x[i, j] = Gauss(i - halfSide, j - halfSide, sigma);
                    sum += x[i, j];
                }
            }

            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    x[i, j] /= sum;
                }
            }

            return x;
        }

        /// <summary>
        /// returns a gaussian coefficient for point x,y distance from the centre of the distribution.
        /// This formula derived from code of LeCun.
        /// See python code at the bottom of the class LocalContrastNormalisation.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double Gauss(int x, int y, double sigma)
        {
            double Z = 2 * Math.PI * sigma * sigma;
            double G = 1 / Z * Math.Pow(Math.E, -((x * x) + (y * y)) / (2 * sigma * sigma));
            return G;
        }
    }
}
