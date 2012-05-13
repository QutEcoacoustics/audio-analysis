namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The settings for preparing files to be processed.
    /// </summary>
    public class PreparerSettings
    {
        /// <summary>
        /// Gets or sets the duration for segments to overlap when the original audio file is longer than <see cref="SegmentMaxDuration"/>.
        /// </summary>
        public TimeSpan SegmentOverlapDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum audio file duration the analysis can process.
        /// </summary>
        public TimeSpan SegmentMaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the audio sample rate the analysis expects (in hertz).
        /// </summary>
        public int SegmentTargetSampleRate { get; set; }
    }
}
