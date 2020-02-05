// <copyright file="ConfigDict.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;

    public class ConfigDict : Dictionary<string, object>
    {
        public double GetDouble(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return double.NaN;
            }

            if (this.TryGetValue(keyString, out var value))
            {
                return (double)value;
            }
            else
            {
                return double.NaN;
            }
        }

        public int GetInt(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return int.MaxValue;
            }

            if (this.TryGetValue(keyString, out var value))
            {
                return (int)value;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public bool GetBool(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return false;
            }

            if (this.TryGetValue(keyString, out var value))
            {
                return (bool)value;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<double> GetDoubles(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return new List<double>();
            }

            var key = this[keyString];


            if (key is double[] doublearray)
            {
                return doublearray;
            }


            if (key is List<double> doublelist)
            {
                return doublelist;
            }

            var result = keyString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i));
            return result;
        }
    }
}
