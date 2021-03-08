// <copyright file="CommonParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools.DSP;
    using TowseyLibrary;

    /// <summary>
    /// Common parameters needed from a config file to detect components.
    /// </summary>
    public abstract class CommonParameters : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the name species name to give to a component.
        /// Leave blank if you're don't want an event to have a species name.
        /// </summary>
        public string SpeciesName { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window size, i.e. number of signal samples. Must be power of 2. Typically <c>512</c>.
        /// </summary>
        /// <value>The size of the window (frame) in samples.</value>.
        public int? FrameSize { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window step i.e. before start of next frame.
        /// The overlap can be any number of samples but less than <see cref="FrameSize"/>.
        /// </summary>
        /// <value>The size of the window step in samples.</value>.
        public int? FrameStep { get; set; }

        /// <summary>
        /// Gets or sets the windowing function used in conjunction with the FFT when making spectrogram.
        /// This can have quite an impact in some cases so it is worth giving user the option.
        /// The default is a <see cref="WindowFunctions.HANNING"/> window.
        /// </summary>
        public WindowFunctions? WindowFunction { get; set; } = WindowFunctions.HANNING;

        /// <summary>
        /// Gets or sets the threshold in decibels which determines signal over
        /// background noise.
        /// </summary>
        public double? BgNoiseThreshold { get; set; }

        /// <summary>snr
        /// Gets or sets the bottom bound of the rectangle. Units are Hertz.
        /// </summary>
        public int? MinHertz { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of the rectangle. Units are Hertz.
        /// </summary>
        public int? MaxHertz { get; set; }

        /// <summary>
        /// Gets or sets the buffer (bandwidth of silence) below the component rectangle. Units are Hertz.
        /// </summary>
        public int? BottomHertzBuffer { get; set; }

        /// <summary>
        /// Gets or sets the buffer (bandwidth of silence) above the component rectangle. Units are Hertz.
        /// Quite often this will be set to <value>null</value> which indicates as upper bounds variable,
        /// depending on distance of the source.
        /// </summary>
        public int? TopHertzBuffer { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed duration of the component. Units are seconds.
        /// </summary>
        public double? MinDuration { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the maximum allowed duration of the component. Units are seconds.
        /// </summary>
        public double? MaxDuration { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets an array of decibel thresholds.
        /// Each threshold determines the minimum "loudness" of an event that can be detected.
        /// Units are decibels.
        /// </summary>
        public double?[] DecibelThresholds { get; set; }

        /// <summary>
        /// The type of noise reduction to use.
        /// Defaults to <see cref="NoiseReductionType.Standard"/>.
        /// </summary>
        /// <value>One of the <see cref="NoiseReductionType"/> values.</value>
        public NoiseReductionType? NoiseReductionType { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return this.MinHertz.ValidateNotNull(nameof(this.MinHertz));
            yield return this.MaxHertz.ValidateNotNull(nameof(this.MaxHertz));
            yield return this.ValidateLessThan(this.MinHertz, nameof(this.MinHertz), this.MaxHertz, nameof(this.MaxHertz));
            yield return this.DecibelThresholds.ValidateNotNull(nameof(this.DecibelThresholds));
            yield return this.DecibelThresholds.ValidateNotEmpty(nameof(this.DecibelThresholds));
        }
    }
}