using System;
using System.Data;
using System.IO;
namespace AnalysisBase
{
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
        /// Gets Identifier.
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
