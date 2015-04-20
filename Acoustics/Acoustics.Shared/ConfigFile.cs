using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acoustics.Shared
{
    using System.IO;

    public class ConfigFile
    {
        private static readonly string ExecutingAssemblyPath = System.Reflection.Assembly.GetEntryAssembly().Location;
        private static readonly string ConfigFolder = Path.Combine(ExecutingAssemblyPath, "ConfigFiles");

        public static FileInfo ResolveConfigFile(FileInfo file, params DirectoryInfo[] searchPaths)
        {
            return ResolveConfigFile(file.FullName, searchPaths);
        }

        public static FileInfo ResolveConfigFile(string file, params DirectoryInfo[] searchPaths)
        {
            FileInfo configFile;
            var sucess = TryResolveConfigFile(file, searchPaths, out configFile);

            if (sucess)
            {
                return configFile;
            }

            throw new FileNotFoundException("The specified config file could not be found", file);
        }

        public static bool TryResolveConfigFile(string file, DirectoryInfo[] searchPaths, out FileInfo configFile)
        {
            configFile = null;
            if (string.IsNullOrWhiteSpace(file))
            {
                return false;
            }

            if (File.Exists(file))
            {
                configFile = new FileInfo(file);
            }

            // if it does not exist
            // and is rooted, it can't exist
            if (Path.IsPathRooted(file))
            {
                return false;
            }

            if (searchPaths != null && searchPaths.Length > 0)
            {
                foreach (var directoryInfo in searchPaths)
                {
                    var searchPath = Path.Combine(directoryInfo.FullName, file);
                    if (File.Exists(searchPath))
                    {
                        configFile = new FileInfo(searchPath);
                        return true;
                    }
                }
            }

            var defaultConfigFile = Path.Combine(ConfigFolder, file);
            if (File.Exists(defaultConfigFile))
            {
                configFile = new FileInfo(file);
                return true;
            }

            return false;
        }
    }
}
