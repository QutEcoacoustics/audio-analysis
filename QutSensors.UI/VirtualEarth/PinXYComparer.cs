// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PinXYComparer.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Comparers two pins and returns the sort postion.
//   Sorts by y then by x
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace SoulSolutions.ClusterArticle
{
    using System.Collections.Generic;

    /// <summary>
    /// Comparers two pins and returns the sort postion.
    /// Sorts by y then by x.
    /// </summary>
    /// <typeparam name="T">
    /// Type of PinXYComparer.
    /// </typeparam>
    public class PinXYComparer<T> : IComparer<ClusteredPin<T>>
    {
        /// <summary>
        /// Compare two pins.
        /// </summary>
        /// <param name="x">First pin.</param>
        /// <param name="y">Second Pin.</param>
        /// <returns>int representing sort order of pins.</returns>
        int IComparer<ClusteredPin<T>>.Compare(ClusteredPin<T> x, ClusteredPin<T> y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }

            return y == null ? 1
                       : x.Loc.Lon > y.Loc.Lon ? 1
                             : x.Loc.Lon != y.Loc.Lon ? -1
                                   : x.Loc.Lat > y.Loc.Lat ? 1
                                         : x.Loc.Lat == y.Loc.Lat ? 0
                                               : -1;
        }
    }
}