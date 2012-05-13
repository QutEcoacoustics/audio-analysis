namespace AnalysisBase
{
    using System.Data;

    /// <summary>
    /// Analysis Results.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Gets or sets Analysis Identifier.
        /// </summary>
        public string AnalysisIdentifier { get; set; }

        /// <summary>
        /// Gets or sets results.
        /// </summary>
        public DataTable Results { get; set; }

        /// <summary>
        /// Gets or sets the settings Used to produce the results.
        /// </summary>
        public AnalysisSettings AnalysisSettingsUsed { get; set; }

        /// <summary>
        /// Gets or sets PreparerSettingsUsed.
        /// </summary>
        public PreparerSettings PreparerSettingsUsed { get; set; }
    }
}
