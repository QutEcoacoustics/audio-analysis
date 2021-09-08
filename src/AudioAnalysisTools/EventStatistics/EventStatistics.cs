// <copyright file="EventStatistics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared.Csv;
    using AcousticWorkbench;
    using AnalysisBase.ResultBases;
    using CsvHelper.Configuration;

    /// <summary>
    /// The data class that holds event statistics.
    /// </summary>
    /// <remarks>
    /// Note that EventBase already has getters/setters for:
    /// TimeSpan SegmentStartOffset
    /// double Score
    /// double EventStartSeconds
    /// double? MinHz
    /// ..
    /// NOTE: When MinHz equals null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
    ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
    /// </remarks>
    public class EventStatistics : EventBase
    {
        public EventStatistics()
        {
            this.LowFrequencyHertz = 0;
        }

        public long? AudioEventId { get; set; }

        public long? AudioRecordingId { get; set; }

        public string ListenUrl
        {
            get
            {
                if (this.AudioRecordingId.HasValue)
                {
                    return Api.Default.GetListenUri(
                            this.AudioRecordingId.Value,
                            Math.Floor(this.ResultStartSeconds))
                        .ToString();
                }

                return string.Empty;
            }
        }

        public DateTimeOffset? AudioRecordingRecordedDate { get; set; }

        // Note: EventStartSeconds is in base class

        public double EventEndSeconds { get; set; }

        public double EventDurationSeconds => this.EventEndSeconds - this.EventStartSeconds;

        public DateTimeOffset? EventStartDate => this.AudioRecordingRecordedDate?.AddSeconds(this.ResultStartSeconds);

        public double MeanDecibels { get; set; }

        public double TemporalStdDevDecibels { get; set; }

        /// <summary>
        /// Gets or sets the relative location of the temporal max within the acoustic event.
        /// E.g. if temporal max is half way through the event then TemporalMaxRelative = 0.5.
        /// </summary>
        public double TemporalMaxRelative { get; set; }

        public new double LowFrequencyHertz { get; set; }

        /// <summary>
        /// Gets or sets the top frequency bound of the acoustic event in Hertz
        /// Note: MinHz implemented in base class.
        /// </summary>
        public double HighFrequencyHertz { get; set; }

        public double Bandwidth => this.HighFrequencyHertz - this.LowFrequencyHertz;

        public int DominantFrequency { get; set; }

        public double FreqBinStdDevDecibels { get; set; }

        /// <summary>
        /// Gets or sets the SpectralCentroid.
        /// The SpectralCentroid is a measure of the "brightness" of a sound event, that is, the relative amount of high freq content compared to low freq content.
        /// Note that this SpectralCentroid is calculated from a weighted average of decibel values and NOT power values.
        /// </summary>
        public int SpectralCentroid { get; set; }

        /// <summary>
        /// Gets or sets a measure of the distribution of energy over the time frames of the event.
        /// TemporalEnergyDistribution = 1 - Ht, where Ht is the temporal entropy calculated as for acoustic indices.
        /// Minimum value = 0.0, when energy is uniform over all time frames.
        /// Maximum value = 1.0, when all the acoustic energy is concentrated in a single time frame.
        /// </summary>
        public double TemporalEnergyDistribution { get; set; }

        /// <summary>
        /// Gets or sets a measure of the distribution of energy over the frequency bins of the event.
        /// SpectralEnergyDistribution = 1 - Hf, where Hf is the spectral entropy calculated as for acoustic indices.
        /// Minimum value = 0.0, when energy is uniform over all frequency bins.
        /// Maximum value = 1.0, when all the acoustic energy is concentrated in a single frequency bin.
        /// </summary>
        public double SpectralEnergyDistribution { get; set; }

        /// <summary>
        /// Gets or sets the event's signal-to-noise ratio in decibels.
        /// </summary>
        public double SnrDecibels { get; set; }

        public int SpectralPeakCount { get; set; }

        public bool Error { get; set; } = false;

        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a metadata field used for sorting results. Not serialized in CSV output.
        /// </summary>
        public int Order { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Sorts the results by their <see cref="P:AudioAnalysisTools.EventStatistics.EventStatistics.Order" /> property if it is available otherwise reverts to the base
        /// class comparison.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public override int CompareTo(ResultBase other)
        {
            if (other is EventStatistics otherEventStatistics)
            {
                var comaprison = this.Order.CompareTo(otherEventStatistics.Order);

                return comaprison == 0 ? base.CompareTo(other) : comaprison;
            }

            return base.CompareTo(other);
        }

        public override int CompareTo(object obj)
        {
            return this.CompareTo(obj as ResultBase);
        }

        public sealed class EventStatisticsClassMap : ClassMap<EventStatistics>
        {
            public EventStatisticsClassMap()
            {
                this.AutoMap(Csv.DefaultConfiguration);

                var ordered = new Dictionary<string, int>()
                {
                    { nameof(EventStatistics.AudioEventId), 0 },
                    { nameof(EventStatistics.AudioRecordingId), 1 },
                    { nameof(EventStatistics.EventStartSeconds), 2 },
                    { nameof(EventStatistics.EventEndSeconds), 3 },
                    { nameof(EventStatistics.LowFrequencyHertz), 4 },
                    { nameof(EventStatistics.HighFrequencyHertz), 5 },
                    { nameof(EventStatistics.Error), 999 },
                    { nameof(EventStatistics.ErrorMessage), 1000 },
                };

                var index = 6;
                foreach (var propertyMap in this.MemberMaps.OrderBy(x => x.Data.Member.Name))
                {
                    var name = propertyMap.Data.Member.Name;

                    if (name == nameof(EventBase.Score) || name == nameof(EventStatistics.Order))
                    {
                        propertyMap.Ignore();
                    }

                    if (name == nameof(EventStatistics.LowFrequencyHertz) &&
                        propertyMap.Data.Member.DeclaringType == typeof(EventBase))
                    {
                        propertyMap.Ignore();
                    }

                    if (ordered.TryGetValue(name, out var orderedIndex))
                    {
                        propertyMap.Data.Index = orderedIndex;
                    }
                    else
                    {
                        propertyMap.Data.Index = index;
                        index++;
                    }
                }
            }
        }
    }
}