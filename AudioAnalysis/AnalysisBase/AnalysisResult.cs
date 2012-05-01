namespace AnalysisBase
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Analysis Results.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisResult"/> class.
        /// </summary>
        /// <param name="results">
        /// The results.
        /// </param>
        public AnalysisResult(DataTable results)
        {
            this.Results = results;
        }

        /// <summary>
        /// Gets Results.
        /// </summary>
        public DataTable Results { get; private set; }
    }
}
