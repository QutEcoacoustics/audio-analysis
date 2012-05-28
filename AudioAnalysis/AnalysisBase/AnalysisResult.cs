namespace AnalysisBase
{
    using System.Data;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Analysis Results.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisResult"/> class.
        /// </summary>
        public AnalysisResult()
        {
            this.DisplayItems = new Dictionary<int, string>();
        }

        #region string identifiers

        /// <summary>
        /// Gets AnalysisIdentifierString.
        /// </summary>
        public static string AnalysisIdentifierString
        {
            get
            {
                return "AnalysisResult.analysisId";
            }
        }

        /// <summary>
        /// Gets ScoreString.
        /// </summary>
        public static string ScoreString
        {
            get
            {
                return "AnalysisResult.score";
            }
        }

        /// <summary>
        /// Gets AdditionalInfoString.
        /// </summary>
        public static string AdditionalInfoString
        {
            get
            {
                return "AnalysisResult.additionalInfo";
            }
        }

        /// <summary>
        /// Gets MinOffsetMsString.
        /// </summary>
        public static string MinOffsetMsString
        {
            get
            {
                return "AnalysisResult.minOffsetMs";
            }
        }

        /// <summary>
        /// Gets MaxOffsetMsString.
        /// </summary>
        public static string MaxOffsetMsString
        {
            get
            {
                return "AnalysisResult.maxOffsetMs";
            }
        }

        /// <summary>
        /// Gets MinFrequencyHzString.
        /// </summary>
        public static string MinFrequencyHzString
        {
            get
            {
                return "AnalysisResult.minFrequencyHz";
            }
        }

        /// <summary>
        /// Gets MaxFrequencyHzString.
        /// </summary>
        public static string MaxFrequencyHzString
        {
            get
            {
                return "AnalysisResult.maxFrequencyHz";
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets Analysis Identifier.
        /// </summary>
        public string AnalysisIdentifier { get; set; }

        /// <summary>
        /// Gets or sets results.
        /// </summary>
        public DataTable Data { get; set; }

        /// <summary>
        /// Gets or sets the settings used to produce the results.
        /// </summary>
        public AnalysisSettings SettingsUsed { get; set; }

        /// <summary>
        /// Gets DisplayItems which contains indexes to display and optional item names.
        /// </summary>
        public Dictionary<int, string> DisplayItems { get; private set; }

        /// <summary>
        /// Gets or sets OutputFiles.
        /// </summary>
        public Dictionary<string, FileInfo> OutputFiles { get; set; }
    }
}
