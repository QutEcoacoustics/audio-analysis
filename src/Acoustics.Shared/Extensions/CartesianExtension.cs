// <copyright file="CartesianExtension.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq.Extensions;

    /// <summary>
    /// Extensions to the MoreLinq.Cartesian function.
    /// </summary>
    public static class CartesianExtension
    {
        /// <summary>
        /// Returns the Cartesian product of multiple sequences by combining each element of every set with every other element
        /// and applying the user-defined projection to the pair.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="enumerables"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="enumerables">The sequence of sequences of element.s</param>
        /// <param name="resultSelector">A projection function that combines elements from both sequences.</param>
        /// <returns>A sequence representing the Cartesian product of the source sequences.</returns>
        public static IEnumerable<TResult> MultiCartesian<TSource, TResult>(
            this IEnumerable<IEnumerable<TSource>> enumerables,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            if (enumerables == null)
            {
                throw new ArgumentNullException(nameof(enumerables));
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }

            var enumerators = enumerables
                .Select(e => e?.GetEnumerator() ?? throw new ArgumentException("One of the enumerables is null"))
                .Pipe(e => e.MoveNext())
                .ToArray();

            do
            {
                yield return resultSelector(enumerators.Select(e => e.Current));
            } while (MoveNext());

            foreach (var enumerator in enumerators)
            {
                enumerator.Dispose();
            }

            bool MoveNext()
            {
                for (var i = enumerators.Length - 1; i >= 0; i--)
                {
                    if (enumerators[i].MoveNext())
                    {
                        return true;
                    }

                    if (i == 0)
                    {
                        continue;
                    }

                    enumerators[i].Reset();
                    enumerators[i].MoveNext();
                }

                return false;
            }
        }
    }
}
