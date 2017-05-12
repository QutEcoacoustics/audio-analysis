// <copyright file="DictionaryExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DictionaryExtensions
    {

        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        public static string ToDictDebugString(this IDictionary dictionary)
        {
            var items = new List<string>(dictionary.Count);
            foreach (DictionaryEntry entry in dictionary)
            {
                items.Add(entry.Key.ToString() + "=" + entry.Value.ToString());
            }

            return "{" + string.Join(",",  items) + "}";
        }
    }
}
