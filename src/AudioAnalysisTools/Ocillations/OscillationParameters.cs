// <copyright file="OscillationParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    public enum OscillationAlgorithm
    {
        Standard,
        Hits,
    }

    /// <summary>
    /// Parameters needed from a config file to detect oscillation components.
    /// </summary>
    [YamlTypeTag(typeof(OscillationParameters))]
    public class OscillationParameters : DctParameters
    {
        /// <summary>
        /// Gets or sets he algorithm to be used to find oscillation events.
        /// </summary>
        public OscillationAlgorithm Algorithm { get; set; }

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

        /// <summary>
        /// Return oscillation events as determined by the user set parameters.
        /// </summary>
        public static (List<EventCommon> OscillEvents, List<Plot> Plots) GetOscillationEvents(
            SpectrogramStandard spectrogram,
            OscillationParameters op,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var algorithm = op.Algorithm;

            List<EventCommon> events;
            List<Plot> plots;

            if (algorithm == OscillationAlgorithm.Hits)
            {
                (events, plots) = Oscillations2012.GetComponentsWithOscillations(
                    spectrogram,
                    op,
                    decibelThreshold,
                    segmentStartOffset,
                    profileName);
            }
            else
            {
                // the standard algorithm is the default.
                (events, plots) = Oscillations2019.GetComponentsWithOscillations(
                    spectrogram,
                    op,
                    decibelThreshold,
                    segmentStartOffset,
                    profileName);
            }

            return (events, plots);
        }
    }
}