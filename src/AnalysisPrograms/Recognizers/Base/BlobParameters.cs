namespace AnalysisPrograms.Recognizers.Base
{
    /// <summary>
    /// Parameters needed from a config file to detect blob components.
    /// The following parameters worked well on a ten minute recording containing 14-16 calls.
    /// Note: if you lower the dB threshold, you need to increase maxDurationSeconds.
    /// </summary>
    public class BlobParameters : CommonParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobParameters"/> class.
        /// </summary>
        public BlobParameters()
        {
            this.MinHertz = 800;
            this.MaxHertz = 8000;
        }
    }
}