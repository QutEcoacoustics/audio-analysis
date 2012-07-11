using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqStatistics
{
    using System.Collections;
    using System.Linq;

    public static partial class EnumerableStats
    {



        public static double EuclideanDistance(this IEnumerable<double> sourceA, IEnumerable<double> sourceB)
        {
            var dists = sourceA.Zip(sourceB, (d1, d2) => Math.Pow((d2 - d1), 2) );

            var sum = dists.Sum();

            return Math.Sqrt(sum);
        }

        public static double EuclideanDistance<TSource>(this IEnumerable<TSource> sourceA, IEnumerable<TSource> sourceB, Func<TSource, double> selector)
        {
            var a = sourceA.Select(selector);
            var b = sourceB.Select(selector);
            return a.EuclideanDistance(b);
        }

    }
}
