﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnalyser.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;

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


}
