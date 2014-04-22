using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Systems
{
    public static class EnumerableExtensions
    {

        public static Tuple<int, int> SumAndCount(this List<int> items, Func<int, bool> predicate) 
        {
            int count = 0;
            int sum = 0;

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

    }
}
