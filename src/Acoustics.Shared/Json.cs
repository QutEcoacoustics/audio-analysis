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
    using System;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Acoustics.Shared.Contracts;

    public static class Json
    {
        static Json()
        {
            Serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
            };
        }

        public static JsonSerializer Serializer { get; }

        public static void Serialise<T>(FileInfo file, T obj)
        {
            using (var stream = file.CreateText())
            using (var writer = new JsonTextWriter(stream))
            {
                Serializer.Serialize(writer, obj);
            }
        }

        public static void Serialise<T>(TextWriter stream, T obj)
        {
            using (var writer = new JsonTextWriter(stream))
            {
                Serializer.Serialize(writer, obj);
            }
        }

        public static string SerializeToString<T>(T obj, bool prettyPrint = true)
        {
            return SerializeToString(obj, prettyPrint, null);
        }

        public static T Deserialize<T>(FileInfo file)
        {
            return Deserialize<T>(file.FullName);
        }

        public static T Deserialize<T>(string file)
        {
            using (var stream = File.OpenText(file))
            using (var reader = new JsonTextReader(stream))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }

        public static T DeserializeFromString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal static string SerializeToString<T>(T obj, bool prettyPrint, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None, settings);
        }

        public class LegacyTimeSpanDataConverter : JsonConverter
        {
            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                // we only want to allow conversion where this converter is explicitly attached via a
                // JsonConverterAttribute
                throw new NotImplementedException();
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);
                if (token.Type == JTokenType.String)
                {
                    var seconds = ((TimeSpan)token).TotalSeconds;
                    if (objectType.IsAssignableFrom(typeof(double)))
                    {
                        return seconds;
                    }
                    else
                    {
                        LoggedConsole.WriteWarnLine("LegacyTimeSpanDataConverter is truncating values.");
                        return (int)seconds;
                    }
                }

                // otherwise fallback to default parsing semantics
                return token.ToObject(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
