// <copyright file="DictionaryExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Linq;
    using Acoustics.Shared.Contracts;

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

        public static TValue FirstValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            Contract.Requires<ArgumentNullException>(dictionary != null, "dictionary was null");

            return dictionary.Values.First();
        }

        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> kvp,
            out TKey key,
            out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
