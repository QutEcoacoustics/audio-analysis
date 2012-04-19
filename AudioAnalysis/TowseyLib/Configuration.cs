// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Configuration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Configuration files.
    /// </summary>
    public class Configuration
    {
        private Dictionary<string, string> table;

        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            table = new Dictionary<string, string>();
        }

        /// <summary>
        /// The configuration.
        /// </summary>
        /// <param name="files">
        /// The files.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Argument is null.
        /// </exception>
        public Configuration(params string[] files)
        {
            if (files == null || files.Length == 0)
            {
                throw new ArgumentNullException("files", "files must be supplied and contain entries.");
            }

            Source = files[files.Length - 1]; // Take last file as filename
            table = new Dictionary<string, string>();
            foreach (var file in files)
            {
                foreach (var item in FileTools.ReadPropertiesFile(file))
                {
                    table[item.Key] = item.Value;
                    ////if (item.Key.StartsWith("VERBOSITY")) Console.WriteLine("VERBOSITY = " + item.Value);
                }
            }
        }


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

        /// <summary>
        /// adds key-value pairs to a properties table.
        /// Removes existing pair if it has same key.
        /// </summary>
        /// <param name="key">
        /// key to add or replace.
        /// </param>
        /// <param name="value">
        /// Value to use.
        /// </param>
        public void SetPair(string key, string value)
        {
            if (table.ContainsKey(key)) table.Remove(key);
            table.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return table.ContainsKey(key);
        }

        public Dictionary<string, string> GetTable()
        {
            return table;
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
            return GetInt(key, this.table);
        }

        public int? GetIntNullable(string key)  
        {
            return GetIntNullable(key, this.table);
        }

        public double GetDouble(string key)
        {
            return GetDouble(key, this.table);
        }

        public double? GetDoubleNullable(string key)
        {
            return GetDoubleNullable(key, this.table);
        }

        public bool GetBoolean(string key)
        {
            return GetBoolean(key, this.table);
        } //end getBoolean




        //#####################################################################################################################################
        //STATIC methods for configuration using Dictionary class.

        /// <summary>
        /// THIS ONLY WORKS IF ONLY HAVE KV PAIRS IN CONFIG FILE.
        /// IF HAVE COMMENTS ETC USE
        /// Dictionary<string,string> dict = FileTools.ReadPropertiesFile(file))
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ReadKVPFile2Dictionary(string path)
        {
            Dictionary<string, string> dict = File.ReadAllLines(path).ToList().Select(s => s.Split('=')).ToDictionary(k => k[0], v => v[1]);
            return dict;
        } // end ReadKVPFile2Dictionary()



        public static bool GetBoolean(string key, Dictionary<string, string> dict)
        {
            if (! dict.ContainsKey(key)) return false;
            string value = dict[key].ToString();
            try
            {
                if (value == null) return false;
                else
                return Boolean.Parse(value);
            }
            catch (System.FormatException ex)
            {
                System.Console.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                System.Console.WriteLine(ex);
                return false;
            }
        } //end getBoolean()

        public static double GetDouble(string key, Dictionary<string, string> dict)
        {

            //if (Double.TryParse(str, out d))     dic.Add(key, str); // if done, then is a number
            
            if (! dict.ContainsKey(key)) return -Double.NaN;
            string value = dict[key].ToString();
            if (value == null) return -Double.NaN;
            try
            {
                double d;
                Double.TryParse(value, out d);
                return  d;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return -Double.NaN;
            }
        }

        public static double? GetDoubleNullable(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key)) return null;
            string value = dict[key].ToString();
            if (value == null) return null;
            try
            {
                double d;
                Double.TryParse(value, out d);
                return d;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return null;
            }
        }

        public static int GetInt(string key, Dictionary<string, string> dict)
        {
            string value = dict.TryGetValue(key, out value) ? value : null;

            //if (!table.ContainsKey(key)) return -Int32.MaxValue;

            //string value = this.table[key].ToString();
            if (value == null) return -Int32.MaxValue;

            try
            {
                int int32;
                if (int.TryParse(value, out int32)) return int32;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return Int32.MaxValue;
            }
            return Int32.MaxValue;
        }

        public static int? GetIntNullable(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key)) return null;

            string value = dict[key].ToString();
            if (value == null) return null;

            try
            {
                int int32;
                if (int.TryParse(value, out int32)) return int32;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                System.Console.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return null;
            }
            return null;
        }





        //#####################################################################################################################################





    } // end of class Configuration

    //#####################################################################################################################################

    /// <summary>
    /// NOTE: This is an extension class
    /// All its methods are extensions for the Configuraiton class.
    /// These methods can be called with unusual syntax!
    /// i.e. can call thus:- writer.WriteConfigPath(string basePath, string key, string value)
    /// where var writer is type TextWriter.
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static void WriteConfigValue(this TextWriter writer, string key, object value)
        {
            if (value == null)
            {
                Log.WriteLine("WriteConfigValue() WARNING!!!! NULL VALUE for KEY=" + key);
                return;
            }
            writer.WriteLine(key + "=" + value.ToString());
        }

        /// <summary>
        /// NOTE: This is an extension method
        /// i.e. can call this thus:- writer.WriteConfigPath(string basePath, string key, string value)
        /// where var writer is type TextWriter.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="basePath"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void WriteConfigPath(this TextWriter writer, string basePath, string key, string value)
        {
            //var relValue = RelativePathTo(basePath, value);
            var relValue = basePath + "\\" + value;
            writer.WriteConfigValue(key, relValue);
        }

        public static void WriteConfigArray(this TextWriter writer, string keyPattern, object[] values)
        {
            if (values == null)
            {
            }

            for (int i = 0; i < values.Length; i++)
                writer.WriteConfigValue(string.Format(keyPattern, i + 1), values[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="basePath">the output directory</param>
        /// <param name="keyPattern">the key as reg exp</param>
        /// <param name="values"></param>
        public static void WriteConfigPathArray(this TextWriter writer, string basePath, string keyPattern, string[] values)
        {
            //Log.WriteLine("WriteConfigPathArray(): keyPattern=" + keyPattern, 1);
            if (keyPattern == null)
            {
                Log.WriteLine("WriteConfigPathArray() WARNING!!!! NULL VALUE for keyPattern");
                return;
            }
            if (values == null)
            {
                Log.WriteLine("WriteConfigPathArray() WARNING!!!! NULL ARRAY for KEY=" + keyPattern, '?');
                return;
            }
            for (int i = 0; i < values.Length; i++)
                writer.WriteConfigPath(basePath, string.Format(keyPattern, i + 1), values[i]);
        }


        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath);
            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toPath), true) != 0;
                if (isDifferentRoot)
                    return toPath;
            }

            var relativePath = new List<string>();
            string[] fromDirectories = fromDirectory.Split(Path.DirectorySeparatorChar);
            string[] toDirectories = toPath.Split(Path.DirectorySeparatorChar);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);
            int lastCommonRoot = -1;

            // find common root
            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                    break;
                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
                relativePath.Add(toDirectories[x]);

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativePath.ToArray());
        } //end of method RelativePathTo(string fromDirectory, string toPath)


    } //end of static class ConfigurationExtensions

}