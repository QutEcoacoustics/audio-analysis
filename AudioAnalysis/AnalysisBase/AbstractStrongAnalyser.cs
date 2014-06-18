// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractStrongAnalyser.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

    using AnalysisBase.ResultBases;

    /// <summary>
    /// Provides sensible defaults for some of the functionality required by <c>IAnalyser2</c>.
    /// </summary>
    public abstract class AbstractStrongAnalyser : IAnalyser2
    {
        public abstract string DisplayName { get; }

        public abstract string Identifier { get; }

        public virtual AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings();
            }
        }


        public abstract AnalysisResult2 Analyse(AnalysisSettings analysisSettings);

        public abstract void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results);

        public abstract void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results);

        public abstract void WriteSpectrumIndicesFile(FileInfo destination, IEnumerable<SpectrumBase> results);

        public virtual SummaryIndexBase[] ConvertEventsToSummaryIndices(
            IEnumerable<EventBase> events,
            TimeSpan unitTime,
            TimeSpan duration,
            double scoreThreshold)
        {

            if (duration == TimeSpan.Zero)
            {
                return null;
            }

            double units = duration.TotalSeconds / unitTime.TotalSeconds;

            // get whole minutes
            int unitCount = (int) (units / 1);

            // add fractional minute
            if ((units % 1) > 0.0)
            {
                unitCount += 1;
            }

            // to store event counts
            var eventsPerUnitTime = new int[unitCount];

            // to store counts of high scoring events
            var bigEvsPerUnitTime = new int[unitCount];

            foreach (var anEvent in events)
            {
                double eventStart = anEvent.EventStartAbsolute ?? anEvent.EventStartSeconds;
                //// (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = anEvent.Score; // (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
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
                var newIndex = new EventIndex();

                newIndex.SegmentStartOffset = unitTime.Multiply(i);
                newIndex.EventsTotal = eventsPerUnitTime[i];
                newIndex.EventsTotalThresholded = bigEvsPerUnitTime[i];

                indices[i] = newIndex;
            }

            return indices;

        }

        public abstract void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectrumBase[] spectra, AnalysisResult2[] results);
    }
}