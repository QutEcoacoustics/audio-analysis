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
    using System.Linq;
    using System.Text.RegularExpressions;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Dynamic;
    using YamlDotNet.RepresentationModel;
    using YamlDotNet.Serialization;

    public class Yaml
    {
        public static DynamicYaml Deserialise(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                // allow merging in yaml back references
                var parser = new EventReader(new MergingParser(new Parser(stream)));

                // MEGA HACK TIME - I APOLOGIZE TO FUTURE ME :-(
                // There's a bug in the YamlStream implementation that does not allow the MergingParser's magic to work because it produces a
                // a duplicate key in an object mapping graph in a yaml back referencing scenario.
                // So to deserialize the graph properly, we first deserialize it generically - which expands all the yaml back references - and we
                // then reserialize to an in memory string, which we can finally send to DynamicYaml (which uses yamlDocument.Load under the sheets).
                // TODO: file a bug against the YamlDotNet project.
                var d = new Deserializer();
                var deserializedObject = d.Deserialize(parser);
                var s = new Serializer();

                DynamicYaml data;
                using (var stream2 = new StringWriter())
                {
                    s.Serialize(stream2, deserializedObject);

                    var yaml = stream2.ToString();
                    data = new DynamicYaml(yaml);
                }
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
                // allow merging in yaml back references
                var parser = new EventReader(new MergingParser(new Parser(stream)));
                var deserializer = new Deserializer();

                return deserializer.Deserialize<T>(parser);
            }
        }

        public static void Serialise<T>(FileInfo file, T obj)
        {
            using (var stream = file.CreateText())
            {
                var serializer = new Serializer(SerializationOptions.EmitDefaults);
                serializer.Serialize(stream, obj);
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
