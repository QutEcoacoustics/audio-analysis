// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the ConfigFile type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Dynamic;

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
    using YamlDotNet.RepresentationModel;

    using Zio;

    public static class ConfigFile
    {
        public const string ProfilesKey = "Profiles";

        private static readonly string ExecutingAssemblyPath =
            (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

        private static readonly string ExecutingAssemblyDirectory = Path.GetDirectoryName(ExecutingAssemblyPath);

        private static string defaultConfigFolder = Path.Combine(ExecutingAssemblyDirectory, "ConfigFiles");

        // Dirty Hack, clean up!
        public static string LogFolder { get; } = Path.Combine(ExecutingAssemblyDirectory, "Logs");

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

            set => defaultConfigFolder = value;
        }

        public static dynamic GetProfile(dynamic configuration, string profileName)
        {
            // TODO: broken when dynamic yaml removed
            if (configuration.GetType() != typeof(DynamicObject))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            return configuration[ProfilesKey][profileName];
        }

        public static bool HasProfiles(dynamic configuration)
        {
            // TODO: broken when dynamic yaml removed
            if (configuration.GetType() != typeof(DynamicObject))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            DynamicObject profiles = configuration[ProfilesKey] ?? null;

            return profiles != null;
        }

        public static bool TryGetProfile(dynamic configuration, string profileName, out dynamic profile)
        {
            // TODO: broken when dynamic yaml removed
            if (configuration.GetType() != typeof(DynamicObject))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            profile = null;

            var hasProfiles = HasProfiles(configuration);

            if (!hasProfiles)
            {
                return false;
            }

            profile = configuration[ProfilesKey][profileName] ?? null;

            return profile != null;
        }

        public static string[] GetProfileNames(dynamic configuration)
        {
            // TODO: broken when dynamic yaml removed
            if (configuration.GetType() != typeof(DynamicObject))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            if (!HasProfiles(configuration))
            {
                return null;
            }

            // TODO: broken when dynamic yaml removed
            DynamicObject profiles = configuration[ProfilesKey];

            // WARN: This is a dirty, dirty, hack. I apologize to my mother for writing this vulgarity.
            var yamlMappingNode = (YamlMappingNode)profiles.GetFieldValue("mappingNode");
            var yamlNodes = yamlMappingNode.Children.Select(kvp => kvp.Key.ToString());
            var keys = yamlNodes.ToArray();

            return keys;
        }

        public static Dictionary<string, dynamic> GetAllProfiles(dynamic configuration)
        {
            // TODO: broken when dynamic yaml removed
            if (configuration.GetType() != typeof(DynamicObject))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            var profiles = configuration[ProfilesKey];

            if (profiles == null)
            {
                return null;
            }

            return (Dictionary<string, dynamic>)profiles;
        }

        public static FileInfo ResolveConfigFile(FileInfo file, params DirectoryInfo[] searchPaths)
        {
            Contract.Requires<ArgumentNullException>(file != null);

            return ResolveConfigFile(file.FullName, searchPaths);
        }

        public static FileInfo ResolveConfigFile(string file, params DirectoryInfo[] searchPaths)
        {
            if (file.IsNullOrEmpty())
            {
                throw new ArgumentException("Try to resolve config failed, because supplied file argument was null or empty.", nameof(file));
            }

            var success = TryResolveConfigFile(file, searchPaths.Select(x => x.ToDirectoryEntry()), out FileEntry configFile);

            if (success)
            {
                return configFile.ToFileInfo();
            }

            var searchedIn =
                searchPaths.Select(x => x.FullName)
                    .Concat(new[] { ConfigFolder + " (and all subdirectories)", Directory.GetCurrentDirectory() })
                    .Aggregate(string.Empty, (lines, dir) => lines += "\n\t" + dir);
            var message = $"The specified config file ({file}) could not be found.\nSearched in: {searchedIn}";
            throw new ConfigFileException(message, file);
        }

        public static bool TryResolveConfigFile(string file, IEnumerable<DirectoryEntry> searchPaths, out FileEntry configFile)
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