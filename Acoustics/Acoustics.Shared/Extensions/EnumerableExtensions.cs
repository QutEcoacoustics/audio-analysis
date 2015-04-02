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

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var index = 0;
            foreach (var item in source)
            {
                action(item, index);
                index++;
            }
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

        public static IEnumerable<T[]> Windowed<T>(this IEnumerable<T> list, int windowSize)
        {
            Contract.Requires(windowSize >= 0);

            var array = new T[windowSize];
            int r = windowSize - 1, i = 0;
            using (var e = list.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    array[i] = e.Current;
                    i = (i + 1) % windowSize;
                    if (r == 0)
                    {
                        var output = new T[windowSize];
                        for (var ii = 0; ii < windowSize; ii++)
                        {
                            output[ii] = array[(i + ii) % windowSize];
                        }

                        yield return output;
                    }
                    else
                    {
                        r--;
                    }
                }
            }
        }

        public static IEnumerable<T[]> WindowedOrDefault<T>(
            this IEnumerable<T> list,
            int windowSize,
            T defaultValue = default(T))
        {
            Contract.Requires(windowSize >= 0);
            Contract.Requires(list != null);

            if (list == null)
            {
                throw new ArgumentNullException("list", "list should not be null");
            }

            var array = new T[windowSize];

            for (int a = 0; a < array.Length; a++)
            {
                array[a] = defaultValue;
            }

            int i = 0;
            bool enumeratorEmpty = true;
            using (var e = list.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    enumeratorEmpty = false;
                    array[i] = e.Current;
                    i = (i + 1) % windowSize;
                    var output = new T[windowSize];
                    for (var ii = 0; ii < windowSize; ii++)
                    {
                        output[ii] = array[(i + ii) % windowSize];
                    }

                    yield return output;
                }

                if (!enumeratorEmpty)
                {
                    for (var w = 1; w < windowSize; w++)
                    {
                        var lastOutput = new T[windowSize];
                        
                        for (var ii = 0; ii < windowSize; ii++)
                        {
                            if (ii < (windowSize - w))
                            {
                                lastOutput[ii] = array[(ii + windowSize + w + i) % windowSize];
                            }
                            else
                            {
                                lastOutput[ii] = defaultValue;
                            }
                        }

                        yield return lastOutput;
                    }
                }
            }
        }

        #endregion
    }
}