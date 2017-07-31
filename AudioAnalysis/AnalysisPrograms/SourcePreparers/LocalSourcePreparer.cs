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
    using AnalysisBase.SegmentAnalysis;
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
            return await new Task<FileSegment>(() =>
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
            var audioUtility = new MasterAudioUtility();

            var defaultAnalysisSegmentMinDuration = TimeSpan.FromSeconds(10);

            foreach (var segment in fileSegments)
            {
                if (!(segment is FileSegment))
                {
                    throw new NotImplementedException("Anthony was too lazy to fix this properly. " +
                                                      "Adding support proper support for ISegment is difficult " +
                                                      "at this stage.");
                }

                var fileSegment = (FileSegment)segment;

                var mediaType = MediaTypes.GetMediaType(fileSegment.TargetFile.Extension);
                var info = audioUtility.Info(fileSegment.TargetFile);

                var startOffset = fileSegment.SegmentStartOffset ?? TimeSpan.Zero;
                var endOffset = fileSegment.SegmentEndOffset ?? info.Duration.Value;
                fileSegment.TargetFileDuration = info.Duration.Value;
                fileSegment.TargetFileSampleRate = info.SampleRate.Value;

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

                var analysisSegmentMinDuration = settings.AnalysisMinSegmentDuration?.TotalMilliseconds
                                                 ?? defaultAnalysisSegmentMinDuration.TotalMilliseconds;

                // --------------------------------------------
                // divide duration to get evenly-sized segments
                // use the max duration to divide up the range
                // this is the number of segments required to not go over the max duration
                double analysisSegmentsForMaxSize = Math.Ceiling(fileSegmentDuration / analysisSegmentMaxDuration);

                // get the segment durations
                // segment evenly - each chunk will be equal or smaller than max, and equal or greater than min.
                ////var segments = AudioFilePreparer.DivideEvenly(Convert.ToInt64(fileSegmentDuration), Convert.ToInt64(analysisSegmentsForMaxSize)).ToList();

                // --------------------------------------------
                // --OR--
                // --------------------------------------------
                // segment into exact chunks - all but the last chunk will be equal to the max duration
                var segments = AudioFilePreparer.DivideExactLeaveLeftoversAtEnd(
                    Convert.ToInt64(fileSegmentDuration),
                    Convert.ToInt64(analysisSegmentMaxDuration));

                // --------------------------------------------

                var overlap = settings.SegmentSettings.SegmentOverlapDuration;
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

        private static FileSegment CreateSegment(
            ref long aggregate,
            long offset,
            FileSegment fileSegment,
            TimeSpan startOffset,
            TimeSpan endOffset,
            TimeSpan overlap)
        {
            // So we aren't actually cutting any files, rather we're preparing to cut files.
            // Thus the clone the object and set new offsets.
            var currentSegment = (FileSegment)fileSegment.Clone();

            currentSegment.SegmentStartOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate));

            aggregate += offset;

            var segmentEndOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate));

            // include overlap
            segmentEndOffset = segmentEndOffset.Add(overlap);
            // don't allow overflow past desired end point
            if (segmentEndOffset > endOffset)
            {
                segmentEndOffset = endOffset;

            }
            currentSegment.SegmentEndOffset = segmentEndOffset;

            return currentSegment;
        }

        /// <summary>
        /// Get the source files based on analysis <paramref name="analysisSettings"/>.
        /// </summary>
        /// <param name="analysisSettings">
        ///   The analysis settings.
        /// </param>
        /// <param name="fileSegments">
        /// File segments to create.
        /// </param>
        /// <returns>
        /// Enumerable of source files.
        /// </returns>
        private IEnumerable<FileInfo> PrepareFiles(
            AnalysisSettings analysisSettings,
            IEnumerable<FileSegment> fileSegments)
        {
            var audioUtility = new MasterAudioUtility();

            foreach (var fileSegment in fileSegments)
            {
                var mediaType = MediaTypes.GetMediaType(fileSegment.TargetFile.Extension);
                var info = audioUtility.Info(fileSegment.TargetFile);

                var startOffset = fileSegment.SegmentStartOffset.HasValue
                                      ? fileSegment.SegmentStartOffset.Value
                                      : TimeSpan.Zero;
                var endOffset = fileSegment.SegmentEndOffset.HasValue
                                    ? fileSegment.SegmentEndOffset.Value
                                    : info.Duration.Value;

                var fileSegmentDuration = (endOffset - startOffset).TotalMilliseconds;
                double currentPostion = 0;

                var offsetsFromEntireFile = new List<Range<TimeSpan>>();

                var segmentMaxDuration = analysisSettings.AnalysisMaxSegmentDuration.HasValue
                                             ? analysisSettings.AnalysisMaxSegmentDuration.Value.TotalMilliseconds
                                             : fileSegmentDuration;
                var segmentMinDuration = analysisSettings.AnalysisMinSegmentDuration.HasValue
                                             ? analysisSettings.AnalysisMinSegmentDuration.Value
                                             : TimeSpan.Zero;

                while (currentPostion < fileSegmentDuration)
                {
                    var start = currentPostion - analysisSettings.SegmentSettings.SegmentOverlapDuration.TotalMilliseconds;
                    start = Math.Max(start, 0);

                    var end = currentPostion + segmentMaxDuration;
                    end = Math.Min(end, fileSegmentDuration);

                    offsetsFromEntireFile.Add(
                        new Range<TimeSpan>(
                                TimeSpan.FromMilliseconds(start) + startOffset,
                                TimeSpan.FromMilliseconds(end) + startOffset));

                    currentPostion = end;
                }

                // make sure last segment is at least segmentMinDuration long if possible.
                var last = offsetsFromEntireFile.Last();
                if (offsetsFromEntireFile.Count > 1 && last.Maximum - last.Minimum < segmentMinDuration)
                {
                    var secondLast = offsetsFromEntireFile.Skip(offsetsFromEntireFile.Count - 2).First();

                    ////var totalDuration = (secondLast.Maximum - secondLast.Minimum) + (last.Upper - last.Lower);
                    var newLast = new Range<TimeSpan>(endOffset - segmentMinDuration, endOffset);
                    var newSecondLast = new Range<TimeSpan>(secondLast.Minimum, newLast.Minimum);

                    offsetsFromEntireFile[offsetsFromEntireFile.Count - 1] = newLast;
                    offsetsFromEntireFile[offsetsFromEntireFile.Count - 2] = newSecondLast;
                }

                // segment and/or convert the file segment to match settings
                foreach (var offset in offsetsFromEntireFile)
                {
                    var filename = string.Format(
                        "{0}_{1}_{2}.{3}",
                        fileSegment.TargetFile.Name,
                        offset.Minimum.TotalMilliseconds,
                        offset.Maximum.TotalMilliseconds,
                        MediaTypes.GetExtension(analysisSettings.SegmentSettings.SegmentMediaType));

                    var path =
                        new FileInfo(
                            Path.Combine(
                                analysisSettings.AnalysisOutputDirectory.FullName,
                                "segmentedaudio",
                                filename));

                    if (!File.Exists(path.FullName))
                    {
                        audioUtility.Modify(
                            fileSegment.TargetFile,
                            mediaType,
                            path,
                            analysisSettings.SegmentSettings.SegmentMediaType,
                            new AudioUtilityRequest
                                {
                                    OffsetStart = offset.Minimum,
                                    OffsetEnd = offset.Maximum,
                                    TargetSampleRate = analysisSettings.AnalysisTargetSampleRate,
                                });
                    }

                    yield return path;
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
        /// <param name="startOffset">
        ///     The start Offset from start of entire original file.
        /// </param>
        /// <param name="endOffset">
        ///     The end Offset from start of entire original file.
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
        public async Task<FileSegment> PrepareFile(
            DirectoryInfo outputDirectory,
            string source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection = null,
            bool? mixDownToMono = null)
        {
            return await new Task<FileSegment>(() =>
            {
                var sourceFileInfo = source.ToFileInfo();

                var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz,
                    MixDownToMono = mixDownToMono,
                    Channels = channelSelection,
                };
                var preparedFile = AudioFilePreparer.PrepareFile(
                    outputDirectory,
                    sourceFileInfo,
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