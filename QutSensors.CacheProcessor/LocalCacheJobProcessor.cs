using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QutSensors.CacheProcessor
{
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;

    using AudioTools;

    using QutSensors.Business.Audio;
    using QutSensors.Data;
    using QutSensors.Shared.LogProviders;

    public class LocalCacheJobProcessor
    {
        private readonly FileInfo fileToProcess;

        private readonly int maxSegmentDurationMs;

        private readonly ILogProvider logger;

        private readonly long fileDuration;

        public LocalCacheJobProcessor(string file, int maxSegmentDurationMs, ILogProvider logger, long fileDuration)
        {
            this.fileToProcess = new FileInfo(file);
            this.maxSegmentDurationMs = maxSegmentDurationMs;
            this.logger = logger;
            this.fileDuration = fileDuration;
        }

        /// <summary>
        /// Start processing.
        /// </summary>
        public void Start()
        {
            var stopWatchAll = new Stopwatch();
            stopWatchAll.Start();

            var transformer = new AudioTransformer(this.fileToProcess.FullName);

            long positionMs = 0;

            while (positionMs < fileDuration)
            {
                var start = positionMs;
                long end = start + Math.Min(this.maxSegmentDurationMs, this.fileDuration - positionMs);

                var stopWatch = new Stopwatch();

                // if mp3 -> mp3 segments
                stopWatch.Start();
                var bytes = CacheUtilities.SegmentMp3(
                    this.fileToProcess.FullName,
                    new CacheRequest { Start = start, End = end, MimeType = MimeTypes.MimeTypeMp3 });

                stopWatch.Stop();

                if (bytes == null || bytes.Length < 1)
                {
                    stopWatch.Reset();

                    using (var ms = new MemoryStream())
                    {
                        stopWatch.Start();
                        transformer.Segment(start, end, MimeTypes.MimeTypeMp3, ms);
                        stopWatch.Stop();

                        bytes = ms.GetBuffer();
                    }
                }

                var file = "segment-" + Path.GetFileNameWithoutExtension(this.fileToProcess.FullName)
                        + "-" + positionMs + "-" + end;

                var path = Path.GetDirectoryName(this.fileToProcess.FullName);

                File.WriteAllBytes(Path.Combine(path, file + "." + MimeTypes.ExtMp3), bytes);

                if (this.logger != null)
                {
                    this.logger.WriteEntry(
                        LogType.Information,
                        "Segmented audio {0} ({1}-{2}) took {3}. Time so far {4}.",
                        this.fileToProcess.Name,
                        start,
                        end,
                        stopWatch.Elapsed.ToReadableString(),
                        stopWatchAll.Elapsed.ToReadableString());
                }

                stopWatch.Reset();

                positionMs += end - start;
            }

            stopWatchAll.Stop();

            if (this.logger != null)
            {
                this.logger.WriteEntry(
                    LogType.Information,
                    "Total time {0}",
                    stopWatchAll.Elapsed.ToReadableString());
            }
        }
    }
}
