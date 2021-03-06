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
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NodeDeserializers;

    public static class Yaml
    {
        private static readonly Dictionary<string, Type> TagMappings = new Dictionary<string, Type>();

        static Yaml()
        {
            // find all YAML tags
            foreach (var tag in Meta.GetAttributesFromQutAssemblies<YamlTypeTagAttribute>())
            {
                TagMappings.Add(tag.Name, tag.Type);
            }
        }

        internal static IDeserializer Deserializer => new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTagMappings(TagMappings)
            .WithTypeConverter(new YamlNullableEnumTypeConverter())
            .Build();

        public static T Deserialize<T>(FileInfo file)
        {
            return Deserialize<T>(file.FullName);
        }

        public static T Deserialize<T>(string file)
        {
            using var stream = File.OpenText(file);
            return Deserialize<T>(stream);
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
            using var stream = file.CreateText();
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
                .WithTagMappings(TagMappings)
                .Build();

            serializer.Serialize(stream, obj);
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

        private class YamlNullableEnumTypeConverter : IYamlTypeConverter
        {
            // deals with nullable-enum parsing bug
            // https://github.com/aaubry/YamlDotNet/issues/544#issuecomment-761711947
            public bool Accepts(Type type)
            {
                return Nullable.GetUnderlyingType(type)?.IsEnum ?? false;
            }

            public object ReadYaml(IParser parser, Type type)
            {
                type = Nullable.GetUnderlyingType(type) ?? throw new ArgumentException("Expected nullable enum type for ReadYaml");

                if (parser.Accept<NodeEvent>(out var @event))
                {
                    if (NodeIsNull(@event))
                    {
                        parser.SkipThisAndNestedEvents();
                        return null;
                    }
                }

                var scalar = parser.Consume<Scalar>();
                try
                {
                    return Enum.Parse(type, scalar.Value, ignoreCase: true);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invalid value: \"{scalar.Value}\" for {type.Name}", ex);
                }
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                type = Nullable.GetUnderlyingType(type) ?? throw new ArgumentException("Expected nullable enum type for WriteYaml");

                if (value != null)
                {
                    var toWrite = Enum.GetName(type, value) ?? throw new InvalidOperationException($"Invalid value {value} for enum: {type}");
                    emitter.Emit(new Scalar(null, null, toWrite, ScalarStyle.Any, true, false));
                }
            }

            private static bool NodeIsNull(NodeEvent nodeEvent)
            {
                // http://yaml.org/type/null.html

                if (nodeEvent.Tag == "tag:yaml.org,2002:null")
                {
                    return true;
                }

                if (nodeEvent is Scalar scalar && scalar.Style == ScalarStyle.Plain)
                {
                    var value = scalar.Value;
                    return value is "" or "~" or "null" or "Null" or "NULL";
                }

                return false;
            }
        }
    }
}