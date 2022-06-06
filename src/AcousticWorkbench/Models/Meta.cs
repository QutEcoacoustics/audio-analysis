// <copyright file="Meta.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System.Collections.Generic;

    public class Meta
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public Error Error { get; set; }

        public Paging Paging { get; set; }

        public Dictionary<string, Capability> Capabilities { get; set; }

        public override string ToString()
        {
            return $"[Status: {this.Status}] {this.Message}\n" +
                (this.Error?.ToString() ?? string.Empty);
        }
    }
}