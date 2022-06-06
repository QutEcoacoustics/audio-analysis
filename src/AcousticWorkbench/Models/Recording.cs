// <copyright file="Recording.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System;

    public class Recording
    {
        public long Id { get; set; }

        public string Uuid { get; set; }

        public DateTimeOffset RecordedDate { get; set; }

        public double DurationSeconds { get; set; }

        public int SampleRateHertz { get; set; }

        public int ChannelCount { get; set; }

        public string MediaType { get; set; }
    }
}