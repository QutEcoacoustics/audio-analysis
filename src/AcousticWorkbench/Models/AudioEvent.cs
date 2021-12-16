// <copyright file="AudioEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System;
    using System.Collections.Generic;

    public class AudioEvent : IModelWithMeta
    {
        public long Id { get; set; }

        public long AudioRecordingId { get; set; }

        public double StartTimeSeconds { get; set; }

        public double EndTimeSeconds { get; set; }

        public double LowFrequencyHertz { get; set; }

        public double HighFrequencyHertz { get; set; }

        public bool IsReference { get; set; }

        public long CreatorId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public IList<Tagging> Taggings { get; set; }

        public Meta Meta { get; set; }
    }
}