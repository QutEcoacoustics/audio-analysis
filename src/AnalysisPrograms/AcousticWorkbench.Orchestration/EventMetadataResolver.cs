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
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventMetadataResolver));
        private readonly AudioRecordingService audioRecordingService;
        private readonly AcousticEventService acousticEventService;
        private readonly int maxDegreeOfParallelism;

        public EventMetadataResolver(IAuthenticatedApi api, Func<double, double> analysisDurationSeconds, int parallelism)
        {
            this.AnalysisDurationSeconds = analysisDurationSeconds;
            this.audioRecordingService = new AudioRecordingService(api);
            this.acousticEventService = new AcousticEventService(api);
            this.maxDegreeOfParallelism = parallelism;
        }

        public Func<double, double> AnalysisDurationSeconds { get; }

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

            var groupedEvents = new ConcurrentDictionary<long, ConcurrentBag<ImportedEvent>>(
                this.maxDegreeOfParallelism,
                events.Length / 10);

            // execution options allows us to throttle requests
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = this.maxDegreeOfParallelism,
                SingleProducerConstrained = true,
            };

            // the transform block maps A->B, ensuring that all events have an audio recording id
            var getRecordingIdBlock = new TransformBlock<ImportedEvent, ImportedEvent>(
                (importedEvent) => this.GetAudioRecordingId(importedEvent),
                options);

            // all events are buffered into groups based on audio recording id
            var groupRecordingsBlock = new ActionBlock<ImportedEvent>(
                importedEvent =>
                {
                    var collection = groupedEvents.GetOrAdd(
                        importedEvent.AudioRecordingId.Value,
                        new ConcurrentBag<ImportedEvent>());

                    collection.Add(importedEvent);
                });

            // the metadata for each recording is retrieved and used to produce many segments (one per imported event)
            var createSegmentsBlock = new TransformManyBlock<KeyValuePair<long, ConcurrentBag<ImportedEvent>>, RemoteSegmentWithData>(
                (group) => this.DownloadRemoteMetadata(group.Key, group.Value),
                options);

            // the transform block can't `Complete` unless it's output is empty
            // so add a buffer block to store the transform block's output
            var bufferBlock = new BufferBlock<RemoteSegmentWithData>();

            // link the two parts of block A
            getRecordingIdBlock.LinkTo(groupRecordingsBlock);

            // link the two parts of block B
            createSegmentsBlock.LinkTo(bufferBlock);

            // kick off the chain, resolve audio recording ids and group
            foreach (var record in events)
            {
                // post an event to the transform block to process
                getRecordingIdBlock.Post(record);
            }

            Log.Trace("Finished posting messages to recording id resolver");
            getRecordingIdBlock.Complete();

            Log.Trace("Waiting for getRecordingIdBlock to resolve");
            await getRecordingIdBlock.Completion;
            Log.Trace("Waiting for groupRecordingsBlock to resolve");
            groupRecordingsBlock.Complete();
            await groupRecordingsBlock.Completion;

            var eventCount = groupedEvents.Sum(kvp => kvp.Value.Count);
            Log.Trace($"Finished waiting for recording ids to resolve, {eventCount} events grouped into {groupedEvents.Count} recordings");

            // now post the grouped audio recordings to the segment generating block
            foreach (var keyValuePair in groupedEvents)
            {
                createSegmentsBlock.Post(keyValuePair);
            }

            Log.Trace("Finished posting messages to recording metadata downloader");
            createSegmentsBlock.Complete();

            // wait for all requests to finish
            Log.Trace("Begin waiting for metadata downloader");
            await createSegmentsBlock.Completion;
            Log.Trace("Finished waiting for metadata downloader");

            if (bufferBlock.TryReceiveAll(out var segments))
            {
                RemoteSegmentWithData[] segmentsArray;
                int finalEventCount;
                lock (segments)
                {
                    segmentsArray = segments.ToArray();

                    // do some excessive logic checking because we used to have race conditions
                    finalEventCount = segmentsArray.Sum(x => x.Data.Count);
                    if (events.Length != finalEventCount)
                    {
                        throw new InvalidOperationException(
                            $"The number of supplied events ({events.Length}) did" +
                            $" not match the number of events that had metadata resolved ({finalEventCount})" +
                            " - a race condition has occurred");
                    }
                }

                Log.Info($"Metadata generated for {finalEventCount} events, {segmentsArray.Length} segments created");

                return segmentsArray;
            }
            else
            {
                throw new InvalidOperationException("Failed to retrieve media info from data flow.");
            }
        }

        private async Task<IEnumerable<RemoteSegmentWithData>> DownloadRemoteMetadata(
            long audioRecordingId,
            IEnumerable<ImportedEvent> events)
        {
            // now download the metadata for the audio recording

            Log.Debug($"Requesting metadata for audio recording media {audioRecordingId}");
            var audioRecording = await this.audioRecordingService.GetAudioRecording(audioRecordingId);
            Log.Trace(
                $"Metadata for audio recording media {audioRecordingId} retrieved, generating segments for associated events");

            // now generate the segments
            // we need to floor the duration to ensure later rounding does round past the limit of the recording
            Log.Verbose($"Audio recording duration {audioRecording.DurationSeconds} will be floored");
            var limit = audioRecording.DurationSeconds.Floor().AsRangeFromZero();
            var results = new List<RemoteSegmentWithData>(20);
            foreach (var importedEvent in events)
            {
                var target = (importedEvent.EventStartSeconds.Value, importedEvent.EventEndSeconds.Value).AsRange();

                // determine how much padding is required (dynamically scales with event size
                var padding = this.AnalysisDurationSeconds(target.Size());

                if (target.Size() + padding > MediaService.MediaDownloadMaximumSeconds)
                {
                    var newPadding = MediaService.MediaDownloadMaximumSeconds - target.Size();
                    Log.Warn(
                        $"Audio event size {audioRecordingId},{target} and padding {padding} exceeds maximum media " +
                        $"download amount - trimming padding from {padding}, to {newPadding}");
                    padding = newPadding;
                }

                // grow target to required analysis length
                // we round the grow size to the nearest integer so that we reduce caching combinatorics in the workbench
                Log.Trace($"Growing target event from {audioRecordingId},{target} by {padding} seconds");
                var analysisRange = target.Grow(limit, padding, 0);

                var segment = new RemoteSegmentWithData(audioRecording, analysisRange, importedEvent.AsArray());

                results.Add(segment);
            }

            return DedupeSegments(results);
        }

        private async Task<ImportedEvent> GetAudioRecordingId(ImportedEvent importedEvent)
        {
            if (!importedEvent.AudioRecordingId.HasValue)
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

                Log.Trace($"Metadata for audio event {audioEventId} retrieved");

                importedEvent.AudioRecordingId = audioEvent.AudioRecordingId;
            }

            return importedEvent;
        }
    }
}
