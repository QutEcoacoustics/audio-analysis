// <copyright file="ForwardTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;

    /// <summary>
    /// Parameters needed from a config file to detect forwards spectral peak tracks.
    /// A ForwardTrack sounds like a fluctuating tone or technically, a chirp. Each track point advances one time step. Points may move up or down by at most two frequency bins.
    /// </summary>
    [YamlTypeTag(typeof(ForwardTrackParameters))]
    public class ForwardTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether coincident tracks stacked on top of one another are to be combined.
        /// Coincident means the tracks' start and end times are not greater than the specified seconds interval.
        /// Stacked means that the frequency gap between each of the stacked tracks does not exceed the specified Hertz interval.
        /// </summary>
        public bool CombinePossibleHarmonics { get; set; }

        public TimeSpan HarmonicsStartDifference { get; set; }

        public int HarmonicsHertzGap { get; set; }
    }
}
