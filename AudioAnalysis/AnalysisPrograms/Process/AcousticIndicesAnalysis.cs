namespace AnalysisPrograms
{
    using System;

    using AnalysisBase;

    /// <summary>
    /// Acoustic Indices Audio Analysis.
    /// </summary>
    public class AcousticIndicesAudioAnalysis : IAnalysis
    {
        /// <summary>
        /// Gets the name to display for the analysis.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "Acoustic Indices";
            }
        }

        /// <summary>
        /// Gets Identifier.
        /// </summary>
        public string Identifier
        {
            get
            {
                return "AcousticIndices";
            }
        }

        /// <summary>
        /// Gets the initial (default) settings for the analysis.
        /// </summary>
        public PreparerSettings DefaultFileSettings
        {
            get
            {
                return new PreparerSettings
                    {
                        SegmentMaxDuration = TimeSpan.FromMinutes(1),
                        SegmentOverlapDuration = TimeSpan.Zero,
                        SegmentTargetSampleRate = 22050
                    };
            }
        }

        /// <summary>
        /// Gets the Default Configuration.
        /// </summary>
        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Run analysis using the given analysis settings.
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
