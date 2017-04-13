// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Json.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Json type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared
{
    using System.IO;

    using Newtonsoft.Json;

    public static class Json
    {
        public static void Serialise<T>(FileInfo file, T obj)
        {
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (var stream = file.CreateText())
            using (var writer = new JsonTextWriter(stream))
            {
                serializer.Serialize(writer, obj);
            }
        }

        public static void Serialise<T>(TextWriter stream, T obj)
        {
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (var writer = new JsonTextWriter(stream))
            {
                serializer.Serialize(writer, obj);
            }
        }

        public static string SerialiseToString<T>(T obj, bool prettyPrint = true)
        {
            return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
        }


        public static T Deserialise<T>(FileInfo file)
        {
            var serializer = new JsonSerializer();

            using (var stream = file.OpenText())
            using (var reader = new JsonTextReader(stream))
            {
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
