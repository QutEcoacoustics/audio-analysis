// <copyright file="Tagging.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench.Models
{
    using System;

    public class Tagging
    {
        public long Id { get; set; }

        public long AudioEventId { get; set; }

        public long TagId { get; set; }

        public long CreatorId { get; set; }

        public long UpdaterId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}