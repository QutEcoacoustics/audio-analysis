using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acoustics.Shared
{
    using System.IO;

    using YamlDotNet.Dynamic;
    using YamlDotNet.Serialization;

    public class Yaml
    {

        public static DynamicYaml Deserialise(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                var data = new DynamicYaml(stream);
                return data;
            }
        }

        public static void Serialise(FileInfo file, dynamic obj)
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
                var serialiser = new Serializer();
                serialiser.Serialize(stream, obj);
            }   
        }
    }
}
