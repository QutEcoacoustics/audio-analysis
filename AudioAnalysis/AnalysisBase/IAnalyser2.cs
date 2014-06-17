// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalyser2.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Interface a compatible analysis must implement.
//   This is a strong typed version of <c>IAnalyser</c> intentionally removed from the old inheritance tree.
//   DO NOT MODIFY THIS FILE UNLESS INSTRUCTED TO!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AnalysisBase.ResultBases;

    /// <summary>
    /// Interface a compatible analysis must implement.
    /// This is a strong typed version of <c>IAnalyser</c> intentionally removed from the old inheritance tree.
    /// DO NOT MODIFY THIS FILE UNLESS INSTRUCTED TO!
    /// </summary>
    public interface IAnalyser2
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets Identifier. This should be a dotted uniquely identifying name. E.g. <code>Towsey.MultiAnalyser</code>.
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

        /// <summary>
        /// Ensures abstract types are downcast by the analyzer and written to file.
        /// </summary>
        /// <param name="destination">The file to write to.</param>
        /// <param name="results">The results to write.</param>
        void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results);

        /// <summary>
        /// Ensures abstract types are downcast by the analyzer and written to file.
        /// </summary>
        /// <param name="destination">The file to write to.</param>
        /// <param name="results">The results to write.</param>
        void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<IndexBase> results);

        /// <summary>
        /// Ensures abstract types are downcast by the analyzer and written to file.
        /// </summary>
        /// <param name="destination">The file to write to.</param>
        /// <param name="results">The results to write.</param> 
        void WriteSpectrumIndicesFile(FileInfo destination, IEnumerable<SpectrumBase> results);

        /// <summary>
        /// Allows Events to be rendered as Summary Indices
        /// </summary>
        /// <param name="events">The events to process.</param>
        /// <param name="unitTime">The unit time of the summary indices to produce.</param>
        /// <param name="duration">The duration of audio for the period analyzed that produced <c>events</c>.</param>
        /// <param name="scoreThreshold">A threshold to filter out low-scoring events.</param>
        /// <returns>A set of summary indices that describe the input events.</returns>
        IndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold);

        /// <summary>
        /// Post-processing for an entire analysis.
        /// </summary>
        /// <param name="settings">The settings used for the analysis.</param>
        /// <param name="inputFileSegment">A reference to the original audio file that was analyzed.</param>
        /// <param name="events">The events produced so far.</param>
        /// <param name="indices">The summary indices produced so far.</param>
        /// <param name="spectra">The spectra produced so far.</param>
        /// <param name="results">The raw result objects produced so far.</param>
        void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, IndexBase[] indices, SpectrumBase[] spectra, AnalysisResult2[] results);
    }
}