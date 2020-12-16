// <copyright file="ConfigFile.Deserialize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using Acoustics.Shared.Contracts;
    using log4net;
    using Newtonsoft.Json;
    using ObjectCloner.Extensions;

    public static partial class ConfigFile
    {
        private static readonly ConcurrentDictionary<string, IConfig> CachedProperties;
        private static JsonSerializerSettings configJsonSerializerSettings;

        static ConfigFile()
        {
            CachedProperties = new ConcurrentDictionary<string, IConfig>();
            configJsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = ConfigSerializeContractResolver.Instance,
            };
        }

        public static Config Deserialize(FileInfo file)
        {
            return Deserialize(file.FullName);
        }

        public static Config Deserialize(string path)
        {
            return LoadAndCache(path, () => new Config());
        }

        public static T Deserialize<T>(FileInfo file)
            where T : IConfig, new()
        {
            return Deserialize<T>(file.FullName);
        }

        public static T Deserialize<T>(string path)
            where T : IConfig, new()
        {
            return LoadAndCache<T>(path, null);
        }

        [Obsolete("use only for writing tests!")]
        internal static Config Deserialize(TextReader streamReader, string configPath)
        {
            Config config = new Config
            {
                GenericConfig = Yaml.Deserialize<object>(streamReader),
                ConfigPath = configPath,
            };
            ((IConfig)config).InvokeLoaded();

            Contract.EnsuresNotNull(config.GenericConfig);
            Contract.EnsuresNotNull(config.ConfigPath);
            return config;
        }

        internal static void FlushCache()
        {
            CachedProperties.Clear();
        }

        /// <summary>
        /// Reads a config file. Supports both "dynamic" and "static" config files.
        /// Additionally dumps config files into the log as JSON on first read for experimental provenance.
        /// Additionally caches the contents of config file on read based on the fully-qualified path.
        /// All configs returned are clones of the cached copy (even the first config).
        /// </summary>
        /// <remarks>
        /// Support exists for processing recursive config file (where a <see cref="Config"/> objected is nested in another
        /// <see cref="Config"/>.
        /// </remarks>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="path">
        /// The path to the config file to read (will be expanded with <see cref="Path.GetFullPath"/>.
        /// </param>
        /// <param name="factory">
        /// A factory used to create a new config if <typeparamref name="T"/> is exactly the type <see cref="Config"/>.
        /// </param>
        /// <returns>The config object, or a cached copy after the first call.</returns>
        private static T LoadAndCache<T>(string path, Func<T> factory)
            where T : IConfig
        {
            Contract.RequiresNotNull(path, nameof(path));
            path = Path.GetFullPath(path);

            lock (CachedProperties)
            {
                // "cache" path skips this
                if (!CachedProperties.TryGetValue(path, out var cachedConfig))
                {
                    // not cached, load, log, and cache
                    T loadedConfig;
                    object generic;

                    // if is exactly the Config type (no sub types)
                    if (typeof(T) == typeof(Config))
                    {
                        // "untyped" config
                        Log.Trace($"Reading untyped config file `{path}`");
                        using (var file = File.OpenText(path))
                        {
                            generic = Yaml.Deserialize<object>(file);
                        }

                        loadedConfig = factory();
                    }
                    else
                    {
                        // deserialize typed config
                        Log.Trace($"Reading typed config file `{path}`");
                        (generic, loadedConfig) = Yaml.LoadAndDeserialize<T>(path);
                    }

                    if (loadedConfig is null)
                    {
                        throw new ConfigFileException($"Tried to load the {path} config file but it looks like it was empty. Add some content?", path);
                    }

                    // if implements Config in any subtype (more specific than IConfig)
                    if (loadedConfig is Config config)
                    {
                        config.GenericConfig = generic;
                        Contract.EnsuresNotNull(config.GenericConfig);
                    }

                    loadedConfig.ConfigPath = path;
                    Contract.EnsuresNotNull(loadedConfig.ConfigPath);

                    // dump the config in the log
                    configJsonSerializerSettings = new JsonSerializerSettings();
                    var configDump = Json.SerializeToString(loadedConfig, false, configJsonSerializerSettings);
                    NoConsole.Log.Info($"Config file `{path}` loaded:{Environment.NewLine}{configDump}");

                    // this has the potential to be recursive here if a config file loads another config file.
                    ((IConfig)loadedConfig).InvokeLoaded();

                    // cache the config (with possible nested configs)
                    CachedProperties.AddOrUpdate(path, loadedConfig, (key, existing) => loadedConfig);

                    cachedConfig = loadedConfig;
                }

                // always need to clone a copy to protect from cross-thread mutability
                return ((T)cachedConfig).DeepClone();
            }
        }
    }
}