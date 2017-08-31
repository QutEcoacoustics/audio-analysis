// <copyright file="ImportedEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AcousticWorkbench;
    using CsvHelper.Configuration;
    using log4net;

    public class ImportedEvent
    {
        public long? AudioEventId { get; set; }

        public long? AudioRecordingId { get; set; }

        public double? EventStartSeconds { get; set; }

        public double? EventEndSeconds { get; set; }

        public double? LowFrequencyHertz { get; set; }

        public double? HighFrequencyHertz { get; set; }

        public bool IsValid()
        {
            return this.AudioEventId.HasValue ||
                   (this.AudioRecordingId.HasValue
                   && this.EventStartSeconds.HasValue
                   && this.EventEndSeconds.HasValue
                   && this.LowFrequencyHertz.HasValue
                   && this.HighFrequencyHertz.HasValue);
        }

        public sealed class ImportedEventNameClassMap : CsvClassMap<ImportedEvent>
        {
            private static readonly PropertyInfo[] Properties = typeof(ImportedEvent).GetProperties();

            public ImportedEventNameClassMap()
            {
                // allow each field to be serialized by PascalCase, snake_case, or camelCase
                foreach (var property in Properties)
                {
                    var map = new CsvPropertyMap(property);
                    var pascalName = property.Name;
                    var snakeName = pascalName.ToSnakeCase();
                    var camelName = pascalName.ToCamelCase();

                    map.Name(pascalName, snakeName, camelName);

                    this.PropertyMaps.Add(map);
                }
            }
        }
    }
}
