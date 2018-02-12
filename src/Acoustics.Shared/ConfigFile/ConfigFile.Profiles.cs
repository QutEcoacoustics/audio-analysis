namespace Acoustics.Shared.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using YamlDotNet.RepresentationModel;

    public static partial class ConfigFile
    {
        public static Config GetProfile<TConfig>(TConfig configuration, string profileName)
            where TConfig : Config
        {
            if (TryGetProfile(configuration, profileName, out var profile))
            {
                return profile;
            }

            throw new ArgumentException("profile not found");
        }

        public static bool TryGetProfile<TConfig>(TConfig configuration, string profileName, out Config profile)
            where TConfig : Config
        {
            profile = null;

            var root = configuration.ConfigYamlDocument.RootNode as YamlMappingNode;
            var profiles =
                (YamlMappingNode)root?.Children.First(kvp => ((YamlScalarNode)kvp.Key).Value == ProfilesKey).Value;

            // find matching profile
            var foundNodes = profiles.Where(kvp => (string)kvp.Key == profileName);
            if (foundNodes.Count() != 1)
            {
                return false;
            }

            var doc = new YamlDocument(foundNodes.First().Value);
            profile = new Config(doc, configuration.ConfigPath);

            return true;
        }

        public static bool HasProfiles(Config configuration)
        {
            var root = configuration.ConfigYamlDocument.RootNode as YamlMappingNode;
            return root?.Children.Any(kvp => ((YamlScalarNode)kvp.Key).Value == ProfilesKey) ?? false;
        }

        public static string[] GetProfileNames<TConfig>(TConfig configuration)
            where TConfig : Config
        {
            var root = configuration.ConfigYamlDocument.RootNode as YamlMappingNode;
            var profiles =
                (YamlMappingNode)root?.Children.First(kvp => ((YamlScalarNode)kvp.Key).Value == ProfilesKey).Value;

            // extract keys
            return profiles.Select(kvp => (string)kvp.Key).ToArray();
        }

        public static IEnumerable<(string Name, object Profile)> GetAllProfiles<T>(T configuration)
            where T : IProfile<T>
        {
            Type profileType = typeof(T);

            var props = profileType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                var value = propertyInfo.GetValue(configuration.Profiles);
                var name = propertyInfo.Name;

                yield return (Name: name, Profile: value);
            }
        }
    }
}