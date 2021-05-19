// <copyright file="OnebinTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect whistle components.
    /// A one-bin sounds like a pure-tone whistle. Each track point advances one time step. Points stay in the same frequency bin.
    /// </summary>
    [YamlTypeTag(typeof(OnebinTrackParameters))]
    public class OnebinTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether proximal whistle tracks are to be combined.
        /// Proximal means the whistle tracks are in the same frequency band
        /// ... and that the gap between their start times is not greater than the specified seconds interval.
        /// </summary>
        public bool CombinePossibleSyllableSequence { get; set; }
    }
}