using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace TowseyLib
{
    public class Configuration
    {
        private string fName;
        Hashtable table;

        public Configuration()
        {
            this.table = new Hashtable();
        }

        public Configuration(string fName)
        {
            this.fName = fName;
            table = FileTools.ReadPropertiesFile(fName);
        }

        public bool ContainsKey(string key)
        {
            return table.ContainsKey(key);
        }

        public string GetString(string key)
        {
            if (table.ContainsKey(key))
				return table[key].ToString();
			return null;
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
        }//end getBoolean



    }  // end of class Configuration
}
