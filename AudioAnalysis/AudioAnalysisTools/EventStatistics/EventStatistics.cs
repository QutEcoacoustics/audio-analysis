// <copyright file="EventStatistics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;
    using AcousticWorkbench;
    using AnalysisBase.ResultBases;
    using DSP;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// Note that EventBase already has getters/setters for:
    /// TimeSpan SegmentStartOffset
    /// double Score
    /// double EventStartSeconds
    /// double? MinHz
    /// ..
    /// NOTE: When MinHz equals null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
    ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
    /// </summary>
    public class EventStatistics : EventBase
    {
        public EventStatistics()
        {
            base.MinHz = 0;
        }

        public int? AudioRecordingId { get; set; }

        public string ListenUrl
        {
            get
            {
                if (this.AudioRecordingId.HasValue)
                {
                    return Api.Default.GetListenUri(
                        this.AudioRecordingId.Value,
                        this.StartOffset.TotalSeconds).ToString();
                }

                return string.Empty;
            }
        }

        public DateTimeOffset? AudioRecordingDateTime { get; set; }

        // Note: EventStartSeconds is in base class

        public double EventEndSeconds { get; set; }

        public double DurationSeconds => this.EventEndSeconds - this.EventStartSeconds;

        public DateTimeOffset? StartDateTime => this.AudioRecordingDateTime?.Add(this.StartOffset);

        public double MeanDecibels { get; set; }

        public double TemporalStdDevDecibels { get; set; }

        /// <summary>
        /// Gets or sets the relative location of the temporal max within the acoustic event.
        /// E.g. if temporal max is half way through the event then TemporalMaxRelative = 0.5
        /// </summary>
        public double TemporalMaxRelative { get; set; }

        // Note: MinHz implemented in base class.
        public new double MinHz
        {
            get => base.MinHz.Value;
            set => base.MinHz = value;
        }

        /// <summary>
        /// Gets or sets the top frequency bound of the acoustic event in Hertz
        /// </summary>
        public double MaxHz { get; set; }

        public double BandWidth => this.MaxHz - this.MinHz;

        public int DominantFrequency { get; set; }

        public double FreqBinStdDevDb { get; set; }

        /// <summary>
        /// Gets or sets the SpectralCentroid.
        /// The SpectralCentroid is a measure of the "brightness" of a sound event, that is, the relative amount of high freq content compared to low freq content.
        /// Note that this SpectralCentroid is calculated from a weighted average of decibel values and NOT power values
        /// </summary>
        public int SpectralCentroid { get; set; }

        /// <summary>
        /// Gets or sets a measure of the distribution of energy over the time frames of the event.
        /// TemporalEnergyDistribution = 1 - Ht, where Ht is the temporal entropy calculated as for acoustic indices.
        /// Minimum value = 0.0, when energy is unifrom over all time frames.
        /// Maximum value = 1.0, when all the acoustic energy is concentrated in a single time frame.
        /// </summary>
        public double TemporalEnergyDistribution { get; set; }

        /// <summary>
        /// Gets or sets a measure of the distribution of energy over the frequency bins of the event.
        /// SpectralEnergyDistribution = 1 - Hf, where Hf is the spectral entropy calculated as for acoustic indices.
        /// Minimum value = 0.0, when energy is unifrom over all frequency bins.
        /// Maximum value = 1.0, when all the acoustic energy is concentrated in a single frequency bin.
        /// </summary>
        public double SpectralEnergyDistribution { get; set; }

        /// <summary>
        /// Gets or sets the event's signal-to-noise ratio in decibels.
        /// </summary>
        public double SnrDecibels { get; set; }
    }
}
