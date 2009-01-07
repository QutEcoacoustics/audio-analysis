using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace TowseyLib
{
    public class Configuration
    {	
        Dictionary<string, string> table;

        public Configuration()
        {
			table = new Dictionary<string, string>();
        }

        public Configuration(params string[] files)
        {
			if (files == null || files.Length == 0)
				throw new ArgumentNullException("files must be supplied and contain entries.");

			Source = files[files.Length - 1]; // Take last file as filename
			table = new Dictionary<string,string>();
			foreach (var file in files)
				foreach (var item in FileTools.ReadPropertiesFile(file))
					table[item.Key] = item.Value;
        }

		public string Source { get; set; }

		public string ResolvePath(string path)
		{
			if (path == null)
				return null;
			if (!Path.IsPathRooted(path))
			{
				if (Source == null)
					throw new InvalidOperationException("Configuration was not loaded from a file. Relative paths can not be resolved.");
				return Path.Combine(Path.GetDirectoryName(Source), path);
			}
			return path;
		}

        public bool ContainsKey(string key)
        {
            return table.ContainsKey(key);
        }

        public string GetString(string key)
        {
			string value;
			return table.TryGetValue(key, out value) ? value : null;
        }

		public string GetPath(string key)
		{
			return ResolvePath(GetString(key));
		}

        public int GetInt(string key)
        {
            if (!table.ContainsKey(key))
				return -Int32.MaxValue;

            string value = this.table[key].ToString();
            if (value == null)
				return -Int32.MaxValue;

            int int32;
			if (int.TryParse(value, out int32))
				return int32;

			System.Console.WriteLine("ERROR READING PROPERTIES FILE");
			System.Console.WriteLine("INVALID VALUE=" + value);
			return -Int32.MaxValue;
        }

		public int? GetIntNullable(string key)
		{
			if (!table.ContainsKey(key))
				return null;

			string value = this.table[key].ToString();
			if (value == null)
				return null;

			int int32;
			if (int.TryParse(value, out int32))
				return int32;

			System.Console.WriteLine("ERROR READING PROPERTIES FILE");
			System.Console.WriteLine("INVALID VALUE=" + value);
			return null;
		}

        public double GetDouble(string key)
        {
            if (!table.ContainsKey(key))
				return -Double.MaxValue;

            string value = table[key].ToString();
            if (value == null)
				return -Double.MaxValue;

            double d;
			if (double.TryParse(value, out d))
				return d;

			System.Console.WriteLine("ERROR READING PROPERTIES FILE");
			System.Console.WriteLine("INVALID VALUE=" + value);
			return -Double.MaxValue;
        }

		public double? GetDoubleNullable(string key)
		{
			if (!table.ContainsKey(key))
				return null;

			string value = table[key].ToString();
			if (value == null)
				return null;

			double d;
			if (double.TryParse(value, out d))
				return d;

			System.Console.WriteLine("ERROR READING PROPERTIES FILE");
			System.Console.WriteLine("INVALID VALUE=" + value);
			return null;
		}

        public bool GetBoolean(string key)
        {
            bool keyExists = this.table.ContainsKey(key);
            if (!keyExists) return false;
            bool b = false;
            string value = this.table[key].ToString();
            if (value == null) return b;
            try
            {
                b = Boolean.Parse(value);
            }
            catch (System.FormatException ex)
            {
                System.Console.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID VALUE=" + value);
                System.Console.WriteLine(ex);
                return false;
            }
            return b;
        } //end getBoolean

		#region Writing Methods
		public static void WriteValue(TextWriter writer, string key, object value)
		{
			writer.WriteLine(key + "=" + value.ToString());
		}

		public static void WriteArray(TextWriter writer, string keyPattern, object[] values)
		{
			for (int i = 0; i < values.Length; i++)
				writer.WriteLine(string.Format(keyPattern, i + 1) + "=" + values[i].ToString());
		}
		#endregion
	} // end of class Configuration
}