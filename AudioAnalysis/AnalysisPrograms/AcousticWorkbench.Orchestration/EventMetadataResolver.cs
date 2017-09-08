// <copyright file="EventMetadataResolver.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.AcousticWorkbench.Orchestration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
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

        public static RemoteSegmentWithData[] DedupeSegments(IList<RemoteSegmentWithData> segments)
        {
            // ugggh don't really need a dictionary but HashSet doesn't allow retrieveal of values
            var unique = new Dictionary<RemoteSegment, RemoteSegmentWithData>(segments.Count);

            foreach (var segment in segments)
            {
                var exists = unique.ContainsKey(segment);

                if (exists)
                {
                    var current = unique[segment];

                    Contract.Ensures(unique.Remove(segment));

                    var data = current.Data.Concat(segment.Data).ToArray();

                    var combined = new RemoteSegmentWithData(current.Source, current.Offsets, data);

                    unique.Add(combined, combined);
                }
                else
                {
                    unique.Add(segment, segment);
                }
            }

            return unique.Values.ToArray();
        }

        public async Task<RemoteSegmentWithData[]> GetRemoteMetadata(ImportedEvent[] events)
        {
            // asynchronously start downloading all the metadata we need
            Log.Info($"Begin downloading metadata for segments (Request concurrency: {this.maxDegreeOfParallelism})");

            // the transform block maps A -> B, and allows us to throttle requests
            var downloader = new TransformBlock<ImportedEvent, RemoteSegmentWithData>(
                (importedEvent) => this.DownloadRemoteMetadata(importedEvent),
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = this.maxDegreeOfParallelism });

            // the transform block can't `Complete` unless it's output is empty
            // so add a buffer block to store the transform block's output
            var buffer = new BufferBlock<RemoteSegmentWithData>();
            downloader.LinkTo(buffer);

            foreach (var record in events)
            {
                // post an event to the transform block to process
                downloader.Post(record);
            }

            // the dataflow should not accept any more messages
            Log.Trace("Finished posting messages to metadata downloader");
            downloader.Complete();

            // wait for all requests to finish
            Log.Trace("Begin waiting for metadata downloader");
            await downloader.Completion;
            Log.Trace("Finished waiting for metadata downloader");

            if (buffer.TryReceiveAll(out var segments))
            {
                return DedupeSegments(segments);
            }
            else
            {
                throw new InvalidOperationException("Failed to retrieve media info from data flow.");
            }
        }

        private async Task<RemoteSegmentWithData> DownloadRemoteMetadata(ImportedEvent importedEvent)
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
                AudioEvent audioEvent;
                try
                {
                    audioEvent = await this.acousticEventService.GetAudioEvent(audioEventId);
                }
                catch (Service.HttpResponseException exception)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    Log.Error(exception);
                    throw;
                }

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
            // we round the grow size to the nearest integer so that we reduce caching combinatorics in the workbench
            var analysisRange = target.Grow(limit, this.AnalysisDurationSeconds, 0);

            var segment = new RemoteSegmentWithData(audioRecording, analysisRange, importedEvent.AsArray());

            return segment;
        }
    }
}
