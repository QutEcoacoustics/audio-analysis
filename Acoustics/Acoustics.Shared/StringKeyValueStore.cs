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

        /// <summary>
        /// Initializes a new instance of the <see cref="StringKeyValueStore"/> class.
        /// </summary>
        public StringKeyValueStore()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringKeyValueStore"/> class.
        /// </summary>
        /// <param name="existing">
        /// The existing.
        /// </param>
        public StringKeyValueStore(Dictionary<string, string> existing)
        {
            foreach (var item in existing)
            {
                this.Add(item.Key, item.Value);
            }
        }

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

        #region Save and Load

        public static void SaveToCsv(FileInfo input, FileInfo output)
        {
            // read in entire file to get all property names
            var inputContents = File.ReadAllLines(input.FullName);
            var inputSplit = inputContents.Select(l => l.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries)).ToList();

            var modified = inputSplit.Skip(1).Select(
                l =>
                {
                    var skvs = new StringKeyValueStore();
                    skvs.LoadFromTwoStrings(l.Skip(9).First(), l.Skip(10).First(), ":");

                    return new { First9 = string.Join(", ", l.Take(9).Select(i => i)), Skvs = skvs };
                }).ToList();

            // headers
            var headers = inputSplit.First().Take(9);

            // string key value store headers
            var skvsHeaders = modified.SelectMany(i => i.Skvs.Keys).Distinct().ToList();

            var sb = new StringBuilder(string.Join(", ", headers.Select(i => i.Replace(',', ' '))) + ", " + string.Join(", ", skvsHeaders.Select(i => i.Replace(',', ' '))) + Environment.NewLine);
            foreach (var item in modified)
            {
                sb.Append(item.First9 + ", ");

                foreach (var key in skvsHeaders)
                {
                    if (item.Skvs.ContainsKey(key))
                    {
                        sb.Append(item.Skvs[key].Replace(',', ' '));
                    }

                    sb.Append(", ");
                }

                sb.AppendLine();
            }

            File.WriteAllText(output.FullName, sb.ToString());
            /*
          
SELECT TOP 1000 [PageEventId]
      ,[UserId]
      ,[DateTimeOccurred]
      ,[DateTimeOccurredUtc]
      ,[DateTimeCreated]
      ,[DateTimeCreatedUtc]
      ,[RequestUrl]
      ,[PageName]
      ,[EventName]
      ,[PropertyNamesStored]
      ,[PropertyValuesStored]
  FROM [QutSensors].[Ecosounds].[PageEvents]
  where userid = '66E4A431-41E1-430F-8E29-FBD22DEF21BB'
  order by pageeventid desc
      
             
             
SELECT TOP 1000 b.boundid,b.audioreadingid,l.text,b.starttimeoffsetMs, b.endtimeoffsetms,lb.createddate,l.createddate,b.createddate,u.username,u.userid,
	b.endtimeoffsetms - b.starttimeoffsetMs as tagdurationms, 
	cast(datepart(year, lb.createddate) as varchar(100))+'/'+
       cast(datepart(month, lb.createddate) as varchar(100))+'/'+
       cast(datepart(day, lb.createddate) as varchar(100)) as CreatedDate
       ,cast(datepart(hh,lb.createddate) as varchar(100))+':'+
       cast(datepart(minute, lb.createddate) as varchar(100))+':'+
       cast(datepart(second, lb.createddate) as varchar(100)) as CreatedTime
       
	   ,cast(datepart(year,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100))+'/'+
       cast(datepart(month,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100))+'/'+
       cast(datepart(day,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100)) as StartDate
       ,cast(datepart(hh,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100))+':'+
       cast(datepart(minute,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100))+':'+
       cast(datepart(second,dateadd(millisecond, b.starttimeoffsetMs, ar.Time)) as varchar(100)) as StartTime
       ,cast(datepart(year,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100))+'/'+
       cast(datepart(month,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100))+'/'+
       cast(datepart(day,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100)) as EndDate
       ,cast(datepart(hh,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100))+':'+
       cast(datepart(minute,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100))+':'+
       cast(datepart(second,dateadd(millisecond, b.endtimeoffsetms, ar.Time)) as varchar(100)) as EndTime
  FROM ecosounds.labelledbounds lb
  inner join aspnet_users u on lb.createdbyuserid = u.userid
  inner join ecosounds.Labels l on lb.labelid = l.labelid
  inner join ecosounds.bounds b on lb.boundid = b.boundid
  inner join audioreadings ar on b.audioreadingid = ar.audioreadingid
  where b.createdtagtypecontext = 'SamfordBirdWalkDawnQuest'
  order by lb.createdbyuserid, b.starttimeoffsetMs
  
             
             
             
             */

        }

        public void LoadFromQueryString(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                // remove everything before (and including) a question mark
                if (query.Contains("?"))
                {
                    query = query.Remove(0, query.IndexOf("?", System.StringComparison.Ordinal));
                }

                var items = query.Split('&').Select(i => i.Split('='));

                foreach (var item in items)
                {
                    if (item.Length != 2)
                    {
                        throw new ArgumentException(
                            string.Format("Found an invalid item '{0}' in {1}.", string.Join("=", item), query));
                    }

                    this.Add(item[0], item[1]);
                }
            }
        }

        public string SaveToQueryString()
        {
            var sb = new StringBuilder();

            foreach (var item in this)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }

                sb.AppendFormat("{0}={1}", item.Key, item.Value);
            }

            return sb.ToString();
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
                if (!string.IsNullOrWhiteSpace(currentName) && currentStartIndex >= 0 && currentLength >= 0)
                {
                    this.Add(currentName, propertyValues.Substring(currentStartIndex, currentLength));
                    currentName = null;
                    currentStartIndex = -1;
                    currentLength = -1;
                }

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

        #endregion
    }
}
