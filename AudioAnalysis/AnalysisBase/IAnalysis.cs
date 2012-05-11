namespace AnalysisBase
{
    /// <summary>
    /// Interface a compatible analysis must implement.
    /// </summary>
    public interface IAnalysis
    {
        /// <summary>
        /// Gets the initial settings for the analysis. These are the default settings.
        /// </summary>
        AnalysisSettings InitialSettings { get; }

        /// <summary>
        /// Run analysis usign the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        AnalysisResult Analyse(AnalysisSettings analysisSettings);
    }
}
