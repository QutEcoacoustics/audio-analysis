// <copyright file="DctParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    /// <summary>
    /// Common parameters needed from a config file to detect components,
    /// used by algorithms that have tunable DCT parameters.
    /// </summary>
    public abstract class DctParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets the time duration (in seconds) of a Discrete Cosine Transform.
        /// </summary>
        public double DctDuration { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the minimum acceptable value of a DCT coefficient.
        /// </summary>
        public double DctThreshold { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the minimum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min &amp; above the max threshold.
        /// </summary>
        public int MinOscillationFrequency { get; set; }

        /// <summary>
        /// Gets or sets the maximum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min &amp; above the max threshold.
        /// </summary>
        public int MaxOscillationFrequency { get; set; }

        /// <summary>
        /// Gets or sets the Event threshold - use this to determine FP / FN trade-off for events.
        /// </summary>
        public double EventThreshold { get; set; } = 0.3;
    }
}