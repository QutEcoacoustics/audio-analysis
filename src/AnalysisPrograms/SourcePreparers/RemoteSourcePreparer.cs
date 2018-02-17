// <copyright file="RemoteSourcePreparer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SourcePreparers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AcousticWorkbench;
    using AcousticWorkbench.Orchestration;
    using AnalysisBase;
    using AnalysisBase.Segment;
    using global::AcousticWorkbench;
    using global::AcousticWorkbench.Models;
    using log4net;

    /// <summary>
    /// Remote source file preparer.
    /// </summary>
    public class RemoteSourcePreparer : ISourcePreparer
    {
        private static readonly double[] RetryDelay = { 0.1, 0.5, 5.0 };
        private static readonly ILog Log = LogManager.GetLogger(nameof(RemoteSourcePreparer));
        private readonly ConcurrentBag<long> recievedSizeTracker = new ConcurrentBag<long>();
        private readonly bool allowSegmentcutting;
        private readonly MediaService mediaService;

        public RemoteSourcePreparer(IAuthenticatedApi authenticatedApi, bool allowSegmentcutting = true)
        {
            this.allowSegmentcutting = allowSegmentcutting;

            this.mediaService = new MediaService(authenticatedApi);
        }

        public long TotalBytesRecieved => this.recievedSizeTracker.Sum();

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file,
        /// modified based on the provided settings.
        /// </summary>
        /// <param name="outputDirectory">
        /// The analysis Base Directory.
        /// </param>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        /// The output Media Type.
        /// </param>
        /// <param name="startOffset">
        /// The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        /// The end Offset from start of entire original file.
        /// </param>
        /// <param name="targetSampleRateHz">
        /// The target Sample Rate Hz.
        /// </param>
        /// <returns>
        /// The prepared file. The returned FileSegment will have the targetFile and OriginalFileDuration set -
        /// these are the path to the segmented file and the duration of the segmented file.
        /// The start and end offsets will not be set.
        /// </returns>
        public async Task<FileSegment> PrepareFile(
            DirectoryInfo outputDirectory,
            string source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz)
        {
            throw new NotImplementedException();
            var request = new AudioUtilityRequest
            {
                OffsetStart = startOffset,
                OffsetEnd = endOffset,
                TargetSampleRate = targetSampleRateHz,
            };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                source.ToFileInfo(),
                outputMediaType,
                request,
                TempFileHelper.TempDir());

            return new FileSegment(
                preparedFile.TargetInfo.SourceFile,
                preparedFile.TargetInfo.SampleRate.Value,
                preparedFile.TargetInfo.Duration.Value);
        }

        /// <summary>
        /// Calculate the file segments for analysis.
        /// </summary>
        /// <param name="fileSegments">
        /// The file segments.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <returns>
        /// Enumerable of sub-segments.
        /// </returns>
        public IEnumerable<ISegment<TSource>> CalculateSegments<TSource>(
            IEnumerable<ISegment<TSource>> fileSegments,
            AnalysisSettings settings)
        {
            if (this.allowSegmentcutting)
            {
                foreach (var segment in fileSegments)
                {
                    if (!(segment is RemoteSegment))
                    {
                        throw new NotImplementedException(
                            $"{nameof(RemoteSourcePreparer)} only supports operating on {nameof(RemoteSegment)}");
                    }

                    var startOffset = segment.StartOffsetSeconds.Seconds();
                    var endOffset = segment.EndOffsetSeconds.Seconds();

                    var segmentDuration = endOffset - startOffset;

                    // segment into exact chunks - all but the last chunk will be equal to the max duration
                    var segments = AudioFilePreparer.DivideExactLeaveLeftoversAtEnd(
                        Convert.ToInt64(segmentDuration.TotalMilliseconds),
                        Convert.ToInt64(settings.AnalysisMaxSegmentDuration.Value.TotalMilliseconds));

                    var overlap = settings.SegmentOverlapDuration;
                    long aggregate = 0;

                    // yield each normal segment
                    foreach (long offset in segments)
                    {
                        yield return LocalSourcePreparer.CreateSegment(
                            ref aggregate,
                            offset,
                            segment,
                            startOffset,
                            endOffset,
                            overlap);
                    }
                }
            }
            else
            {
                foreach (var segment in fileSegments)
                {
                    var duration = (segment.EndOffsetSeconds - segment.StartOffsetSeconds).Seconds();
                    if (duration > settings.AnalysisMaxSegmentDuration.Value)
                    {
                        throw new SegmentSplitException(
                            $"Splitting segments has been disabled for" +
                            $" {nameof(RemoteSourcePreparer)}, cannot split {segment}");
                    }

                    yield return segment;
                }
            }
        }

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
        /// </summary>
        /// <param name="outputDirectory">
        ///     The analysis Base Directory.
        /// </param>
        /// <param name="source">
        ///     The source audio file.
        /// </param>
        /// <param name="outputMediaType">
        ///     The output Media Type.
        /// </param>
        /// <param name="targetSampleRateHz">
        ///     The target Sample Rate Hz.
        /// </param>
        /// <param name="temporaryFilesDirectory"></param>
        /// <param name="channelSelection"></param>
        /// <param name="mixDownToMono"></param>
        /// <exception cref="RemoteSourcePreparerException"></exception>
        /// <returns>
        /// The prepared file. The returned FileSegment will have the targetFile and OriginalFileDuration set -
        /// these are the path to the segmented file and the duration of the segmented file.
        /// The start and end offsets will not be set.
        /// </returns>
        public async Task<FileSegment> PrepareFile<TSource>(
            DirectoryInfo outputDirectory,
            ISegment<TSource> source,
            string outputMediaType,
            int? targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection,
            bool? mixDownToMono)
        {
            if (!(source is RemoteSegment segment))
            {
                throw new NotImplementedException(
                    $"{nameof(RemoteSourcePreparer)} only supports preparing files with {nameof(RemoteSegment)} sources");
            }

            if (channelSelection != null && channelSelection.Length > 1)
            {
                throw new NotSupportedException(
                    $"{nameof(RemoteSourcePreparer)} does not support multi channel selection");
            }

            var recording = segment.Source;
            var identifier = segment.SourceMetadata.Identifier;
            var filename = AudioFilePreparer.GetFileName(
                identifier,
                outputMediaType,
                source.StartOffsetSeconds.Seconds(),
                source.EndOffsetSeconds.Seconds());

            var destination = outputDirectory.CombineFile(filename);

            // channel values:
            // null - select all channels
            // 0 - mixdown all channels
            // n - select nth channel
            byte? channel = (mixDownToMono ?? false)
                ? 0
                : (channelSelection == null ? (byte?)null : (byte)channelSelection[0]);

            int attemptCount = 0;
            while (attemptCount < RetryDelay.Length)
            {
                if (attemptCount > 0)
                {
                    Log.Debug(
                        $"Attempting to download media: {recording.Id}, {segment.Offsets} - attempt {attemptCount}");

                    destination.TryDelete();
                }

                attemptCount++;

                try
                {
                    var length = await this.DownloadSegment(
                        source,
                        targetSampleRateHz,
                        recording,
                        segment,
                        channel,
                        destination);

                    // exit retry loop if successful
                    break;
                }
                catch (IOException ioex) when (ioex.InnerException is SocketException)
                {
                    Log.Warn($"Media download failed {recording.Id}, {segment.Offsets}", ioex);
                }
                catch (RemoteSourcePreparerException rspex) when (attemptCount < RetryDelay.Length)
                {
                    Log.Warn($"Media download failed {recording.Id}, {segment.Offsets}", rspex);
                }

                // back off and then try again
                await Task.Delay(RetryDelay[attemptCount].Seconds());
            }

            // finally inspect the bit of audio we downloaded, extract the metadata, and return a file segment
            var preparedFile = new FileSegment(destination, TimeAlignment.None);

            // do some sanity checks
            var expectedDuration = segment.Offsets.Size().Seconds();
            var durationDelta = expectedDuration - preparedFile.TargetFileDuration.Value;
            if (durationDelta > 1.0.Seconds())
            {
                Log.Warn(
                    $"Downloaded media ({recording.Id}, {segment.Offsets}) did not have expected duration."
                    + $" Expected: {expectedDuration}, Actual: {preparedFile.TargetFileDuration}");
            }

            return preparedFile;
        }

        private async Task<long> DownloadSegment<TSource>(
            ISegment<TSource> source,
            int? targetSampleRateHz,
            AudioRecording recording,
            RemoteSegment segment,
            byte? channel,
            FileInfo destination)
        {
            Log.Debug($"Downloading media: {recording.Id}, {segment.Offsets}");
            var (stream, contentLength) = await this.mediaService.DownloadMediaWave(
                recording.Id,
                source.StartOffsetSeconds,
                source.EndOffsetSeconds,
                targetSampleRateHz,
                channel);

            Log.Trace(
                $"Downloading media: {recording.Id}, {segment.Offsets} - headers received,"
                + $" body is {contentLength?.ToString() ?? "<unknown>"} bytes, writing stream to file {destination}");

            // The output file should never exist already - if it does there's something wrong with the program
            // or a previous run of the program left behind files.
            // This restriction is similar to that found in MasterAudioUtility
            if (destination.Exists)
            {
                Log.Warn($"RemoteSource preparer is trying to create file {destination} that already exists");
            }

            // purposely use FileMode.CreateNew to ensure target file does not exist already (or else throw)
            long length;
            using (var file = File.Open(destination.FullName, FileMode.CreateNew, FileAccess.Write))
            {
                await stream.CopyToAsync(file);
                length = file.Length;
            }

            Log.Trace(
                $"Downloading media: {recording.Id}, {segment.Offsets} - file received, "
                + $"{length} bytes written to file {destination}");

            if (length == 0)
            {
                throw new RemoteSourcePreparerException(
                    "Downloaded media has a content length of zero bytes. This means media download has failed."
                    + $"{recording.Id}, {segment.Offsets} (file: {destination})");
            }

            this.recievedSizeTracker.Add(length);

            return length;
        }
    }
}