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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    using Zio;

    public static class Yaml
    {
        private static readonly Dictionary<string, Type> TagMappings = new Dictionary<string, Type>();

        internal static IDeserializer Deserializer => new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTagMappings(TagMappings)
            .Build();

        /// <summary>
        /// Registers a type and a tag name that will be emitted in a YAML document when
        /// serializing, and will allow for unambiguous parsing when deserializing.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <param name="name">The tag name to use.</param>
        public static void AddTagMapping<T>(string name = null)
        {
            // yml type tags must be prefixed with an "!" character.
            if (name == null)
            {
                name = "!" + typeof(T).Name;
            }
            else
            {
                if (name[0] != '!')
                {
                    name = "!" + name;
                }
            }

            TagMappings.Add(name, typeof(T));
        }

        public static T Deserialize<T>(FileInfo file)
        {
            return Deserialize<T>(file.ToFileEntry());
        }

        public static T Deserialize<T>(FileEntry file)
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

        public static void Serialize<T>([NotNull] FileInfo file, [CanBeNull] T obj)
        {
            using (var stream = file.CreateText())
            {
                var serializer = new SerializerBuilder()
                    .EmitDefaults()
                    .WithTagMappings(TagMappings)
                    .Build();

                serializer.Serialize(stream, obj);
            }
        }

        internal static (object, T) LoadAndDeserialize<T>(string path)
        {
            using (var stream = File.OpenText(path))
            {
                // allow merging in yaml back references
                var parser = new MergingParser(new Parser(stream));

                var generic = Deserializer.Deserialize<object>(parser);

                // it appears we can't just reuse the parser :-/ so we have to recreate it :-(
                stream.DiscardBufferedData();
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                parser = new MergingParser(new Parser(stream));

                T obj = Deserializer.Deserialize<T>(parser);

                return (generic, obj);
            }
        }

        /// <summary>
        /// A helper extension method that registers multiple tag mappings with
        /// the <see cref="SerializerBuilder"/> or the <see cref="DeserializerBuilder"/>.
        /// </summary>
        private static T WithTagMappings<T>(
            this T builder,
            Dictionary<string, Type> mappings)
            where T : BuilderSkeleton<T>
        {
            return mappings
                .Aggregate(builder, (b, kvp) => b.WithTagMapping(kvp.Key, kvp.Value));
        }
    }
}
