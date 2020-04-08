// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ArrayExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace System
{
    using System.Diagnostics;

    public static class ArrayExtensions
    {
        /// <summary>
        /// A helper method designed to fill an array with the specified values.
        /// Modifies the array in place, return value is only for fluent method calling.
        /// Fast is a bit of a misnomer - this operation is only faster after about a million elements!.
        /// </summary>
        /// <remarks>
        /// https://github.com/mykohsu/Extensions/blob/master/ArrayExtensions.cs
        /// Inspired from several Stack Overflow discussions and an implementation by David Walker at http://coding.grax.com/2011/11/initialize-array-to-value-in-c-very.html.
        /// </remarks>
        /// <typeparam name="T">They type of the array.</typeparam>
        /// <param name="destinationArray">The array being filled.</param>
        /// <param name="value">The value[s] to insert into the array.</param>
        public static T[] FastFill<T>(this T[] destinationArray, params T[] value)
        {
            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }

            if (value.Length > destinationArray.Length)
            {
                throw new ArgumentException("Length of value array must not be more than length of destination");
            }

            // set the initial array value
            Array.Copy(value, destinationArray, value.Length);

            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = value.Length; copyLength < arrayToFillHalfLength; copyLength <<= 1)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);

            return destinationArray;
        }

        /// <summary>
        /// A helper method designed to fill an array with the specified values.
        /// Modifies the array in place, return value is only for fluent method calling.
        /// </summary>
        /// <remarks>
        /// https://github.com/mykohsu/Extensions/blob/master/ArrayExtensions.cs
        /// Inspired from several Stack Overflow discussions and an implementation by David Walker at http://coding.grax.com/2011/11/initialize-array-to-value-in-c-very.html.
        /// </remarks>
        /// <typeparam name="T">They type of the array.</typeparam>
        /// <param name="destinationArray">The array being filled.</param>
        /// <param name="value">The value[s] to insert into the array.</param>
        public static T[] Fill<T>(this T[] destinationArray, T value)
        {
            if (destinationArray == null)
            {
                throw new ArgumentNullException(nameof(destinationArray));
            }

            for (int i = 0; i < destinationArray.Length; i++)
            {
                destinationArray[i] = value;
            }

            return destinationArray;
        }

        /// <summary>
        /// Debug function used to print an array to console (Debug.WriteLine()).
        /// </summary>
        /// <typeparam name="T">The type of the input array.</typeparam>
        /// <param name="array">The array to print.</param>
        /// <returns>The same array that was input.</returns>
        public static T[] Print<T>(this T[] array)
        {
#if DEBUG
            foreach (T foo in array)
            {
                if (foo is ValueType || foo != null)
                {
                    Debug.WriteLine(foo.ToString());
                }
                else
                {
                    Debug.WriteLine('\u0000');
                }
            }
#endif

            return array;
        }

        /// <summary>
        /// Compares two arrays, matching each element in order using the default Equals method for the array type T.
        /// </summary>
        /// <typeparam name="T">The common type of each array.</typeparam>
        /// <param name="arr1">The first array to compare.</param>
        /// <param name="arr2">The second array to compare.</param>
        /// <returns>True: If each element matches; otherwise: False.</returns>
        public static bool Compare<T>(this T[] arr1, T[] arr2)
        {
            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            for (int i = 0; i < arr1.Length; i++)
            {
                T foo = arr1[i];
                T bar = arr2[i];

                if (!(foo is ValueType) && foo == null)
                {
                    if (bar == null)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (!foo.Equals(bar))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// retrieving the max value of a vector.
        /// </summary>
        public static double GetMaxValue(this double[] data)
        {
            double max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                }
            }

            return max;
        }

        /// <summary>
        /// retrieving the min value of a vector.
        /// </summary>
        public static double GetMinValue(this double[] data)
        {
            double min = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }
            }

            return min;
        }
    }
}