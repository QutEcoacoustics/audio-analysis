using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Acoustics.Shared;
using Acoustics.Tools;
using Acoustics.Tools.Audio;
using AnalysisBase;

namespace AnalysisPrograms.SourcePreparers
{
    using System.Reflection;

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
        public FileSegment PrepareFile(
            DirectoryInfo outputDirectory,
            FileInfo source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz)
        {
            var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz
                };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                source,
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
        public IEnumerable<FileSegment> CalculateSegments(
            IEnumerable<FileSegment> fileSegments,
            AnalysisSettings settings)
        {
            var audioUtility = new MasterAudioUtility();

            var defaultAnalysisSegmentMinDuration = TimeSpan.FromSeconds(10);

            foreach (var fileSegment in fileSegments)
            {
                var mediaType = MediaTypes.GetMediaType(fileSegment.TargetFile.Extension);
                var info = audioUtility.Info(fileSegment.TargetFile);

                var startOffset = fileSegment.SegmentStartOffset ?? TimeSpan.Zero;
                var endOffset = fileSegment.SegmentEndOffset ?? info.Duration.Value;
                fileSegment.TargetFileDuration = info.Duration.Value;
                fileSegment.TargetFileSampleRate = info.SampleRate.Value;

                // process time alignment
                if (fileSegment.Alignment != TimeAlignment.None)
                {
                    // FileSegment should have already verified a date will be present
                    // ReSharper disable once PossibleInvalidOperationException
                    var startDate = fileSegment.TargetFileStartDate.Value;

                    // if there's a zero second to the time
                    if (startDate.TimeOfDay.Seconds == 0 && startDate.TimeOfDay.Milliseconds == 0)
                    {
                        // then do nothing
                        Log.Debug("TimeAlignment ignored because start date is already aligned");
                    }
                    else
                    {
                        // calculate the delta to the next minute
                        var nextMinute = startDate.Date.AddHours(startDate.Hour).AddMinutes(startDate.Minute);
                        var delta = nextMinute - startDate;

                        // advance the start offset by the delta
                        startOffset += delta;
                        // TODO: BROKEN!!!!!!!!!!!!!!!!!!!!!!
                    }
                }

                var fileSegmentDuration = (endOffset - startOffset).TotalMilliseconds;

                var analysisSegmentMaxDuration = settings.SegmentMaxDuration?.TotalMilliseconds ?? fileSegmentDuration;

                var analysisSegmentMinDuration = settings.SegmentMinDuration?.TotalMilliseconds
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
                var segments =
                    AudioFilePreparer.DivideExactLeaveLeftoversAtEnd(
                        Convert.ToInt64(fileSegmentDuration),
                        Convert.ToInt64(analysisSegmentMaxDuration)).ToList();

                // --------------------------------------------
                long aggregate = 0;

                for (var index = 0; index < segments.Count; index++)
                {
                    // So we aren't actually cutting any files, rather we're preparing to cut files.
                    // Thus the clone the object and set new offsets.
                    var currentSegment = (FileSegment)fileSegment.Clone();

                    currentSegment.SegmentStartOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate));

                    aggregate += segments[index];

                    // include overlap
                    var segmentEndOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate)).Add(settings.SegmentOverlapDuration);
                    // don't allow overflow past desired end point
                    if (segmentEndOffset > endOffset)
                    {
                        segmentEndOffset = endOffset;
                    }
                    currentSegment.SegmentEndOffset = segmentEndOffset;

                    yield return currentSegment;
                }
            }
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

                var segmentMaxDuration = analysisSettings.SegmentMaxDuration.HasValue
                                             ? analysisSettings.SegmentMaxDuration.Value.TotalMilliseconds
                                             : fileSegmentDuration;
                var segmentMinDuration = analysisSettings.SegmentMinDuration.HasValue
                                             ? analysisSettings.SegmentMinDuration.Value
                                             : TimeSpan.Zero;

                while (currentPostion < fileSegmentDuration)
                {
                    var start = currentPostion - analysisSettings.SegmentOverlapDuration.TotalMilliseconds;
                    start = Math.Max(start, 0);

                    var end = currentPostion + segmentMaxDuration;
                    end = Math.Min(end, fileSegmentDuration);

                    offsetsFromEntireFile.Add(
                        new Range<TimeSpan>
                            {
                                Minimum = TimeSpan.FromMilliseconds(start) + startOffset,
                                Maximum = TimeSpan.FromMilliseconds(end) + startOffset
                            });

                    currentPostion = end;
                }

                // make sure last segment is at least segmentMinDuration long if possible.
                var last = offsetsFromEntireFile.Last();
                if (offsetsFromEntireFile.Count > 1 && last.Maximum - last.Minimum < segmentMinDuration)
                {
                    var secondLast = offsetsFromEntireFile.Skip(offsetsFromEntireFile.Count - 2).First();

                    ////var totalDuration = (secondLast.Maximum - secondLast.Minimum) + (last.Maximum - last.Minimum);
                    var newLast = new Range<TimeSpan> { Maximum = endOffset, Minimum = endOffset - segmentMinDuration };
                    var newSecondLast = new Range<TimeSpan> { Maximum = newLast.Minimum, Minimum = secondLast.Minimum };

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
                        MediaTypes.GetExtension(analysisSettings.SegmentMediaType));

                    var path =
                        new FileInfo(
                            Path.Combine(
                                analysisSettings.AnalysisBaseOutputDirectory.FullName,
                                "segmentedaudio",
                                filename));

                    if (!File.Exists(path.FullName))
                    {
                        audioUtility.Modify(
                            fileSegment.TargetFile,
                            mediaType,
                            path,
                            analysisSettings.SegmentMediaType,
                            new AudioUtilityRequest
                                {
                                    OffsetStart = offset.Minimum,
                                    OffsetEnd = offset.Maximum,
                                    TargetSampleRate = analysisSettings.SegmentTargetSampleRate
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
        public FileSegment PrepareFile(
            DirectoryInfo outputDirectory,
            FileInfo source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection = null,
            bool? mixDownToMono = null)
        {
            var request = new AudioUtilityRequest
                {
                    OffsetStart = startOffset,
                    OffsetEnd = endOffset,
                    TargetSampleRate = targetSampleRateHz,
                    MixDownToMono = mixDownToMono,
                    Channels = channelSelection
                };
            var preparedFile = AudioFilePreparer.PrepareFile(
                outputDirectory,
                source,
                outputMediaType,
                request,
                temporaryFilesDirectory);

            return new FileSegment(
                preparedFile.TargetInfo.SourceFile,
                preparedFile.TargetInfo.SampleRate.Value,
                preparedFile.TargetInfo.Duration.Value);
        }
    }
}