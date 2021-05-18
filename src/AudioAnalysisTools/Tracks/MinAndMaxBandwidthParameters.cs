// <copyright file="MinAndMaxBandwidthParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Acoustics.Shared.ConfigFile;

    public class MinAndMaxBandwidthParameters : CommonParameters
    {
        /*
        /// <summary>
        /// Gets or sets the bottom bound of a search band. Units are Hertz.
        /// A search band is the frequency band within which an algorithm searches for a particular track or event.
        /// This is to be carefully distinguished from the top and bottom bounds of a specific event.
        /// A search band consists of two parallel lines/freqeuncy bins.
        /// An event is represented by a rectangle.
        /// Events will/should always lie within a search band. There may be exception in edge cases, i.e. where an event sits on a search bound.
        /// </summary>
        public int? SearchbandMinHertz { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of a search band. Units are Hertz.
        /// A search band is the frequency band within which an algorithm searches for a particular track or event.
        /// </summary>
        public int? SearchbandMaxHertz { get; set; }
        */

        /// <summary>
        /// Gets or sets the minimum allowed bandwidth of a spectrogram track or event, units = Hertz.
        /// </summary>
        public int? MinBandwidthHertz { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed bandwidth of a spectrogram track or event, units = Hertz.
        /// </summary>
        public int? MaxBandwidthHertz { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return this.MinHertz.ValidateNotNull(nameof(this.MinHertz));
            yield return this.MaxHertz.ValidateNotNull(nameof(this.MaxHertz));
            yield return this.MinBandwidthHertz.ValidateNotNull(nameof(this.MinBandwidthHertz));
            yield return this.MaxBandwidthHertz.ValidateNotNull(nameof(this.MaxBandwidthHertz));

            foreach (var validation in base.Validate(validationContext))
            {
                yield return validation;
            }
        }
    }
}
