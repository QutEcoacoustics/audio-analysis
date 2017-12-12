// <copyright file="MathExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

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
