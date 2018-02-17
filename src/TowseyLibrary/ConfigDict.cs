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

            object value;
            if (this.TryGetValue(keyString, out value))
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

            object value;
            if (this.TryGetValue(keyString, out value))
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

            object value;
            if (this.TryGetValue(keyString, out value))
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

            var doublearray = key as double[];

            if (doublearray != null)
            {
                return doublearray;
            }

            var doublelist = key as List<double>;

            if (doublelist != null)
            {
                return doublelist;
            }

            var result = keyString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => double.Parse(i));
            return result;
        }
    }
}
