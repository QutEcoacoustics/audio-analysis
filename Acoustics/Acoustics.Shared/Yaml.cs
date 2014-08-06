// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Yaml.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the Yaml type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Dynamic;
    using YamlDotNet.Serialization;

    public class Yaml
    {
        static Yaml()
        {
        }

        public static DynamicYaml Deserialise(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                var data = new DynamicYaml(stream);
                return data;
            }
        }

        public static void SerialiseDynamic(FileInfo file, dynamic obj)
        {
            // YMMV - not tested
            Serialise(file, obj);
        }

        public static T Deserialise<T>(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                var deserialiser = new Deserializer();
                return deserialiser.Deserialize<T>(stream);
            }
        }

        public static void Serialise<T>(FileInfo file, T obj)
        {
            using (var stream = file.CreateText())
            {
                var serialiser = new Serializer(SerializationOptions.EmitDefaults);
                serialiser.Serialize(stream, obj);
            }   
        }
    }

    /// <summary>
    /// An attempt to desrialize custom types - does not work.
    /// See: https://github.com/aaubry/YamlDotNet/issues/103.
    /// For now, serialize special types through proxy properties.
    /// </summary>
    public class YamlFileInfoConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(FileInfo);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
