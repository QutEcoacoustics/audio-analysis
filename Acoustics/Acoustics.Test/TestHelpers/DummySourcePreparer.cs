// <copyright file="DummySourcePreparer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::AnalysisBase;
    using global::AnalysisBase.SegmentAnalysis;

    public class DummySourcePreparer : ISourcePreparer
    {
        public Task<FileSegment> PrepareFile(
            DirectoryInfo outputDirectory,
            string source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz)
        {
            int min = (int)startOffset.TotalMinutes;

            return Task.Run(() =>
            {
                var path = outputDirectory.CombineFile(source + $"_{min}min" + outputMediaType);

                using (var file = path.CreateText())
                {
                    file.WriteLine(
                        $"{outputDirectory},{source},{outputMediaType},{startOffset},{endOffset},{targetSampleRateHz}");
                }

                return new FileSegment(path);
            });
        }

        public Task<FileSegment> PrepareFile(
            DirectoryInfo outputDirectory,
            string source,
            string outputMediaType,
            TimeSpan startOffset,
            TimeSpan endOffset,
            int targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection,
            bool? mixDownToMono)
        {
            int min = (int)startOffset.TotalMinutes;

            return Task.Run(() =>
            {
                var path = outputDirectory.CombineFile(source + $"_{min}min" + outputMediaType);

                using (var file = path.CreateText())
                {
                    file.WriteLine(
                        $"{outputDirectory},{source},{outputMediaType},{startOffset},{endOffset},{targetSampleRateHz}"
                        + $",{temporaryFilesDirectory},{channelSelection},{mixDownToMono}");
                }

                return new FileSegment(path);
            });
        }

        public IEnumerable<ISegment<TSource>> CalculateSegments<TSource>(
            IEnumerable<ISegment<TSource>> fileSegments,
            AnalysisSettingsBase<TSource> settings)
        {
            foreach (var segment in fileSegments)
            {
                var start = segment.StartOffsetSeconds.Seconds();
                var end = segment.EndOffsetSeconds.Seconds();
                var duration = end - start;

                var segmentDuration = 60.0.Seconds();
                for (var t = start; t < end; t += segmentDuration)
                {
                    yield return segment.SplitSegment(t.TotalSeconds, (t + segmentDuration).TotalSeconds);
                }
            }
        }
    }
}
