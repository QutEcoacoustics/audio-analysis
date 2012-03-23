namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Key value store is a wrapper around a dictionary with ways to convert values to the desired type.
    /// It can also load values from an app.config file.
    /// </summary>
    public class KeyValueStore : Dictionary<string, object>
    {
        private static IEnumerable<T> GetValues<T>(object value)
        {
            if (value == null)
            {
                return new List<T>();
            }

            var array = value as T[];
            if (array != null)
            {
                return array;
            }

            var list = value as List<T>;
            if (list != null)
            {
                return list;
            }

            var itemCheck = value is T;
            if (itemCheck)
            {
                return new List<T> { (T)value };
            }

            return new List<T>();
        }

        public string GetAppSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == key))
            {
                var valueString = ConfigurationManager.AppSettings[key];

                if (!string.IsNullOrEmpty(valueString))
                {
                    return valueString;
                }
            }

            return string.Empty;
        }

        public object GetDictionaryEntry(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (this.ContainsKey(key))
            {
                var value = this[key];

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        public string GetString(string key)
        {
            var valueString = this.GetDictionaryEntry(key) as string;
            if (!string.IsNullOrEmpty(valueString))
            {
                return valueString;
            }

            valueString = this.GetAppSetting(key);
            if (!string.IsNullOrEmpty(valueString))
            {
                return valueString;
            }

            return string.Empty;
        }

        public IEnumerable<double> GetDoubles(string key)
        {
            // dictionary
            var value = this.GetDictionaryEntry(key);

            var doubleEnumeration = GetValues<double>(value);
            if (doubleEnumeration != null && doubleEnumeration.Count() > 0)
            {
                return doubleEnumeration;
            }

            // list of doubles
            var valueStringList = value as string;
            if (!string.IsNullOrEmpty(valueStringList) && valueStringList.Contains(","))
            {
                return valueStringList
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => double.Parse(i, NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            double result1;
            if (double.TryParse(valueStringList, NumberStyles.Float, CultureInfo.InvariantCulture, out result1))
            {
                return new List<double> { result1 };
            }

            // app settings
            valueStringList = this.GetAppSetting(key);
            if (!string.IsNullOrEmpty(valueStringList) && valueStringList.Contains(","))
            {
                return valueStringList
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => double.Parse(i, NumberStyles.Float, CultureInfo.InvariantCulture));
            }

            double result2;
            if (double.TryParse(valueStringList, NumberStyles.Float, CultureInfo.InvariantCulture, out result2))
            {
                return new List<double> { result2 };
            }

            return new List<double>();
        }
    }
}
