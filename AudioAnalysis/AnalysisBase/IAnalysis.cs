namespace AnalysisBase
{
    /// <summary>
    /// Interface a compatible analysis must implement.
    /// </summary>
    public interface IAnalysis
    {
        /// <summary>
        /// Gets the initial settings for the analysis.
        /// </summary>
        AnalysisSettings InitialSettings { get; }

        /// <summary>
        /// Prepare the analysis processing. This could involve creating files or directories in the working directory,
        /// setting additional settings, or any other preparation.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis settings.
        /// </param>
        /// <returns>
        /// The analysis settings to use to run the analysis.
        /// </returns>
        AnalysisSettings PrepareAnalysis(AnalysisSettings analysisSettings);

        /// <summary>
        /// Run analysis over the given audio file, using the 
        /// settings from configuration file. Use the working directory.
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
