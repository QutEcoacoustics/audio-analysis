using System;
using System.Data;
using System.IO;
using System.Text;

//using AudioAnalysisTools;

namespace AnalysisBase
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface a compatible analysis must implement.
    /// </summary>
    public interface IAnalyser
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets Identifier. This should be a dotted uniquely identifying name. E.g. Towsey.MultiAnalyser.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        AnalysisSettings DefaultSettings { get; }

        /// <summary>
        /// Run analysis using the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        AnalysisResult Analyse(AnalysisSettings analysisSettings);

        Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile);

        DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold);

        //FileInfo AnalysisImage(AnalysisSettings settings);
    }

    public interface IAnalyser2 : IAnalyser
    {
        new AnalysisResult2 Analyse(AnalysisSettings analysisSettings);

        new IEnumerable<IndexBase> ProcessCsvFile(FileInfo csvFile, FileInfo configFile);

        /// <summary>
        /// Ensures abstract types are downcast by the analyser and written to file.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="results"></param>
        void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results);
        
        /// <summary>
        /// Ensures abstract types are downcast by the analyser and written to file.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="results"></param>
        void WriteIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);

        IndexBase[] ConvertEventsToIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold);

        // TODO:
        //void WriteSpectraFile(FileInfo destination, IEnumerable<SpectraBase> results);
        //?void? SummariseResults(EventBase[] events, IndexBase[] index, SpectraBase[] spectras);
    }

    public static class AnalyserHelpers
    {
        public static IndexBase[] StandardEventToIndexConverter(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {
            if (duration == TimeSpan.Zero)
            {
                return null;
            }

            double units = duration.TotalSeconds / unitTime.TotalSeconds;

            // get whole minutes
            int unitCount = (int)(units / 1);

            // add fractional minute
            if ((units % 1) > 0.0)
            {
                unitCount += 1;
            }

            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (EventBase anEvent in events)
            {
                double eventStart = anEvent.EventStartAbsolute ?? anEvent.EventStartSeconds;// (double)ev[AudioAnalysisTools.Keys.EVENT_START_ABS];
                double eventScore = anEvent.Score; // (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);

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

                int unitId = (int)(i * unitTime.TotalMinutes);

                newIndex.MinuteOffset = unitId;
                newIndex.EventsTotal = eventsPerUnitTime[i];
                newIndex.EventsTotalThresholded = bigEvsPerUnitTime[i];

                indices[i] = newIndex;
            }

            return indices;
        }
    }

    public abstract class ResultBase
    {
        public string FileName { get; set; }

        public int SegmentCount { get; set; }

        public TimeSpan SegmentOffsetFromStartOfSource { get; set; }

        public TimeSpan SegmentDuration { get; set; } //SEGMENT_TIMESPAN = "SegTimeSpan";

        public int MinuteOffset { get; set; } //START_MIN = "start-min" = AudioAnalysisTools.Keys.EVENT_START_MIN

    }




    // TODO: Bring this to parity with the standard event class - ask Michael
    public abstract class EventBase : ResultBase
    {
        //AudioAnalysisTools.Keys.EVENT_START_ABS,    //4
        public double? EventStartAbsolute { get; set; }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public double Score { get; set; }

        //AudioAnalysisTools.Keys.EVENT_START_SEC,    //3
        public double EventStartSeconds { get; set; }

        //AudioAnalysisTools.Keys.MIN_HZ
        public double? MinHz { get; set; }

        //AudioAnalysisTools.Keys.EVENT_COUNT,        //1
        public int EventCount { get; set; }

        
        //AudioAnalysisTools.Keys.SEGMENT_TIMESPAN,   //5
        //AudioAnalysisTools.Keys.EVENT_DURATION,     //6
        ////AudioAnalysisTools.Keys.EVENT_INTENSITY,
        //AudioAnalysisTools.Keys.EVENT_NAME,         //7
        //AudioAnalysisTools.Keys.DOMINANT_FREQUENCY,
        //AudioAnalysisTools.Keys.OSCILLATION_RATE,
        //AudioAnalysisTools.Keys.EVENT_NORMSCORE,
        //AudioAnalysisTools.Keys.MAX_HZ,
        
    }





    // TODO: Bring this to parity with the standard index class - ask Michael
    public class IndexBase : ResultBase
    {
        //these dictionaries used to store index values accessible by key
        //private Dictionary<string, double> summaryIndicesOfTypeDouble = new Dictionary<string, double>();
        public Dictionary<string, double> SummaryIndicesOfTypeDouble { get; set; }
        //private Dictionary<string, int> summaryIndicesOfTypeInt = new Dictionary<string, int>();
        public Dictionary<string, int> SummaryIndicesOfTypeInt { get; set; }
        //private Dictionary<string, TimeSpan> summaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();
        public Dictionary<string, TimeSpan> SummaryIndicesOfTypeTimeSpan { get; set; }

        /// <summary>
        /// for storing spectral indices in a dictionary
        /// </summary>
        private Dictionary<string, double[]> spectralIndices;
        public Dictionary<string, double[]> SpectralIndices
        {
            get { return spectralIndices; }
            set { spectralIndices = value; }
        }


        // get any index as a double
        public double GetIndex(string key)
        {
            if (SummaryIndicesOfTypeDouble.ContainsKey(key)) return SummaryIndicesOfTypeDouble[key];
            if (SummaryIndicesOfTypeInt.ContainsKey(key)) return (double)SummaryIndicesOfTypeInt[key];
            if (SummaryIndicesOfTypeTimeSpan.ContainsKey(key)) return SummaryIndicesOfTypeTimeSpan[key].TotalMilliseconds;
            return 0.0;
        }

        public string GetIndexAsString(string key, string units)
        {
            string str = "";
            if (SummaryIndicesOfTypeDouble.ContainsKey(key)) 
                return SummaryIndicesOfTypeDouble[key].ToString();
            if (SummaryIndicesOfTypeInt.ContainsKey(key)) 
                return SummaryIndicesOfTypeInt[key].ToString();
            if (SummaryIndicesOfTypeTimeSpan.ContainsKey(key))
            {
                if (units == "s")  return SummaryIndicesOfTypeTimeSpan[key].TotalSeconds.ToString();
                if (units == "ms") return SummaryIndicesOfTypeTimeSpan[key].Milliseconds.ToString();
            }

            return str;
        }


        // get indices from relevant dictionaries
        public double GetIndexAsDouble(string key)
        {
            return SummaryIndicesOfTypeDouble[key];
        }
        public int GetIndexAsInteger(string key)
        {
            return SummaryIndicesOfTypeInt[key];
        }
        public TimeSpan GetIndexAsTimeSpan(string key)
        {
            return SummaryIndicesOfTypeTimeSpan[key];
        }


    } //class IndexBase : ResultBase
    





    public class EventIndex : IndexBase
    {
        public int EventsTotal { get; set; }

        // TODO: possibility for dynamic column name
        public int EventsTotalThresholded { get; set; }


    }
}
