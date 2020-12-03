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
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.AcousticWorkbench.Orchestration;
    using AudioAnalysisTools.EventStatistics;
    using AudioAnalysisTools.WavTools;
    using global::AcousticWorkbench;
    using log4net;

    public class EventStatisticsAnalysis : AbstractStrongAnalyser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventStatisticsAnalysis));

        public override string DisplayName { get; } = "Event statistics calculation";

        public override string Identifier { get; } = "Ecosounds.EventStatistics";

        public override string Description { get; } = "Calculates useful statistics (features) from an acoustic event";

        public override AnalysisSettings DefaultSettings { get; } = new AnalysisSettings
        {
            // The workbench supports extracting as much as 5 minutes of data at a time
            AnalysisMaxSegmentDuration = MediaService.MediaDownloadMaximumSeconds.Seconds(),
            AnalysisMinSegmentDuration = MediaService.MediaDownloadMinimumSeconds.Seconds(),

            // The analsysis could very easily include annotations with spectral bounds outside of a normalized set of
            // bounds defined by a fixed sample rate. Thus, do not normalize sample rate.
            AnalysisTargetSampleRate = null,
        };

        public override Status Status => Status.Maintained;

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<EventStatisticsConfiguration>(file);
        }

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

                var temporalRange = new Interval<TimeSpan>(
                    importedEvent.EventStartSeconds.Value.Seconds(),
                    importedEvent.EventEndSeconds.Value.Seconds());
                var spectralRange = new Interval<double>(
                    importedEvent.LowFrequencyHertz.Value,
                    importedEvent.HighFrequencyHertz.Value);

                Log.Debug(
                    $"Calculating event statistics for {importedEvent.AudioEventId},{temporalRange}," +
                    $"{spectralRange} in {segmentSettings.SegmentAudioFile}, Duration: {recording.Duration}");

                // Repeat sanity check here. Previous duration sanity check only checks the header of the audio file,
                // but that still allows for a fragmented audio file to have been downloaded, shorter than it should be
                var expectedDuration = segment.Offsets.Size().Seconds();
                var durationDelta = expectedDuration - recording.Duration;
                if (durationDelta > 1.0.Seconds())
                {
                    Log.Warn(
                        $"Media ({segmentSettings.SegmentAudioFile}) did not have expected duration."
                        + $" Expected: {expectedDuration}, Actual: {recording.Duration}");
                }

                var configuration = (EventStatisticsConfiguration)analysisSettings.Configuration;

                var statistics = EventStatisticsCalculate.AnalyzeAudioEvent(
                    recording,
                    temporalRange,
                    spectralRange,
                    configuration,
                    segmentSettings.SegmentStartOffset);

                if (statistics.Error)
                {
                    Log.Warn($"Event statistics failed for {importedEvent.AudioEventId},{temporalRange}," +
                             $"{spectralRange} in {segmentSettings.SegmentAudioFile}, Duration: {recording.Duration}");
                }

                // lastly add some metadata to make the results useful
                statistics.Order = importedEvent.Order;
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