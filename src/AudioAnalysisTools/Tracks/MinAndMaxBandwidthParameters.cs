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
