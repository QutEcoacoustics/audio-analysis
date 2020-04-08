// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigDictionary.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Configuration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Configuration files: this class is a wrapper around a Dictionary.
    /// </summary>
    [Obsolete]
    public class ConfigDictionary
    {
        private readonly Dictionary<string, string> dictionary;

        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDictionary"/> class.
        /// </summary>
        public ConfigDictionary()
        {
            this.dictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDictionary"/> class.
        /// The configuration.
        /// </summary>
        /// <param name="files">
        /// The files.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Argument is null.
        /// </exception>
        public ConfigDictionary(params string[] files)
            : this(files.Select(x => new FileInfo(x)).ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDictionary"/> class.
        /// The configuration.
        /// </summary>
        /// <param name="files">
        ///     The files.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Argument is null.
        /// </exception>
        public ConfigDictionary(params FileInfo[] files)
        {
            if (files == null || files.Length == 0)
            {
                throw new ArgumentNullException("files", "files must be supplied and contain entries.");
            }

            this.Source = files[files.Length - 1].FullName; // Take last file as filename
            this.dictionary = new Dictionary<string, string>();
            foreach (var file in files)
            {
                Dictionary<string, string> dict = ReadPropertiesFile(file);

                foreach (var item in dict)
                {
                    this.dictionary[item.Key] = item.Value;
                    ////if (item.Key.StartsWith("VERBOSITY")) LoggedConsole.WriteLine("VERBOSITY = " + item.Value);
                }
            }
        }

        public string ResolvePath(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (!Path.IsPathRooted(path))
            {
                if (this.Source == null)
                {
                    throw new InvalidOperationException("Configuration was not loaded from a file. Relative paths can not be resolved.");
                }

                return Path.Combine(Path.GetDirectoryName(this.Source), path);
            }

            return path;
        }

        public Dictionary<string, string> GetDictionary()
        {
            return this.dictionary;
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
            if (this.dictionary.ContainsKey(key))
            {
                this.dictionary.Remove(key);
            }

            this.dictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.dictionary.ContainsKey(key);
        }

        public Dictionary<string, string> GetTable()
        {
            return this.dictionary;
        }

        public string GetString(string key)
        {
            return this.dictionary.TryGetValue(key, out var value) ? value : null;
        }

        public string GetPath(string key)
        {
            return this.ResolvePath(this.GetString(key));
        }

        public int GetInt(string key)
        {
            return GetInt(key, this.dictionary);
        }

        public int? GetIntNullable(string key)
        {
            return GetIntNullable(key, this.dictionary);
        }

        public double GetDouble(string key)
        {
            return GetDouble(key, this.dictionary);
        }

        public double? GetDoubleNullable(string key)
        {
            return GetDoubleNullable(key, this.dictionary);
        }

        public bool GetBoolean(string key)
        {
            return GetBoolean(key, this.dictionary);
        } //end getBoolean

        public static void WriteConfgurationFile(Dictionary<string, string> dict, FileInfo path)
        {
            var lines = new List<string>();
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                lines.Add(kvp.Key + "=" + kvp.Value);
            }

            FileTools.WriteTextFile(path.FullName, lines);
        } // end WriteConfgurationFile()

        //#####################################################################################################################################
        //STATIC methods for configuration using Dictionary class.

        /// <summary>
        /// THIS ONLY WORKS IF ONLY HAVE KV PAIRS IN CONFIG FILE.
        /// IF HAVE COMMENTS ETC USE. <code>Dictionary{string,string} dict = FileTools.ReadPropertiesFile(file))</code>
        /// </summary>
        public static Dictionary<string, string> ReadKVPFile2Dictionary(string path)
        {
            Dictionary<string, string> dict = File.ReadAllLines(path).ToList().Select(s => s.Split('=')).ToDictionary(k => k[0], v => v[1]);
            return dict;
        } // end ReadKVPFile2Dictionary()

        public static bool GetBoolean(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key))
            {
                return false;
            }

            string value = dict[key].ToString();
            try
            {
                if (value == null)
                {
                    return false;
                }
                else
                {
                    return bool.Parse(value);
                }
            }
            catch (FormatException ex)
            {
                LoggedConsole.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                LoggedConsole.WriteLine(ex);
                return false;
            }
        } //end getBoolean()

        public static double GetDouble(string key, Dictionary<string, string> dict)
        {
            //if (Double.TryParse(str, out d))     dic.Add(key, str); // if done, then is a number

            if (!dict.ContainsKey(key))
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("DICTIONARY DOES NOT CONTAIN KEY: {0}", key);
                return -double.NaN;
            }

            string value = dict[key].ToString();
            if (value == null)
            {
                return -double.NaN;
            }

            try
            {
                double.TryParse(value, out var d);
                return d;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return -double.NaN;
            }
        }

        public static double? GetDoubleNullable(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            string value = dict[key].ToString();
            if (value == null)
            {
                return null;
            }

            try
            {
                double.TryParse(value, out var d);
                return d;
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return null;
            }
        }

        public static int GetInt(string key, Dictionary<string, string> dict)
        {
            string value = dict.TryGetValue(key, out value) ? value : null;

            //if (!table.ContainsKey(key)) return -Int32.MaxValue;

            //string value = this.table[key].ToString();
            if (value == null)
            {
                return -int.MaxValue;
            }

            try
            {
                if (int.TryParse(value, out var int32))
                {
                    return int32;
                }
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return int.MaxValue;
            }

            return int.MaxValue;
        }

        public static int? GetIntNullable(string key, Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey(key))
            {
                return null;
            }

            string value = dict[key].ToString();
            if (value == null)
            {
                return null;
            }

            try
            {
                if (int.TryParse(value, out var int32))
                {
                    return int32;
                }
            }
            catch
            {
                Log.WriteLine("ERROR READING PROPERTIES FILE");
                LoggedConsole.WriteLine("INVALID KVP: key={0}, value={1}", key, value);
                return null;
            }

            return null;
        }

        public static string ReadPropertyFromFile(string fName, string key)
        {
            Dictionary<string, string> dict = ReadPropertiesFile(fName);
            dict.TryGetValue(key, out var value);
            return value;
        }

        public static Dictionary<string, string> ReadPropertiesFile(string fName)
        {
            return ReadPropertiesFile(new FileInfo(fName));
        }

        public static Dictionary<string, string> ReadPropertiesFile(FileInfo fileName)
        {
            var fileInfo = fileName;
            if (!fileInfo.Exists)
            {
                return null;
            }

            var table = new Dictionary<string, string>();
            using (TextReader reader = fileName.OpenText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // read one line at a time and process
                    string trimmed = line.Trim();
                    if (trimmed == null)
                    {
                        continue;
                    }

                    if (trimmed.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] words = trimmed.Split('=');
                    if (words.Length == 1)
                    {
                        continue;
                    }

                    string key = words[0].Trim(); // trim because may have spaces around the = sign i.e. ' = '
                    string value = words[1].Trim();
                    if (!table.ContainsKey(key))
                    {
                        table.Add(key, value); // this may not be a good idea!
                    }
                } // end while
            } // end using

            return table;
        } // end ReadPropertiesFile()
    }

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

        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
            {
                throw new ArgumentNullException("fromDirectory");
            }

            if (toPath == null)
            {
                throw new ArgumentNullException("toPath");
            }

            bool isRooted = Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath);
            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toPath), true) != 0;
                if (isDifferentRoot)
                {
                    return toPath;
                }
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
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return toPath;
            }

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                {
                    relativePath.Add("..");
                }
            }

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), relativePath.ToArray());
        }
    }
}