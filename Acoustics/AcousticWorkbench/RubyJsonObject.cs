// <copyright file="RubyJsonObject.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

//    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
//    public abstract class RubyJsonObject
//    {
//    }

    public class AcousticWorkbenchResponse<T>
    {
        public Meta Meta { get; set; }

        public T Data { get; set; }
    }

    public class Meta
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"[Status: {this.Status}] {this.Message}";
        }
    }
}