// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace System

// ReSharper restore CheckNamespace
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// The object extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        [ContractAnnotation("obj:null => false; obj:notnull => true")]
        public static bool NotNull(this object obj) => obj != null;

        public static T[] AsArray<T>(this T item)
        {
            return new[] { item };
        }

        public static List<T> AsList<T>(this T item)
        {
            return new List<T> { item };
        }

        public static IEnumerable<T> Wrap<T>(this T item)
        {
            yield return item;
        }
    }
}