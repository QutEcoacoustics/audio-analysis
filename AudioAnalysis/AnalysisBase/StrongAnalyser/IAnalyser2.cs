using System;
using System.Collections.Generic;
using System.IO;
using AnalysisBase.StrongAnalyser.ResultBases;

namespace AnalysisBase.StrongAnalyser
{
    /// <summary>
    /// Interface a compatible analysis must implement.
    /// This is a stong typed version of IAnalyser intentionally removed from the old inheritence tree.
    /// </summary>
    public interface IAnalyser2
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
        AnalysisResult2 Analyse(AnalysisSettings analysisSettings);

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
        void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);

        /// <summary>
        /// Ensures abstract types are downcast by the analyser and written to file.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="results"></param>  
        void WriteSpectrumIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);

        /// <summary>
        /// Allows Events to be rendered as Summary Indices
        /// </summary>
        /// <param name="events"></param>
        /// <param name="unitTime"></param>
        /// <param name="duration"></param>
        /// <param name="scoreThreshold"></param>
        /// <returns></returns>
        IndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <param name="index"></param>
        /// <param name="spectras"></param>
        void SummariseResults(EventBase[] events, IndexBase[] index, SpectrumBase[] spectras);
    }
}