namespace Acoustics.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// </summary>
    [Serializable]
    public class StringKeyValueStore : Dictionary<string, string>
    {
        protected static string[] DateTimeFormatStrings = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.fffzzz", 
                "yyyy-MM-ddTHH:mm:ss.fffzz", 
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss"
            };

        protected static DateTimeStyles DateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

        protected static string[] TimeSpanFormatStrings = new[]
            {
                "ddd.hh:mm:ss.fff", "hh:mm:ss.fff", "ddd.hh:mm:ss", "hh:mm:ss"
            };

        public static Type GuessType(string value)
        {
            Guid guidResult;
            if (Guid.TryParse(value, out guidResult))
            {
                return typeof(Guid);
            }

            DateTime dateTimeResult;
            if (DateTime.TryParseExact(value, DateTimeFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles, out dateTimeResult))
            {
                return typeof(DateTime);
            }

            TimeSpan timeSpanResult;
            if (TimeSpan.TryParseExact(value, TimeSpanFormatStrings, CultureInfo.InvariantCulture, TimeSpanStyles.None, out timeSpanResult))
            {
                return typeof(TimeSpan);
            }

            int intResult;
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out intResult))
            {
                return typeof(int);
            }

            long longResult;
            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out longResult))
            {
                return typeof(long);
            }

            decimal decimalResult;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalResult))
            {
                return typeof(decimal);
            }

            bool boolResult;
            if (bool.TryParse(value, out boolResult))
            {
                return typeof(bool);
            }

            return typeof(string);
        }

        /// <summary>
        /// Get the string representation of a value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The String representation.
        /// </returns>
        public static string ConvertToString(object value)
        {
            if (ReferenceEquals(value.GetType(), typeof(bool)))
            {
                return ((bool)value) ? "true" : "false";
            }

            if (ReferenceEquals(value.GetType(), typeof(decimal)))
            {
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(int)))
            {
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(long)))
            {
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(double)))
            {
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(float)))
            {
                return ((float)value).ToString(CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(string)))
            {
                return value.ToString();
            }

            if (ReferenceEquals(value.GetType(), typeof(DateTime)))
            {
                return ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(TimeSpan)))
            {
                return ((TimeSpan)value).ToString("ddd.hh:mm:ss.fff", CultureInfo.InvariantCulture);
            }

            if (ReferenceEquals(value.GetType(), typeof(Guid)))
            {
                return ((Guid)value).ToString();
            }

            return value.ToString();
        }

        public string GetValueAsString(string key)
        {
            if (!this.ContainsKey(key))
            {
                throw new ArgumentException("Key '" + key + "' was not found.", "key");
            }

            return this[key];
        }

        public IEnumerable<string> GetValueAsStrings(string key, params string[] separators)
        {
            if (separators == null || separators.All(s => s == null))
            {
                throw new ArgumentException("Provide a valid separator.", "separators");
            }

            var value = this.GetValueAsString(key);

            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                result.Add(value);
            }
            else
            {
                result.AddRange(value.Split(separators, StringSplitOptions.None));
            }

            return result;
        }

        public bool GetValueAsBool(string key)
        {
            var value = this.GetValueAsString(key);
            bool result;
            if (bool.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "bool");
        }

        public int GetValueAsInt(string key)
        {
            var value = this.GetValueAsString(key);
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "int");
        }

        public double GetValueAsDouble(string key)
        {
            var value = this.GetValueAsString(key);
            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "double");
        }

        public long GetValueAsLong(string key)
        {
            var value = this.GetValueAsString(key);
            long result;
            if (long.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "long");
        }

        public DirectoryInfo GetValueAsDirectory(string key)
        {
            var value = this.GetValueAsString(key);

            return new DirectoryInfo(value);
        }

        public IEnumerable<DirectoryInfo> GetValueAsDirectories(string key, params string[] separators)
        {
            var strings = this.GetValueAsStrings(key, separators);
            return strings.Select(s => new DirectoryInfo(s));
        }

        public FileInfo GetValueAsFile(string key)
        {
            var value = this.GetValueAsString(key);

            return new FileInfo(value);
        }

        public IEnumerable<FileInfo> GetValueAsFiles(string key, params string[] separators)
        {
            var strings = this.GetValueAsStrings(key, separators);
            return strings.Select(s => new FileInfo(s));
        }

        public DateTime GetValueAsDateTime(string key)
        {
            var value = this.GetValueAsString(key);
            DateTime result;
            if (DateTime.TryParseExact(value, DateTimeFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "DateTime");
        }

        public TimeSpan GetValueAsTimeSpan(string key)
        {
            var value = this.GetValueAsString(key);
            TimeSpan result;
            if (TimeSpan.TryParseExact(value, TimeSpanFormatStrings, CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "Timespan");
        }

        public Guid GetValueAsGuid(string key)
        {
            var value = this.GetValueAsString(key);
            Guid result;
            if (Guid.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "Guid");
        }

        public decimal GetValueAsDecimal(string key)
        {
            var value = this.GetValueAsString(key);
            decimal result;
            if (decimal.TryParse(value, out result))
            {
                return result;
            }

            throw this.ConvertFailed(key, value, "decimal");
        }

        public void LoadFromTwoStrings(string propertyNames, string propertyValues, string separator)
        {
            if (string.IsNullOrWhiteSpace(propertyNames))
            {
                propertyNames = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(propertyValues))
            {
                propertyValues = string.Empty;
            }

            var properties = propertyNames.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            string currentName = null;
            int currentStartIndex = -1;
            int currentLength = -1;

            // format: <name><sep><startindex><sep><length>
            // separated by <sep>

            for (var index = 0; index < properties.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(currentName))
                {
                    currentName = properties[index];
                }
                else if (currentStartIndex < 0)
                {
                    currentStartIndex = int.Parse(properties[index]);
                }
                else if (currentLength < 0)
                {
                    currentLength = int.Parse(properties[index]);
                }
                else
                {
                    this.Add(currentName, propertyValues.Substring(currentStartIndex, currentLength));
                    currentName = null;
                    currentStartIndex = -1;
                    currentLength = -1;
                }
            }
        }

        public KeyValuePair<string, string> SaveToTwoStrings(string separator)
        {
            var sbPropertyNames = new StringBuilder();
            var sbPropertyValues = new StringBuilder();

            foreach (var kvp in this)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key) && kvp.Key.Contains(separator))
                {
                    throw new InvalidOperationException("A key contains the separator, which will make it impossible to deserialise. Key: '" + kvp.Key + "' separator: '" + separator + "'.");
                }

                if (sbPropertyNames.Length > 0)
                {
                    sbPropertyNames.Append(separator);
                }

                // format: <name><sep><startindex><sep><length>
                // separated by <sep>
                sbPropertyNames.AppendFormat("{0}{1}{2}{3}{4}", kvp.Key, separator, sbPropertyValues.Length, separator, kvp.Value.Length);
                sbPropertyValues.Append(kvp.Value);
            }

            return new KeyValuePair<string, string>(sbPropertyNames.ToString(), sbPropertyValues.ToString());
        }

        public void LoadFromSimple(FileInfo file, string[] separators)
        {
            this.LoadFromSimple(File.ReadAllLines(file.FullName), separators);
        }

        public void LoadFromSimple(string lines, string[] lineSeparators, string[] separators)
        {
            var splitLines = lines.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);

            this.LoadFromSimple(splitLines, separators);
        }

        public void LoadFromSimple(IEnumerable<string> lines, string[] separators)
        {
            foreach (var line in lines)
            {
                var items = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length != 2)
                {
                    throw new ArgumentException(
                        string.Format("Found an invalid line when splitting using {0} as separators: '{1}'", string.Join(", ", "'" + separators + "'"), line));
                }

                this.Add(items[0], items[1]);
            }
        }

        public IEnumerable<string> SaveToSimple(string separator)
        {
            foreach (var item in this)
            {
                if (!string.IsNullOrWhiteSpace(item.Key) && item.Key.Contains(separator))
                {
                    throw new InvalidOperationException("A key contains the separator, which will make it impossible to deserialise. Key: '" + item.Key + "' separator: '" + separator + "'.");
                }

                if (!string.IsNullOrWhiteSpace(item.Value) && item.Value.Contains(separator))
                {
                    throw new InvalidOperationException("A value contains the separator, which will make it impossible to deserialise. Value: '" + item.Value + "' separator: '" + separator + "'.");
                }

                yield return string.Format("{0}{1}{2}", item.Key, separator, item.Value);
            }
        }

        public void LoadFromAppConfig(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            // Create a filemap refering the config file.
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = file.FullName };

            // Retrieve the config file.
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            this.Load(configuration.AppSettings.Settings);
        }

        public void LoadFromAppConfig()
        {
            var items = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
            this.Load(items);
        }

        private void Load(KeyValueConfigurationCollection items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            foreach (KeyValueConfigurationElement item in items)
            {
                this.Add(item.Key, item.Value);
            }
        }

        private Exception ConvertFailed(string key, string value, string type)
        {
            return new InvalidOperationException(
                string.Format("Found key '{0}' but could not convert its value '{1}' to a {2}.", key, value, type));
        }
    }
}
