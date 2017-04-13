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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Fasterflect;
    using YamlDotNet.Dynamic;
    using YamlDotNet.RepresentationModel;

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

            set
            {
                defaultConfigFolder = value;
            }
        }

        public static dynamic GetProfile(dynamic configuration, string profileName)
        {
            if (configuration.GetType() != typeof(DynamicYaml))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            return configuration[ProfilesKey][profileName];
        }

        public static bool HasProfiles(dynamic configuration)
        {
            if (configuration.GetType() != typeof(DynamicYaml))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            DynamicYaml profiles = configuration[ProfilesKey] ?? null;

            return profiles != null;
        }

        public static bool TryGetProfile(dynamic configuration, string profileName, out dynamic profile)
        {
            if (configuration.GetType() != typeof(DynamicYaml))
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
            if (configuration.GetType() != typeof(DynamicYaml))
            {
                throw new ArgumentException("The configuration parameter must be a DynamicYaml object", nameof(configuration));
            }

            if (!HasProfiles(configuration))
            {
                return null;
            }

            DynamicYaml profiles = configuration[ProfilesKey];

            // WARN: This is a dirty, dirty, hack. I apologize to my mother for writing this vulgarity.
            var yamlMappingNode = (YamlMappingNode)profiles.GetFieldValue("mappingNode");
            var yamlNodes = yamlMappingNode.Children.Select(kvp => kvp.Key.ToString());
            var keys = yamlNodes.ToArray();

            return keys;
        }

        public static Dictionary<string, dynamic> GetAllProfiles(dynamic configuration)
        {
            if (configuration.GetType() != typeof(DynamicYaml))
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
            FileInfo configFile;
            var success = TryResolveConfigFile(file, searchPaths, out configFile);

            if (success)
            {
                return configFile;
            }

            var searchedIn =
                searchPaths.Select(x => x.FullName)
                    .Concat(new[] { ConfigFolder + " (and all subdirectories)", Directory.GetCurrentDirectory() })
                    .Aggregate(string.Empty, (lines, dir) => lines += "\n\t" + dir);
            var message = $"The specified config file ({file}) could not be found.\nSearched in: {searchedIn}";
            throw new ConfigFileException(message, file);
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
}