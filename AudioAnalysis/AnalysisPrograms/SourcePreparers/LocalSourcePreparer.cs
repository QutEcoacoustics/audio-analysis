namespace AnalysisPrograms.SourcePreparers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisBase.Segment;
    using log4net;

    /// <summary>
    /// Local source file preparer.
    /// </summary>
    public class LocalSourcePreparer : ISourcePreparer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prepare an audio file. This will be a single segment of a larger audio file, modified based on the analysisSettings.
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
            return await TaskEx.Run(() =>
            {
                FileInfo sourceFileInfo = source.ToFileInfo();

                var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz,
                };
                var preparedFile = AudioFilePreparer.PrepareFile(
                    outputDirectory,
                    sourceFileInfo,
                    outputMediaType,
                    request,
                    TempFileHelper.TempDir());

                return new FileSegment(
                    preparedFile.TargetInfo.SourceFile,
                    preparedFile.TargetInfo.SampleRate.Value,
                    preparedFile.TargetInfo.Duration.Value);
            });
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
            foreach (var segment in fileSegments)
            {
                if (!(segment is FileSegment))
                {
                    throw new NotImplementedException("Anthony was too lazy to fix this properly. " +
                                                      "Adding support proper support for ISegment is difficult " +
                                                      "at this stage.");
                }

                var fileSegment = (FileSegment)segment;

                var startOffset = fileSegment.StartOffsetSeconds.Seconds();
                var endOffset = fileSegment.EndOffsetSeconds.Seconds();

                // process time alignment
                var startDelta = TimeSpan.Zero;
                var endDelta = TimeSpan.Zero;
                if (fileSegment.Alignment != TimeAlignment.None)
                {
                    // FileSegment should have already verified a date will be present
                    // ReSharper disable once PossibleInvalidOperationException
                    var startDate = fileSegment.TargetFileStartDate.Value.ToUniversalTime();

                    // if there's a zero second to the time
                    if (startDate.TimeOfDay.Seconds == 0 && startDate.TimeOfDay.Milliseconds == 0)
                    {
                        // then do nothing
                        Log.Debug("TimeAlignment ignored because start date is already aligned");
                    }
                    else
                    {
                        // calculate the delta to the next minute
                        // 1:23:45, startOffset = 15
                        // 1:38:45 - start date with offset
                        // 1:39:00 - next minute
                        var dateWithStartOffset = startDate.Add(startOffset);
                        var nextMinute = dateWithStartOffset.Ceiling(TimeSpan.FromMinutes(1));
                        startDelta = nextMinute - dateWithStartOffset;

                        var dateWithEndOffset = startDate.Add(endOffset);
                        var lastMinute = dateWithEndOffset.Floor(TimeSpan.FromMinutes(1));
                        endDelta = dateWithEndOffset - lastMinute;
                    }
                }

                // the rest of the duration (excluding the start and end fractions from the time alignment)
                var fileSegmentDuration = (endOffset - startOffset - startDelta - endDelta).TotalMilliseconds;

                var analysisSegmentMaxDuration = settings.AnalysisMaxSegmentDuration?.TotalMilliseconds ?? fileSegmentDuration;

                var analysisSegmentMinDuration = settings.AnalysisMinSegmentDuration.TotalMilliseconds;

                // segment into exact chunks - all but the last chunk will be equal to the max duration
                var segments = AudioFilePreparer.DivideExactLeaveLeftoversAtEnd(
                    Convert.ToInt64(fileSegmentDuration),
                    Convert.ToInt64(analysisSegmentMaxDuration));

                var overlap = settings.SegmentOverlapDuration;
                long aggregate = 0;

                // include fractional segment cut from time alignment
                if ((fileSegment.Alignment == TimeAlignment.TrimEnd
                     || fileSegment.Alignment == TimeAlignment.TrimNeither) && startDelta > TimeSpan.Zero)
                {
                    Log.Debug($"Generated fractional segment for time alignment ({startOffset} - {startOffset + startDelta})");
                    var startAlignDelta = Convert.ToInt64(startDelta.TotalMilliseconds);
                    yield return
                        (ISegment<TSource>)CreateSegment(ref aggregate, startAlignDelta, fileSegment, startOffset, endOffset, overlap);
                }
                else
                {
                    // advance the counter but don't produce the first segment
                    aggregate += Convert.ToInt64(startDelta.TotalMilliseconds);
                }

                // yield each normal segment
                foreach (long offset in segments)
                {
                    yield return
                        (ISegment<TSource>)CreateSegment(ref aggregate, offset, fileSegment, startOffset, endOffset, overlap);
                }

                // include fractional segment cut from time alignment
                if ((fileSegment.Alignment == TimeAlignment.TrimStart
                     || fileSegment.Alignment == TimeAlignment.TrimNeither) && startDelta > TimeSpan.Zero)
                {
                    Log.Debug($"Generated fractional segment for time alignment ({endOffset - endDelta} - {endOffset})");
                    var endAlignDelta = Convert.ToInt64(endDelta.TotalMilliseconds);
                    yield return
                        (ISegment<TSource>)CreateSegment(ref aggregate, endAlignDelta, fileSegment, startOffset, endOffset, overlap);
                }
            }
        }

        internal static ISegment<TSource> CreateSegment<TSource>(
            ref long aggregate,
            long offset,
            ISegment<TSource> currentSegment,
            TimeSpan startOffset,
            TimeSpan endOffset,
            TimeSpan overlap)
        {
            var newStart = startOffset.Add(TimeSpan.FromMilliseconds(aggregate));

            aggregate += offset;

            var segmentEndOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate));

            // include overlap
            segmentEndOffset = segmentEndOffset.Add(overlap);

            // don't allow overflow past desired end point
            if (segmentEndOffset > endOffset)
            {
                segmentEndOffset = endOffset;
            }

            var newEnd = segmentEndOffset;

            // So we aren't actually cutting any files, rather we're preparing to cut files.
            // Thus the clone the object and set new offsets.
            return currentSegment.SplitSegment(newStart.TotalSeconds, newEnd.TotalSeconds);
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
            return await TaskEx.Run(() =>
            {
                if (!(source is FileSegment segment))
                {
                    throw new NotSupportedException($"{nameof(LocalSourcePreparer)} only knows how to access {nameof(FileSegment)} types");
                }

                var request = new AudioUtilityRequest
                {
                    OffsetStart = segment.StartOffsetSeconds.Seconds(),
                    OffsetEnd = segment.EndOffsetSeconds.Seconds(),
                    TargetSampleRate = targetSampleRateHz,
                    MixDownToMono = mixDownToMono,
                    Channels = channelSelection,
                };
                var preparedFile = AudioFilePreparer.PrepareFile(
                    outputDirectory,
                    segment.Source,
                    outputMediaType,
                    request,
                    temporaryFilesDirectory);

                return new FileSegment(
                    preparedFile.TargetInfo.SourceFile,
                    preparedFile.TargetInfo.SampleRate.Value,
                    preparedFile.TargetInfo.Duration.Value);
            });
        }
    }
}