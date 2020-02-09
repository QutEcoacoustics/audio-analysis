// <copyright file="ConfigFile.Profiles.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

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

            if (!(configuration.GenericConfig is Dictionary<object, object> root))
            {
                return false;
            }

            if (!root.TryGetValue(ProfilesKey, out var profileNode))
            {
                return false;
            }

            if (!(profileNode is Dictionary<object, object> profileMapping))
            {
                return false;
            }

            // find matching profile
            if (!profileMapping.TryGetValue(profileName, out var mapping))
            {
                return false;
            }

            profile = new Config() { GenericConfig = mapping, ConfigPath = configuration.ConfigPath };
            return true;
        }

        public static bool HasProfiles(Config configuration)
        {
            if (!(configuration.GenericConfig is Dictionary<object, object> root))
            {
                return false;
            }

            if (!root.TryGetValue(ProfilesKey, out var profileNode))
            {
                return false;
            }

            return profileNode is Dictionary<object, object>;
        }

        public static string[] GetProfileNames<TConfig>(TConfig configuration)
            where TConfig : Config
        {
            if (!(configuration.GenericConfig is Dictionary<object, object> root))
            {
                return null;
            }

            if (!root.TryGetValue(ProfilesKey, out var profileNode))
            {
                return null;
            }

            if (!(profileNode is Dictionary<object, object> profileMapping))
            {
                return null;
            }

            return profileMapping.Keys.Cast<string>().ToArray();
        }

        // public static IEnumerable<(string Name, object Profile)> GetAllProfiles<T>(T configuration)
        //     where T : IProfile<T>
        // {
        //     Type profileType = typeof(T);
        //
        //     var props = profileType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //     foreach (var propertyInfo in props)
        //     {
        //         var value = propertyInfo.GetValue(configuration.Profiles);
        //         var name = propertyInfo.Name;
        //
        //         yield return (Name: name, Profile: value);
        //     }
        // }
    }
}