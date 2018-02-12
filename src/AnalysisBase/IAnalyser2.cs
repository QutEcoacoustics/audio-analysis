// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalyser2.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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

    using Acoustics.Shared.ConfigFile;

    using ResultBases;

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
        /// Gets a user friendly string describing the analyzer. Intending for printing in the console.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        AnalysisSettings DefaultSettings { get; }

        /// <summary>
        /// An (optional) method for returning a strongly typed config file
        /// </summary>
        /// <param name="file">The file that represents the config to read.</param>
        /// <returns>Ideally a strongly typed config, but as a fallback, a base <see cref="Config"/> can be returned.</returns>
        AnalyzerConfig ParseConfig(FileInfo file);

        /// <summary>
        /// A hook to modify analysis settings before an analysis is run.
        /// Ideally run once (whereas Analyze is run N times).
        /// </summary>
        /// <param name="analysisSettings">The analysis Settings.</param>
        void BeforeAnalyze(AnalysisSettings analysisSettings);

        /// <summary>
        /// Run analysis using the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        ///     The analysis Settings.
        /// </param>
        /// <param name="segmentSettings">The settings unique to the current segment being analyzed.</param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings);

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
        void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results);

        /// <summary>
        /// Ensures abstract types are downcast by the analyzer and written to file.
        /// </summary>
        /// <param name="destination">The file to write to.</param>
        /// <param name="fileNameBase"></param>
        /// <param name="results">The results to write.</param>
        List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results);

        /// <summary>
        /// Allows Events to be rendered as Summary Indices
        /// </summary>
        /// <param name="events">The events to process.</param>
        /// <param name="unitTime">The unit time of the summary indices to produce.</param>
        /// <param name="duration">The duration of audio for the period analyzed that produced <c>events</c>.</param>
        /// <param name="scoreThreshold">A threshold to filter out low-scoring events.</param>
        /// <returns>A set of summary indices that describe the input events.</returns>
        SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold);

        /// <summary>
        /// Post-processing for an entire analysis.
        /// </summary>
        /// <param name="settings">The settings used for the analysis.</param>
        /// <param name="inputFileSegment">A reference to the original audio file that was analyzed.</param>
        /// <param name="events">The events produced so far.</param>
        /// <param name="indices">The summary indices produced so far.</param>
        /// <param name="spectralIndices">The spectra produced so far.</param>
        /// <param name="results">The raw result objects produced so far.</param>
        void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results);
    }
}