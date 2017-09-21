using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class Maths
    {

        public static T Min<T>(params T[] vals)
        {
            return vals.Min();
        }
        public static T Max<T>(params T[] vals)
        {
            return vals.Max();
        }

    }
}
