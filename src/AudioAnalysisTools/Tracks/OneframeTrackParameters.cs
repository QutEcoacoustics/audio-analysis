// <copyright file="OneframeTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;

    /// <summary>
    /// Parameters needed from a config file to detect click components.
    /// </summary>
    [YamlTypeTag(typeof(OneframeTrackParameters))]
    public class OneframeTrackParameters : MinAndMaxBandwidthParameters
    {
        /// <summary>
        /// MAY NOT WANT TO COMBINE CLICK EVENTS.
        /// Gets or sets a value indicating whether proximal similar clicks are to be combined.
        /// Proximal means the clicks' time starts are not separated by more than the specified seconds interval.
        /// Similar means that the clicks' frequency bounds do not differ by more than the specified Hertz interval.
        /// </summary>
        //public bool CombineProximalSimilarEvents { get; set; }
    }
}
