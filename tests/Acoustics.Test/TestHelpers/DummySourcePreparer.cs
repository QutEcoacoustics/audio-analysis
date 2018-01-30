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
    using Acoustics.Shared;
    using global::AnalysisBase;
    using global::AnalysisBase.Segment;

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

            var basename = Path.GetFileNameWithoutExtension(source);

            return Task.Run(() =>
            {
                var path = outputDirectory.CombineFile(basename + $"_{min}min." + outputMediaType);

                using (var file = path.CreateText())
                {
                    file.WriteLine(
                        $"{outputDirectory},{source},{outputMediaType},{startOffset},{endOffset},{targetSampleRateHz}");
                }

                return new FileSegment(path, targetSampleRateHz, endOffset - startOffset);
            });
        }

        public Task<FileSegment> PrepareFile<TSource>(
            DirectoryInfo outputDirectory,
            ISegment<TSource> source,
            string outputMediaType,
            int? targetSampleRateHz,
            DirectoryInfo temporaryFilesDirectory,
            int[] channelSelection,
            bool? mixDownToMono)
        {
            int min = (int)source.StartOffsetSeconds.Seconds().TotalMinutes;

            if (typeof(TSource) != typeof(FileInfo))
            {
                throw new NotSupportedException("Dummy Source Preparer only works with FileInfos");
            }

            var basename = Path.GetFileNameWithoutExtension((source.Source as FileInfo).Name);

            return Task.Run(() =>
            {
                var path = outputDirectory.CombineFile(basename + $"_{min}min." + MediaTypes.GetExtension(outputMediaType));

                using (var file = path.CreateText())
                {
                    file.WriteLine(
                        $"{outputDirectory},{source},{outputMediaType},{source.StartOffsetSeconds},{source.EndOffsetSeconds},{targetSampleRateHz}"
                        + $",{temporaryFilesDirectory},{channelSelection},{mixDownToMono}");
                }

                return new FileSegment(path, targetSampleRateHz.Value, (source.EndOffsetSeconds - source.StartOffsetSeconds).Seconds());
            });
        }

        public IEnumerable<ISegment<TSource>> CalculateSegments<TSource>(
            IEnumerable<ISegment<TSource>> fileSegments,
            AnalysisSettings settings)
        {
            foreach (var segment in fileSegments)
            {
                var start = segment.StartOffsetSeconds.Seconds();
                var end = segment.EndOffsetSeconds.Seconds();
                var duration = end - start;

                var segmentDuration = 60.0.Seconds();
                for (var t = start; t < end; t += segmentDuration)
                {
                    var segmentEnd = end.Min(t + segmentDuration);
                    yield return segment.SplitSegment(t.TotalSeconds, segmentEnd.TotalSeconds);
                }
            }
        }
    }
}
