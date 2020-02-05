// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using JetBrains.Annotations;

    public static class EnumerableExtensions
    {
        [ContractAnnotation("items:null => true")]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
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
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
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
            TwoDimensionalArray dimensionality = TwoDimensionalArray.None)
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
                var value = dimensionality == TwoDimensionalArray.None ? new T[itemCount, current.Length] : new T[current.Length, itemCount];
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
                                    case TwoDimensionalArray.None:
                                        result[key][i, j] = line[j];
                                        break;
                                    case TwoDimensionalArray.Transpose:
                                        result[key][j, i] = line[j];
                                        break;
                                    case TwoDimensionalArray.Rotate90ClockWise:
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

        public static TBase[] FromTwoDimensionalArray<TBase, T>(
            this Dictionary<string, T[,]> items,
            Dictionary<string, Action<TBase, T[]>> setters,
            TwoDimensionalArray dimensionality = TwoDimensionalArray.None)
            where TBase : new()
        {
            // This code is covered by unit tests Acoustics.Test - change the unit tests before you change the class!
            Contract.Requires(items != null);
            Contract.Requires(setters != null);

            // assume all matrices contain the same number of elements
            int major = dimensionality == TwoDimensionalArray.None ? 0 : 1;
            int minor = major == 1 ? 0 : 1;
            var itemCount = items.First().Value.GetLength(major);

            int keyCount = setters.Keys.Count;
            var results = new TBase[itemCount];

            // initialize all values
            for (int index = 0; index < results.Length; index++)
            {
                results[index] = new TBase();
            }

            Parallel.ForEach(
                setters,
                (kvp, state, index) =>
                    {
                        var key = kvp.Key;
                        var setter = kvp.Value;

                        var matrix = items[key];
                        for (int i = 0; i < itemCount; i++)
                        {
                            var lineLength = matrix.GetLength(minor);
                            T[] line = new T[lineLength];

                            for (int j = 0; j < lineLength; j++)
                            {
                                switch (dimensionality)
                                {
                                    case TwoDimensionalArray.None:
                                        line[j] = matrix[i, j];
                                        break;
                                    case TwoDimensionalArray.Transpose:
                                        line[j] = matrix[j, i];
                                        break;
                                    case TwoDimensionalArray.Rotate90ClockWise:
                                        line[j] = matrix[lineLength - 1 - j, i];
                                        break;
                                    default:
                                        throw new NotImplementedException("Dimensionality not supported");
                                }
                            }

                            // set the line (row or column) to the instance
                            setter(results[i], line);
                        }
                    });

            return results;
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
            T defaultValue = default)
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
                            if (ii < windowSize - w)
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

        public static bool All(this IEnumerable<bool> sequence)
        {
            foreach (var item in sequence)
            {
                if (!item)
                {
                    return false;
                }
            }

            return true;
        }

        public static string Join(this IEnumerable items, string delimiter = " ") => Join(items.Cast<object>(), delimiter);

        public static string Join<T>(this IEnumerable<T> items, string delimiter = " ")
        {
            var result = new StringBuilder();
            foreach (var item in items)
            {
                result.Append(item);
                result.Append(delimiter);
            }

            // return one delimiter length less because we always add a delimiter on the end
            return result.ToString(0, result.Length - delimiter.Length);
        }
    }
}
