// <copyright file="EventStatisticsAnalysis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench.Orchestration;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.EventStatistics;
    using AudioAnalysisTools.WavTools;
    using global::AcousticWorkbench;
    using log4net;

    public partial class EventStatisticsAnalysis : AbstractStrongAnalyser
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EventStatisticsAnalysis));

        public override string DisplayName { get; } = "Event statistics calculation";

        public override string Identifier { get; } = "Ecosounds.EventStatistics";

        public override string Description { get; } = "Event statistics calculation analysis used to extract critical statistics (features) from an acoustic event";

        public override AnalysisSettings DefaultSettings { get; } = new AnalysisSettings
        {
            // The workbench supports extracting as much as 5 minutes of data at a time
            AnalysisMaxSegmentDuration = MediaService.MediaDownloadMaximumSeconds.Seconds(),
            AnalysisMinSegmentDuration = MediaService.MediaDownloadMinimumSeconds.Seconds(),

            // The analsysis could very easily include annotations with spectral bounds outside of a normalized set of
            // bounds defined by a fixed sample rate. Thus, do not normalize sample rate.
            AnalysisTargetSampleRate = null,
        };

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            Contract.Requires(segmentSettings.SegmentStartOffset == segmentSettings.Segment.StartOffsetSeconds.Seconds());

            var recording = new AudioRecording(segmentSettings.SegmentAudioFile);

            var segment = (RemoteSegmentWithData)segmentSettings.Segment;

            // sometimes events will share the same audio block so we have to analyze each event
            // within this segment of audio
            IReadOnlyCollection<object> importedEvents = segment.Data;

            Log.Debug($"Calculating event statistics for {importedEvents.Count} items in {segmentSettings.SegmentAudioFile}");
            EventStatistics[] results = new EventStatistics[importedEvents.Count];
            int index = 0;
            foreach (var importedEventObject in importedEvents)
            {
                var importedEvent = (ImportedEvent)importedEventObject;

                var temporalRange = new Range<TimeSpan>(
                    importedEvent.EventStartSeconds.Value.Seconds(),
                    importedEvent.EventEndSeconds.Value.Seconds());
                var spectralRange = new Range<double>(
                    importedEvent.LowFrequencyHertz.Value,
                    importedEvent.HighFrequencyHertz.Value);

                Log.Debug(
                    $"Calculating event statistics for {importedEvent.AudioEventId},{temporalRange}," +
                    $"{spectralRange} in {segmentSettings.SegmentAudioFile}, Duration: {recording.Duration}");

                var configuration = (EventStatisticsConfiguration)analysisSettings.Configuration;

                var statistics = EventStatisticsCalculate.AnalyzeAudioEvent(
                    recording,
                    temporalRange,
                    spectralRange,
                    configuration,
                    segmentSettings.SegmentStartOffset);

                // lastly add some metadata to make the results useful
                statistics.AudioRecordingId = segment.Source.Id;
                statistics.AudioRecordingRecordedDate = segment.SourceMetadata.RecordedDate;
                statistics.AudioEventId = importedEvent.AudioEventId;

                results[index] = statistics;
                index++;
            }

            var result = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            result.Events = results;

            return result;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<EventStatistics>());
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // noop
        }
    }
}
