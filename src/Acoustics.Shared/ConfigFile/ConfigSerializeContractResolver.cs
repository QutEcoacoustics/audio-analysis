// <copyright file="ConfigSerializeContractResolver.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.ConfigFile
{
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class ConfigSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ConfigSerializeContractResolver Instance = new ConfigSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Config) && property.PropertyName == nameof(Config.GenericConfig))
            {
                // only serialize generic config iff the current type is Config exactly
                property.ShouldSerialize = instance => typeof(Config) == instance.GetType();
            }

            return property;
        }
    }
}