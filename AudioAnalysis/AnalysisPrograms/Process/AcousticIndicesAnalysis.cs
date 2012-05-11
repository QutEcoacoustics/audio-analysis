namespace AnalysisPrograms
{
    using System.Data;
    using System.IO;

    using AnalysisBase;

    /// <summary>
    /// </summary>
    public class AcousticIndicesAudioProcessor : IAnalysis
    {
        /// <summary>
        /// Gets the initial settings for the analysis. These are the default settings.
        /// </summary>
        public AnalysisSettings InitialSettings
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Run analysis usign the given analysis settings.
        /// </summary>
        /// <param name="analysisSettings">
        /// The analysis Settings.
        /// </param>
        /// <returns>
        /// The results of the analysis.
        /// </returns>
        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            throw new System.NotImplementedException();
        }
    }
}
