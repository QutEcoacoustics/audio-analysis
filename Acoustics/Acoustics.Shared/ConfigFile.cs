using System;
using System.Linq;

namespace Acoustics.Shared
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;

    public static class ConfigFile
    {
        private static readonly string ExecutingAssemblyPath = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;
        private static readonly string ExecutingAssemblyDirectory = Path.GetDirectoryName(ExecutingAssemblyPath);
        private static string defaultConfigFolder = Path.Combine(ExecutingAssemblyDirectory, "ConfigFiles");

        // Dirty Hack, clean up!
        public static readonly string LogFolder = Path.Combine(ExecutingAssemblyDirectory, "Logs");

        public static string ConfigFolder
        {
            get
            {
                if (Directory.Exists(defaultConfigFolder))
                {
                    return defaultConfigFolder;
                }
                
                throw new DirectoryNotFoundException($"Cannot find currently set ConfigFiles directory. {defaultConfigFolder} does not exist!");
            }
            set
            {
                defaultConfigFolder = value;
            }
        }

        public static FileInfo ResolveConfigFile(FileInfo file, params DirectoryInfo[] searchPaths)
        {
            Contract.Requires<ArgumentNullException>(file != null);

            return ResolveConfigFile(file.FullName, searchPaths);
        }

        public static FileInfo ResolveConfigFile(string file, params DirectoryInfo[] searchPaths)
        {
            FileInfo configFile;
            var success = TryResolveConfigFile(file, searchPaths, out configFile);

            if (success)
            {
                return configFile;
            }

            var searchedIn =
                searchPaths
                .Select(x => x.FullName)
                .Concat(new []{ ConfigFolder + " (and all subdirectories)", Directory.GetCurrentDirectory() })
                .Aggregate(string.Empty, (lines, dir) => lines += "\n\t" + dir);
            var message = $"The specified config file ({file}) could not be found.\nSearched in: {searchedIn}";
            throw new ConfigFileException(message, file);
        }

        public static bool TryResolveConfigFile(string file, DirectoryInfo[] searchPaths, out FileInfo configFile)
        {
            Contract.Ensures(Contract.Result<bool>() == false || Contract.Result<bool>() == true && Contract.ValueAtReturn(out configFile).Exists);

            configFile = null;
            if (string.IsNullOrWhiteSpace(file))
            {
                return false;
            }

            if (File.Exists(file))
            {
                configFile = new FileInfo(file);
                return true;
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
                    var searchPath = Path.GetFullPath(Path.Combine(directoryInfo.FullName, file));
                    if (File.Exists(searchPath))
                    {
                        configFile = new FileInfo(searchPath);
                        return true;
                    }
                }
            }

            var defaultConfigFile = Path.GetFullPath(Path.Combine(ConfigFolder, file));
            if (File.Exists(defaultConfigFile))
            {
                configFile = new FileInfo(defaultConfigFile);
                return true;
            }

            // search all sub-directories
            foreach (var directory in Directory.EnumerateDirectories(ConfigFolder, "*", SearchOption.AllDirectories))
            {
                var nestedDefaultConfigFile = Path.Combine(directory, file);
                if (File.Exists(nestedDefaultConfigFile))
                {
                    configFile = new FileInfo(nestedDefaultConfigFile);
                    return true;
                }
            }
            
            return false;
        }
    }

    public class ConfigFileException : Exception
    {
        public string File { get; set; }

        public const string Prelude = "Configuration exception: ";

        public ConfigFileException(string message)
            : base(message)
        {
        }

        public ConfigFileException(string message, string file)
            : base(message)
        {
            this.File = file;

        }

        public ConfigFileException(string message, Exception innerException, string file)
            : base(message, innerException)
        {
            this.File = file;
        }

        public ConfigFileException()
        {
        }

        public override string Message => Prelude + base.Message;
    }
}
