using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    public class ResultItem
    {
        string key;
        object value;
        Dictionary<string, string> info;
        Type valueType;

        public ResultItem(string key, double? value, Dictionary<string, string> info)
        {
            this.key = key;
            this.value = value;
            this.info = info;
            this.valueType = typeof(double);
        }

        public ResultItem(string key, Int32? value, Dictionary<string, string> info)
        {
            this.key = key;
            this.value = value;
            this.info = info;
            this.valueType = typeof(Int32);
        }

        public ResultItem(string key, string value, Dictionary<string, string> info)
        {
            this.key = key;
            this.value = value;
            this.info = info;
            this.valueType = typeof(string);
        }


        public override string ToString()
        {
            if (value == null) return " null value";
            if (valueType == typeof(double)) return ((double)value).ToString("F3");
            if (valueType == typeof(Int32))  return ((Int32)value).ToString();
            else                             return value.ToString();
        }

        Dictionary<string, string> GetInfo()
        { return info; }

        public string GetName()
        { return key; }

        public Type GetValueType()
        { return valueType; }

        public object GetValue()
        { return value; }

    } //end ResultItem
}
