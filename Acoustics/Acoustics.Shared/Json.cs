// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Json.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

            using (var stream = file.CreateText())
            using (var writer = new JsonTextWriter(stream))
            {
                serializer.Serialize(writer, obj);
            }   
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
