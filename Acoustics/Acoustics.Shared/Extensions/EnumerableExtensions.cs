// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;

    using Acoustics.Shared;

    public static class EnumerableExtensions
    {
        #region Public Methods and Operators

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static Tuple<int, int> SumAndCount(this List<int> items, Func<int, bool> predicate)
        {
            var count = 0;
            var sum = 0;

            foreach (var item in items)
            {
                var filtered = predicate(item);

                if (filtered)
                {
                    count++;
                    sum = sum + item;
                }
            }

            return Tuple.Create(count, sum);
        }

        public static Dictionary<string, T[,]> ToTwoDimensionalArray<T, TBase>(
            this IList<TBase> items,
            Dictionary<string, Func<TBase, T[]>> selectors,
            TwoDimensionalArray dimensionality = TwoDimensionalArray.RowMajor)
        {
            // This code is covered by unit tests Acoustics.Test - change the unit tests before you change the class!
            Contract.Requires(items != null);
            Contract.Requires(selectors != null);

            var itemCount = items.Count;
            int keyCount = selectors.Keys.Count;
            var result = new Dictionary<string, T[,]>(keyCount);
            foreach (var kvp in selectors)
            {
                var current = kvp.Value(items[0]);
                var value = dimensionality == TwoDimensionalArray.RowMajor ? new T[itemCount, current.Length] : new T[current.Length, itemCount];
                result.Add(kvp.Key, value);
            }


            Parallel.ForEach(
                selectors,
                (kvp, state, index) =>
                    {
                        var key = kvp.Key;
                        var selector = kvp.Value;

                        for (int i = 0; i < itemCount; i++)
                        {
                            T[] line = selector(items[i]);
                            var lineLength = line.Length;
                            for (int j = 0; j < lineLength; j++)
                            {
                                switch (dimensionality)
                                {
                                    case TwoDimensionalArray.RowMajor:
                                        result[key][i, j] = line[j];
                                        break;
                                    case TwoDimensionalArray.ColumnMajor:
                                        result[key][j, i] = line[j];
                                        break;
                                    case TwoDimensionalArray.ColumnMajorFlipped:
                                        result[key][lineLength - 1 - j, i] = line[j];
                                        break;
                                    default:
                                        throw new NotImplementedException("Dimensionality not supported");
                                }
                            }
                        }
                    });

            return result;
        }

        #endregion
    }
}