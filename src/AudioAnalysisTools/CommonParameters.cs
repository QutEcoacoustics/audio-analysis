// <copyright file="CommonParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using TowseyLibrary;

    /// <summary>
    /// Common parameters needed from a config file to detect components.
    /// </summary>
    public abstract class CommonParameters
    {
        /// <summary>
        /// Gets or sets the name species name to give to a component.
        /// Leave blank if you're don't want an event to have a species name.
        /// </summary>
        public string SpeciesName { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window size, i.e. number of signal samples. Must be power of 2. Typically 512.
        /// </summary>
        public int? FrameSize { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window step i.e. before start of next frame.
        /// The overlap can be any number of samples but less than the frame length/size.
        /// </summary>
        public int? FrameStep { get; set; }

        /// <summary>
        /// Gets or sets the windowing funciton used in conjunction with the FFT when making spectrogram.
        /// This can have quite an impact in some cases so it is worth giving user the option.
        /// The default is a HAMMIN window.
        /// </summary>
        public string WindowFunction { get; set; } = WindowFunctions.HAMMING.ToString();

        /// <summary>
        /// Gets or sets the threshold in decibels which determines signal over
        /// background noise.
        /// </summary>
        public double? BgNoiseThreshold { get; set; }

        /// <summary>
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
        /// Gets or sets the threshold of "loudness" of a component. Units are decibels.
        /// </summary>
        public double? DecibelThreshold { get; set; } = 6;

        /// <summary>
        /// Gets or sets the maximum score for an event.
        /// Setting this value sets a normalised score value for the event.
        /// The normalised score is a linear conversion from 0 - maxScore to [0, 1].
        /// </summary>
        public double? MaxScore { get; set; }
    }
}