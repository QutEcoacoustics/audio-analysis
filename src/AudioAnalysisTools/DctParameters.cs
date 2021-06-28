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
        /// <value>The duration of the window in seconds.</value>
        public double DctDuration { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the minimum acceptable value of a DCT coefficient.
        /// </summary>
        /// <remarks>
        /// Lowering `DctThreshold` increases the likelihood that random noise
        /// will be accepted as a true oscillation; increasing `DctThreshold`
        /// increases the likelihood that a target oscillation is rejected.
        /// </remarks>
        /// <value>A value representing a minimum amplitude threshold in the range `[0, 1]`.</value>
        public double DctThreshold { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the Event threshold - use this to determine FP / FN trade-off for events.
        /// </summary>
        /// <value>A number between <c>0.0</c> and <c>1.0</c>.</value>
        public double EventThreshold { get; set; } = 0.3;
    }
}