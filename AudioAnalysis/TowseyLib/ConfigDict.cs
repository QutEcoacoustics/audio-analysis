using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConfigDict : Dictionary<string, object>
    {

        public double GetDouble(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return Double.NaN;
            }

            object value;
            if (this.TryGetValue(keyString, out value)) return (double)value;
            else                                        return Double.NaN;
        }

        public int GetInt(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return Int32.MaxValue;
            }

            object value;
            if (this.TryGetValue(keyString, out value)) return (int)value;
            else return Int32.MaxValue;
        }


        public bool GetBool(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return false;
            }

            object value;
            if (this.TryGetValue(keyString, out value)) return (bool)value;
            else return false;
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
