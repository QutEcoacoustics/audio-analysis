namespace AnalysisRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Acoustics.Shared;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
using Acoustics.Tools;

    /// <summary>
    /// Local source file preparer.
    /// </summary>
    public class LocalSourcePreparer : ISourcePreparer
    {
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
        /// The prepared file.
        /// </returns>
        public FileInfo PrepareFile(DirectoryInfo outputDirectory, FileInfo source, string outputMediaType, TimeSpan startOffset, TimeSpan endOffset, int targetSampleRateHz)
        {
            return AudioFilePreparer.PrepareFile(outputDirectory, source, outputMediaType, startOffset, endOffset, targetSampleRateHz);
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
        /// Enumerable of file segments.
        /// </returns>
        public IEnumerable<FileSegment> CalculateSegments(IEnumerable<FileSegment> fileSegments, AnalysisSettings settings)
        {
            var audioUtility = this.GetNewAudioUtility(settings.SegmentTargetSampleRate);

            var defaultAnalysisSegmentMinDuration = TimeSpan.FromSeconds(10);

            foreach (var fileSegment in fileSegments)
            {
                var mediaType = MediaTypes.GetMediaType(fileSegment.OriginalFile.Extension);
                var duration = audioUtility.Duration(fileSegment.OriginalFile, mediaType);

                var startOffset = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
                var endOffset = fileSegment.SegmentEndOffset.HasValue ? fileSegment.SegmentEndOffset.Value : duration;

                var fileSegmentDuration = (endOffset - startOffset).TotalMilliseconds;

                var analysisSegmentMaxDuration = settings.SegmentMaxDuration.HasValue
                                                     ? settings.SegmentMaxDuration.Value.TotalMilliseconds
                                                     : fileSegmentDuration;

                var analysisSegmentMinDuration = settings.SegmentMinDuration.HasValue
                                                     ? settings.SegmentMinDuration.Value.TotalMilliseconds
                                                     : defaultAnalysisSegmentMinDuration.TotalMilliseconds;

                // use the max duration to divide up the range
                // this is the number of segments required to not go over the max duration
                var analysisSegmentsForMaxSize = Math.Ceiling(fileSegmentDuration / analysisSegmentMaxDuration);

                // get the segment durations
                // TODO: assumption is for 1 minute chunks.
                var segments = DivideEvenly(Convert.ToInt64(fileSegmentDuration), Convert.ToInt64(analysisSegmentsForMaxSize)).ToList();

                long aggregate = 0;

                for (var index = 0; index < segments.Count; index++)
                {
                    var currentSegment = new FileSegment
                        {
                            OriginalFile = fileSegment.OriginalFile,
                            SegmentStartOffset = startOffset.Add(TimeSpan.FromMilliseconds(aggregate))
                        };

                    aggregate += segments[index];

                    // include overlap
                    currentSegment.SegmentEndOffset =
                        startOffset.Add(TimeSpan.FromMilliseconds(aggregate)).Add(settings.SegmentOverlapDuration);

                    yield return currentSegment;
                }
            }
        }

        // from: http://stackoverflow.com/a/577451/31567
        // This doesn't try to cope with negative numbers :)
        private static IEnumerable<long> DivideEvenly(long numerator, long denominator)
        {
            long rem;
            long div = Math.DivRem(numerator, denominator, out rem);

            for (long i = 0; i < denominator; i++)
            {
                yield return i < rem ? div + 1 : div;
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
        private IEnumerable<FileInfo> PrepareFiles(AnalysisSettings analysisSettings, IEnumerable<FileSegment> fileSegments)
        {
            var audioUtility = new MasterAudioUtility(
                analysisSettings.SegmentTargetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);

            foreach (var fileSegment in fileSegments)
            {
                var mediaType = MediaTypes.GetMediaType(fileSegment.OriginalFile.Extension);
                var duration = audioUtility.Duration(fileSegment.OriginalFile, mediaType);

                var startOffset = fileSegment.SegmentStartOffset.HasValue ? fileSegment.SegmentStartOffset.Value : TimeSpan.Zero;
                var endOffset = fileSegment.SegmentEndOffset.HasValue ? fileSegment.SegmentEndOffset.Value : duration;

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

                    offsetsFromEntireFile.Add(new Range<TimeSpan>
                    {
                        Minimum = TimeSpan.FromMilliseconds(start) + startOffset,
                        Maximum = TimeSpan.FromMilliseconds(end) + startOffset
                    });

                    currentPostion = end;
                }

                // make sure last segment is at least segmentMinDuration long if possible.
                var last = offsetsFromEntireFile.Last();
                if (offsetsFromEntireFile.Count > 1
                    && last.Maximum - last.Minimum < segmentMinDuration)
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
                        fileSegment.OriginalFile.Name,
                        offset.Minimum.TotalMilliseconds,
                        offset.Maximum.TotalMilliseconds,
                        MediaTypes.GetExtension(analysisSettings.SegmentMediaType));

                    var path = new FileInfo(Path.Combine(analysisSettings.AnalysisBaseDirectory.FullName, "segmentedaudio", filename));

                    if (!File.Exists(path.FullName))
                    {
                        audioUtility.Segment(
                        fileSegment.OriginalFile,
                        mediaType,
                        path,
                        analysisSettings.SegmentMediaType,
                        offset.Minimum,
                        offset.Maximum);
                    }

                    yield return path;
                }
            }
        }

        private IAudioUtility GetNewAudioUtility(int targetSampleRateHz)
        {
            var audioUtility = new MasterAudioUtility(targetSampleRateHz, SoxAudioUtility.SoxResampleQuality.VeryHigh);
            return audioUtility;
        }
    }
}