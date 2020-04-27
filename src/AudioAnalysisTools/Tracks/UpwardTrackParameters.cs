// <copyright file="UpwardTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect vertical track components i.e. events which are completed within very few time frames, i.e. whips and near clicks.
    /// An UpwardTrack sounds like a whip. Each track point ascends one frequency bin. Points may move forwards or back one frame step.
    /// </summary>
    [YamlTypeTag(typeof(UpwardTrackParameters))]
    public class UpwardTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets the minimum bandwidth, units = Hertz.
        /// </summary>
        public int? MinBandwidthHertz { get; set; }

        /// <summary>
        /// Gets or sets maximum bandwidth, units = Hertz.
        /// </summary>
        public int? MaxBandwidthHertz { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether proximal similar vertical tracks are to be combined.
        /// Proximal means track time starts are not separated by more than the specified seconds interval.
        /// Similar means that track frequency bounds do not differ by more than the specified Hertz interval.
        /// </summary>
        public bool CombineProximalSimilarEvents { get; set; }

        public TimeSpan SyllableStartDifference { get; set; }

        public int SyllableHertzDifference { get; set; }
    }
}
