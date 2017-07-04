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

    public class ImportedEvent
    {
        public long AudioEventId { get; set; }

        public long AudioRecordingId { get; set; }

        public double EventStartSeconds { get; set; }

        public double EventEndSeconds { get; set; }


        private static Type thisType = typeof(ImportedEvent);

        private Dictionary<PropertyInfo, string[]> columnMappings = new Dictionary<PropertyInfo, string[]>()
        {
            {
                thisType.GetProperty(nameof(ImportedEvent.AudioEventId)),
                eventId
            },
            {
                thisType.GetProperty(nameof(ImportedEvent.AudioRecordingId)),
                recordingId
            },
            {
                thisType.GetProperty(nameof(ImportedEvent.EventStartSeconds)),
                start
            },
            {
                thisType.GetProperty(nameof(ImportedEvent.EventEndSeconds)),
                end
            },
        };

        private static string[] eventId = new[] { "AudioEventId", "audio_event_id" };
        private static string[] recordingId = new[] { "AudioRecordingId", "audio_recording_id" };
        private static string[] start = new[] { "EventStartSeconds", "event_start_seconds" };
        private static string[] end = new[] { "EventEndSeconds", "event_end_seconds" };

        private static readonly string[][] segmentStrings = new string[][]
        {
            recordingId,
            start,
            end
        };

        public static void MapColumns
    }
}
