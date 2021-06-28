// <copyright file="OscillationParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using Acoustics.Shared;

    /// <summary>
    /// Parameters needed from a config file to detect oscillation components.
    /// </summary>
    [YamlTypeTag(typeof(OscillationParameters))]
    public class OscillationParameters : DctParameters
    {
        /// <summary>
        /// Gets or sets the minimum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min threshold.
        /// </summary>
        /// <value>The value in oscillations per second.</value>
        public double? MinOscillationFrequency { get; set; }

        /// <summary>
        /// Gets or sets the maximum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates above the max threshold.
        /// </summary>
        /// <value>The value in oscillations per second.</value>
        public double? MaxOscillationFrequency { get; set; }
    }
}