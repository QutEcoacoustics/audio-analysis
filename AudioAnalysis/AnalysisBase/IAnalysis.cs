using System.IO;
namespace AnalysisBase
{
    /// <summary>
    /// Interface a compatible analysis must implement.
    /// </summary>
    public interface IAnalysis
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
        PreparerSettings DefaultFileSettings { get; }

        /// <summary>
        /// Gets the Default Configuration.
        /// </summary>
        string DefaultConfiguration { get; }

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

        //FileInfo AnalysisImage(AnalysisSettings settings);
    }
}
