// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ConfigFile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Fasterflect;

    using log4net;

    using YamlDotNet.RepresentationModel;

    using Zio;

    public static partial class ConfigFile
    {
        public const string ProfilesKey = nameof(IProfile<object>.Profiles);

        public static readonly Dictionary<Type, string> Defaults = new Dictionary<Type, string>();

        private static readonly ILog Log = LogManager.GetLogger(nameof(ConfigFolder));
        private static string defaultConfigFolder = Path.Combine(AppConfigHelper.ExecutingAssemblyDirectory, "ConfigFiles");

        public static string ConfigFolder
        {
            get
            {
                if (Directory.Exists(defaultConfigFolder))
                {
                    return defaultConfigFolder;
                }

                throw new DirectoryNotFoundException(
                    $"Cannot find currently set ConfigFiles directory. {defaultConfigFolder} does not exist!");
            }

            internal set => defaultConfigFolder = value;
        }

        public static Config Deserialize(FileInfo file)
        {
            return new Config(Yaml.Load(file), file.FullName);
        }

        public static T Deserialize<T>(FileInfo file)
            where T : Config
        {
            
            (YamlDocument document, T config) = Yaml.LoadAndDeserialize<T>(file);

            config.ConfigYamlDocument = document;

            return config;
        }


        public static FileInfo ResolveOrDefault<T>(FileInfo file, params DirectoryInfo[] searchPaths)
        {
            return ResolveOrDefault<T>(file.FullName, searchPaths);
        }

        public static FileInfo ResolveOrDefault<T>(string path, params DirectoryInfo[] searchPaths)
        {
            if (!path.IsNullOrEmpty())
            {
                var success = TryResolve(path, searchPaths.Select(x => x.ToDirectoryEntry()), out FileEntry configFile);

                if (success)
                {
                    return configFile.ToFileInfo();
                }
            }

            if (TryDefault<T>(out var errorMessage, out var fileInfo))
            {
                return fileInfo;
            }

            var message = NotFoundMessage(path, searchPaths)
                          + "\nAdditionally, no default could be found either because "
                          + errorMessage;
            throw new ConfigFileException(message);
        }
        public static FileInfo Default<T>()
        {
            if (TryDefault<T>(out var errorMessage, out var fileInfo))
            {
                return fileInfo;
            }

            throw new ConfigFileException("Loading default config failed because " + errorMessage);
        }

        private static bool TryDefault<T>(out string errorMessage, out FileInfo fileInfo)
        {
            errorMessage = string.Empty;
            fileInfo = null;
            Type configType = typeof(T);

            // lookup default config from well known names list
            if (Defaults.TryGetValue(configType, out var defaultName))
            {
                FileEntry defaultConfig = null;
                if (TryResolveInConfigFolder(defaultName, ref defaultConfig))
                {
                    var file = defaultConfig.ToFileInfo();
                    Log.Info($"Supplied config file not found, but a default was found and returned (`{file}`)");
                    {
                        fileInfo = file;
                        return true;
                    }
                }

                errorMessage = $"attempt to find default file `{defaultName}` in config folders failed";
            }
            else
            {
                errorMessage = $"no default was registered for type `{configType.Name}`";
            }

            return false;
        }

        public static FileInfo Resolve(FileInfo file, params DirectoryInfo[] searchPaths)
        {
            Contract.Requires<ArgumentNullException>(file != null);

            return Resolve(file.FullName, searchPaths);
        }

        public static FileInfo Resolve(string file, params DirectoryInfo[] searchPaths)
        {
            if (file.IsNullOrEmpty())
            {
                throw new ArgumentException("Try to resolve config failed, because supplied file argument was null or empty.", nameof(file));
            }

            var success = TryResolve(file, searchPaths.Select(x => x.ToDirectoryEntry()), out FileEntry configFile);

            if (success)
            {
                return configFile.ToFileInfo();
            }

            var message = NotFoundMessage(file, searchPaths);
            throw new ConfigFileException(message, file);
        }

        private static string NotFoundMessage(string file, DirectoryInfo[] searchPaths)
        {
            var searchedIn = searchPaths
                .Select(x => x.FullName)
                .Distinct()
                .Concat(new[] { ConfigFolder + " (and all subdirectories)", Directory.GetCurrentDirectory() })
                .Aggregate(string.Empty, (lines, dir) => lines + "\n\t" + dir);
            var message = $"The specified config file ({file}) could not be found.\nSearched in: {searchedIn}";
            return message;
        }

        public static bool TryResolve(string file, IEnumerable<DirectoryEntry> searchPaths, out FileEntry configFile)
        {
            configFile = null;
            if (string.IsNullOrWhiteSpace(file))
            {
                return false;
            }

            // this is a holdover from concrete file systems. The concept of a working directory has no real
            // equivalent in a virtual file system but this is implemented for compatibility
            // GetFullPath should take care of relative paths relative to current working directory
            var fullPath = Path.GetFullPath(file);
            if (File.Exists(fullPath))
            {
                configFile = fullPath.ToFileEntry();
                return true;
            }

            if (Path.IsPathRooted(file))
            {
                // if absolute on concrete file system and can't be found then we can't resolve automatically
                return false;
            }

            if (searchPaths != null)
            {
                foreach (var directory in searchPaths)
                {
                    var searchPath = directory.CombineFile(file);
                    if (searchPath.Exists)
                    {
                        configFile = searchPath;
                        return true;
                    }
                }
            }

            return TryResolveInConfigFolder(file, ref configFile);
        }

        private static bool TryResolveInConfigFolder(string file, ref FileEntry configFile)
        {
            // config files are always packaged with the app so use a physical file system
            var defaultConfigFile = Path.GetFullPath(Path.Combine(ConfigFolder, file));
            if (File.Exists(defaultConfigFile))
            {
                configFile = defaultConfigFile.ToFileEntry();
                return true;
            }

            // search all sub-directories
            foreach (var directory in Directory.EnumerateDirectories(ConfigFolder, "*", SearchOption.AllDirectories))
            {
                var nestedDefaultConfigFile = Path.Combine(directory, file);
                if (File.Exists(nestedDefaultConfigFile))
                {
                    configFile = nestedDefaultConfigFile.ToFileEntry();
                    return true;
                }
            }
            return false;
        }
    }
}