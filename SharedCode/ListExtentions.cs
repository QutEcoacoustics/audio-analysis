


using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class IEnumerableHelpers
    {

        public static bool IsEmpty<T>(this ICollection<T> aList)
        {
            return aList.Count == 0;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> aList)
        {
            IEnumerator<T> enumerator = aList.GetEnumerator();
            bool retVal = enumerator.MoveNext();
            return retVal;
        }
    }

}