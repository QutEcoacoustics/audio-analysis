// <copyright file="DictionaryTools.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DictionaryTools
    {
        public static List<string> FilterKeysInDictionary(Dictionary<string, string> dict, string filter)
        {
            List<string> keys = dict.Keys.ToList();

            var list = new List<string>();
            foreach (string key in keys)
            {
                if (key.StartsWith(filter))
                {
                    list.Add(key);
                }
            }

            return list;
        }

        public static Dictionary<string, int> WordsHisto(List<string> list)
        {
            var ht = new Dictionary<string, int>();
            foreach (var item in list)
            {
                if (!ht.ContainsKey(item))
                {
                    ht.Add(item, 1);
                }
                else
                {
                    ht[item] = ht[item] + 1;
                }
            }

            return ht;
        }
    }
}
