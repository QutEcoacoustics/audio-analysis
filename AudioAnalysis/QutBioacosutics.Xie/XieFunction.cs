using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutBioacosutics.Xie
{
    public static class XieFunction
    {
        /// <summary>
        /// Adding Multiple Elements to a List on One Line
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddMany<T>(this List<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }
        }
    }
}
