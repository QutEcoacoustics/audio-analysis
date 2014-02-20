using System;
using System.Data;
using System.IO;
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

        new IEnumerable<ResultBase> ProcessCsvFile(FileInfo csvFile, FileInfo configFile);

        IEnumerable<IndexBase> ConvertEventsToIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold);
    }

    public abstract class ResultBase
    {
        public int MinuteOffset { get; set; }
        protected abstract AnalysisSettings AnalysisSettings { get; set; }
    }

    public abstract class EventBase : ResultBase
    {
    }

    public abstract class IndexBase : ResultBase
    {
    }
}
