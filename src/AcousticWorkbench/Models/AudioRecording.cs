// <copyright file="AudioRecording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System;
    using Newtonsoft.Json;

    public class AudioRecording : IModelWithMeta
    {
        public long Id { get; set; }

        public string Uuid { get; set; }

        public DateTimeOffset RecordedDate { get; set; }

        public long SiteId { get; set; }

        public double DurationSeconds { get; set; }

        public int SampleRateHertz { get; set; }

        public int Channels { get; set; }

        public int BitRateBps { get; set; }

        public string MediaType { get; set; }

        public long DataLengthBytes { get; set; }

        public string Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public string CanonicalFileName { get; set; }

        [JsonProperty("sites.name")]
        public string SiteName { get; set; }

        public Meta Meta { get; set; }
    }
}