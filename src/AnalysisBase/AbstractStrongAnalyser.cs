// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractStrongAnalyser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Provides sensible defaults for some of the functionality required by IAnalyser2.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared.ConfigFile;

    using ResultBases;

    /// <summary>
    /// Provides sensible defaults for some of the functionality required by <c>IAnalyser2</c>.
    /// </summary>
    public abstract class AbstractStrongAnalyser : IAnalyser2
    {
        /// <inheritdoc/>
        public abstract string DisplayName { get; }

        /// <inheritdoc/>
        public abstract string Identifier { get; }

        /// <inheritdoc/>
        public virtual string Description => "YOU SHOULD IMPLEMENT THIS!";

        /// <inheritdoc/>
        public virtual AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings();
            }
        }

        /// <inheritdoc cref="IAnalyser2.ParseConfig"/>
        public virtual AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<AnalyzerConfig>(file);
        }

        /// <inheritdoc/>
        public virtual void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            return; // noop
        }

        /// <inheritdoc/>
        public abstract AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings);

        /// <inheritdoc/>
        public abstract void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results);

        /// <inheritdoc/>
        public abstract void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results);

        /// <inheritdoc/>
        public abstract List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results);

        /// <inheritdoc />
        public virtual SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {

            if (duration == TimeSpan.Zero)
            {
                return null;
            }

            double units = duration.TotalSeconds / unitTime.TotalSeconds;

            // get whole minutes
            int unitCount = (int)(units / 1);

            // add fractional minute
            if (units % 1 > 0.0)
            {
                unitCount += 1;
            }

            // to store event counts
            var eventsPerUnitTime = new int[unitCount];

            // to store counts of high scoring events
            var bigEvsPerUnitTime = new int[unitCount];

            foreach (var anEvent in events)
            {
                // note: absolute determines what value is used
                // EventStartSeconds (relative to ~~segment~~, 2017-09: RELATIVE TO RECORDING)
                // StartOffset (relative to recording)
                double eventStart = anEvent.EventStartSeconds;
                double eventScore = anEvent.Score;
                var timeUnit = (int)(eventStart / unitTime.TotalSeconds);

                // NOTE: eventScore filter replaced with greater then as opposed to not equal to
                if (eventScore >= 0.0)
                {
                    eventsPerUnitTime[timeUnit]++;
                }

                if (eventScore > scoreThreshold)
                {
                    bigEvsPerUnitTime[timeUnit]++;
                }
            }

            var indices = new SummaryIndexBase[eventsPerUnitTime.Length];

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                var newIndex = new EventIndex
                                   {
                                       ResultStartSeconds = unitTime.Multiply(i).TotalSeconds,
                                       EventsTotal = eventsPerUnitTime[i],
                                       EventsTotalThresholded = bigEvsPerUnitTime[i],
                                       SegmentDurationSeconds = unitTime.TotalSeconds,
                                   };

                indices[i] = newIndex;
            }

            return indices;

        }

        /// <inheritdoc/>
        public abstract void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results);
    }
}