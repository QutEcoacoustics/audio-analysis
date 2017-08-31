// <copyright file="EventMetadataResolver.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Acoustics.Shared;
    using AnalysisBase.Segment;
    using EventStatistics;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using log4net;

    public class EventMetadataResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EventMetadataResolver));
        private readonly AudioRecordingService audioRecordingService;
        private readonly AcousticEventService acousticEventService;
        private readonly int maxDegreeOfParallelism;

        public EventMetadataResolver(IAuthenticatedApi api, double analysisDurationSeconds, int parallelism)
        {
            this.AnalysisDurationSeconds = analysisDurationSeconds;
            this.audioRecordingService = new AudioRecordingService(api);
            this.acousticEventService = new AcousticEventService(api);
            this.maxDegreeOfParallelism = parallelism;
        }

        public double AnalysisDurationSeconds { get; }

        public async Task<RemoteSegment[]> GetRemoteMetadata(
            ImportedEvent[] events)
        {
            // asynchronously start downloading all the metadata we need
            Log.Info("Begin downloading metadata for segments");
            var downloader = new TransformBlock<ImportedEvent, RemoteSegment>(
                (importedEvent) => this.DownloadRemoteMetadata(importedEvent),
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = this.maxDegreeOfParallelism });

            foreach (var record in events)
            {
                // post an event to the transform block to process
                downloader.Post(record);
            }

            // the dataflow should not accept any more messages
            downloader.Complete();

            // wait for all requests to finish
            await downloader.Completion;

            if (downloader.TryReceiveAll(out var segments))
            {
                return segments.ToArray();
            }
            else
            {
                throw new InvalidOperationException("Failed to retrieve media info from data flow.");
            }
        }

        private async Task<RemoteSegment> DownloadRemoteMetadata(ImportedEvent importedEvent)
        {
            long audioRecordingId;
            if (importedEvent.AudioRecordingId.HasValue)
            {
                audioRecordingId = importedEvent.AudioRecordingId.Value;
            }
            else
            {
                long audioEventId = importedEvent.AudioEventId.Value;
                Log.Debug($"Requesting metadata for audio event {audioEventId}");
                var audioEvent = await this.acousticEventService.GetAudioEvent(audioEventId);
                audioRecordingId = audioEvent.AudioRecordingId;
                Log.Trace($"Metadata for audio event {audioEventId} retrieved");

                importedEvent.AudioRecordingId = audioRecordingId;
            }

            // now download the metadata for the media
            // TODO: concurrent cache?
            Log.Debug($"Requesting metadata for audio recording media {audioRecordingId}");
            var audioRecording = await this.audioRecordingService.GetAudioRecording(audioRecordingId);
            Log.Trace($"Metadata for audio recording media {audioRecordingId} retrieved");

            var limit = audioRecording.DurationSeconds.AsRangeFromZero();
            var target = (importedEvent.EventStartSeconds.Value, importedEvent.EventEndSeconds.Value).AsRange();

            // grow target to required analysis length
            var analysisRange = target.Grow(limit, this.AnalysisDurationSeconds);

            var segment = new RemoteSegmentWithDatum(audioRecording, analysisRange, importedEvent);

            return segment;
        }
    }
}
