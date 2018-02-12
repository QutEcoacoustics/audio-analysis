// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Yaml.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Yaml type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.Serialization;

    using Zio;

    public class Yaml
    {
        internal static Deserializer Deserializer => new Deserializer();

        public static YamlDocument Load(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                return Load(stream);
            }
        }

        public static YamlDocument Load(TextReader stream)
        {
            // allow merging in yaml back references
            var parser = new MergingParser(new Parser(stream));

            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(parser);

            if (yamlStream.Documents.Count != 1)
            {
                throw new InvalidOperationException("Acoustics.Shared.Yaml supports loading only one document at a time");
            }

            return yamlStream.Documents[0];
        }

        public static T Deserialise<T>(FileInfo file)
        {
            return Deserialise<T>(file.ToFileEntry());
        }

        public static T Deserialise<T>(FileEntry file)
        {
            using (var stream = file.OpenText())
            {
                return Deserialize<T>(stream);
            }
        }

        public static T Deserialize<T>(TextReader stream)
        {
            // allow merging in yaml back references
            var parser = new MergingParser(new Parser(stream));
            var deserializer = Deserializer;

            return deserializer.Deserialize<T>(parser);
        }

        public static void Serialise<T>(FileInfo file, T obj)
        {
            using (var stream = file.CreateText())
            {
                var serializer = new SerializerBuilder().EmitDefaults().Build();

                serializer.Serialize(stream, obj);
            }
        }

        internal static (YamlDocument, T) LoadAndDeserialize<T>(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                // allow merging in yaml back references
                var parser = new MergingParser(new Parser(stream));

                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(parser);

                if (yamlStream.Documents.Count != 1)
                {
                    throw new InvalidOperationException(
                        "Acoustics.Shared.Yaml supports loading only one document at a time");
                }

                T obj = Deserializer.Deserialize<T>(parser);

                return (yamlStream.Documents[0], obj);
            }
        }
    }
}
