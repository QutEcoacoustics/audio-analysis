// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="MQUTeR">
//   Originally taken from http://www.codeproject.com/Articles/43123/Sammon-Projection
//   By Günther M. FOIDL, 20 Oct 2009
// </copyright>
// <summary>
//   Defines the Helper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace gfoidl.SammonProjection
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    internal static class Helper
    {
        private static Random _rnd = new Random();

        /// <summary>
        /// The manhatten distance.
        /// </summary>
        /// <param name="vec1">
        /// The vec 1.
        /// </param>
        /// <param name="vec2">
        /// The vec 2.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        internal static double ManhattenDistance(double[] vec1, double[] vec2)
        {
            double distance = 0;

            for (int i = 0; i < vec1.Length; i++)
            {
                distance += Math.Abs(vec1[i] - vec2[i]);
            }

            return distance;
        }

        /// <summary>
        /// The fisher yates shuffle.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        internal static void FisherYatesShuffle<T>(this T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                // Pick random positoin:
                int pos = _rnd.Next(i + 1);

                // Swap:
                T tmp = array[i];
                array[i] = array[pos];
                array[pos] = tmp;
            }
        }
    }
}