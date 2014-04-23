using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AnalysisBase.StrongAnalyser.ResultBases;

namespace AnalysisBase.StrongAnalyser
{
    /// <summary>
    /// Provides sensible defaults for some of the functionality required by IAnalyser2
    /// </summary>
    public abstract class IAnalyser2Abstract : IAnalyser2
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
        public abstract IEnumerable<IndexBase> ProcessCsvFile(FileInfo csvFile, FileInfo configFile);
        public abstract void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results);
        public abstract void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);
        public abstract void WriteSpectrumIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);

        public virtual IndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime,
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

            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (EventBase anEvent in events)
            {
                double eventStart = anEvent.EventStartAbsolute ?? anEvent.EventStartSeconds;
                // (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = anEvent.Score; // (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int) (eventStart / unitTime.TotalSeconds);

                // TODO: why not -gt, ask michael
                if (eventScore != 0.0)
                {
                    eventsPerUnitTime[timeUnit]++;
                }
                if (eventScore > scoreThreshold)
                {
                    bigEvsPerUnitTime[timeUnit]++;
                }
            }

            var indices = new IndexBase[eventsPerUnitTime.Length];

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                var newIndex = new EventIndex();

                int unitId = (int) (i * unitTime.TotalMinutes);

                newIndex.StartOffsetMinute = unitId;
                newIndex.EventsTotal = eventsPerUnitTime[i];
                newIndex.EventsTotalThresholded = bigEvsPerUnitTime[i];

                indices[i] = newIndex;
            }

            return indices;

        }

        public abstract void SummariseResults(EventBase[] events, IndexBase[] index, SpectrumBase[] spectras);
    }
}
