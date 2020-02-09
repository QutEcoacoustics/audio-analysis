// <copyright file="ImportedEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.Reflection;
    using Acoustics.Shared;
    using CsvHelper.Configuration;

    public class ImportedEvent
    {
        public long? AudioEventId { get; set; }

        public long? AudioRecordingId { get; set; }

        public double? EventStartSeconds { get; set; }

        public double? EventEndSeconds { get; set; }

        public double? LowFrequencyHertz { get; set; }

        public double? HighFrequencyHertz { get; set; }

        /// <summary>
        /// Gets or sets the order, a tag field that allows us to maintain the order of imported events, as provided to
        /// the program.
        /// </summary>
        internal int Order { get; set; }

        public bool IsValid()
        {
            return this.AudioEventId.HasValue ||
                   (this.AudioRecordingId.HasValue
                   && this.EventStartSeconds.HasValue
                   && this.EventEndSeconds.HasValue
                   && this.LowFrequencyHertz.HasValue
                   && this.HighFrequencyHertz.HasValue);
        }

        /// <summary>
        /// Returns a JSON encoded string that describes this object.
        /// </summary>
        /// <returns>The string that describes this object.</returns>
        public override string ToString()
        {
            return Json.SerializeToString(this);
        }

        public sealed class ImportedEventNameClassMap : ClassMap<ImportedEvent>
        {
            private static readonly PropertyInfo[] Properties = typeof(ImportedEvent).GetProperties();

            public ImportedEventNameClassMap()
            {
                // allow each field to be serialized by PascalCase, snake_case, or camelCase
                foreach (var property in Properties)
                {
                    var map = MemberMap.CreateGeneric(typeof(ImportedEvent), property);
                    var pascalName = property.Name;
                    var snakeName = pascalName.ToSnakeCase();
                    var camelName = pascalName.ToCamelCase();

                    map.Name(pascalName, snakeName, camelName);

                    this.MemberMaps.Add(map);
                }
            }
        }
    }
}
