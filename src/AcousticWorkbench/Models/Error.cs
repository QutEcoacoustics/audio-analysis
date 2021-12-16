// <copyright file="Error.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Error
    {
        public string Details { get; set; }

        public Dictionary<string, string> Links { get; set; }

        public JRaw Info { get; set; }

        public override string ToString()
        {
            return "API error: " + this.Details + Environment.NewLine + (this.Info?.ToString(Formatting.Indented) ?? string.Empty);
        }
    }
}