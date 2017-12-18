// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryIndexValues.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.StandardSpectrograms;

    using Fasterflect;
    using TowseyLibrary;

    public class IndexCalculateResult
    {
        public IndexCalculateResult(
            int freqBinCount,
            Dictionary<string, IndexProperties> indexProperties,
            TimeSpan indexCalculationDuration,
            TimeSpan subsegmentOffset)
        {
            TimeSpan durationOfResult = indexCalculationDuration; // subsegment TimeSpan

            // TimeSpan startOffset = analysisSettings.SegmentStartOffset.Value; // offset from beginning of source audio
            TimeSpan subsegmentOffsetFromStartOfSource = subsegmentOffset; // offset from beginning of source audio

            this.Hits = null;
            this.Tracks = null;
            this.TrackScores = new List<Plot>();

            this.SummaryIndexValues = new SummaryIndexValues(durationOfResult, indexProperties)
                                          {
                                              // give the results object an offset value so it can be sorted.
                                              ResultStartSeconds =
                                                  subsegmentOffsetFromStartOfSource.TotalSeconds,
                                              SegmentDurationSeconds =
                                                  durationOfResult.TotalSeconds,
                                          };

            this.SpectralIndexValues = new SpectralIndexValues(freqBinCount, indexProperties)
                                           {
                                               // give the results object an offset value so it can be sorted.
                                               ResultStartSeconds =
                                                   subsegmentOffsetFromStartOfSource.TotalSeconds,
                                               SegmentDurationSeconds =
                                                   durationOfResult.TotalSeconds,
                                           };
        }

        public List<SpectralTrack> Tracks { get; set; }

        public SummaryIndexValues SummaryIndexValues { get; private set; }

        public SpectralIndexValues SpectralIndexValues { get; private set; }

        // other possible results to store
        public BaseSonogram Sg { get; set; }

        public double[,] Hits { get; set; }

        public List<Plot> TrackScores { get; set; }
    }

    /// <summary>
    /// This class is used to store the values of all indices regardless of type.
    /// They are stored in dictionaries in order to make them accessible by key without having to write a special method each time a new index is created.
    /// Some of the functionality is in the parent class IndexBase.
    /// </summary>
    public class SummaryIndexValues : SummaryIndexBase
    {
        static SummaryIndexValues()
        {
            CachedSelectors = ReflectionExtensions.GetGetters<SummaryIndexValues, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryIndexValues"/> class.
        /// All summary indices initialised to zero except background and av Sig AMplitude both = -100 dB.
        /// </summary>
        public SummaryIndexValues()
        {
            // serialization entry
            this.BackgroundNoise = -100;
            this.AvgSignalAmplitude = -100;
        }

        public SummaryIndexValues(TimeSpan wavDuration, Dictionary<string, IndexProperties> indexProperties)
            : this()
        {
            this.SegmentDurationSeconds = wavDuration.TotalSeconds;

            // initialize with default values stored values in the dictionary of index properties.
            foreach (var kvp in indexProperties)
            {
                // do not process spectral indices properties
                // don't bother with slow reflection if the default is 0.0
                if (kvp.Value.IsSpectralIndex || kvp.Value.DefaultValue == default(double))
                {
                    continue;
                }

                this.SetPropertyValue(kvp.Key, kvp.Value.DefaultValueCasted);
            }
        }

        public double ZeroSignal { get; set; }

        public double HighAmplitudeIndex { get; set; }

        public double ClippingIndex { get; set; }

        public double AvgSignalAmplitude { get; set; }

        public double BackgroundNoise { get; set; }

        public double Snr { get; set; }

        public double AvgSnrOfActiveFrames { get; set; }

        public double Activity { get; set; }

        public double EventsPerSecond { get; set; }

        // Commented out on 2nd Feb 2015.
        // AvgEventDuration is no longer accurately calculated now that estimating it on subsegments of < 1 second duration.
        // public TimeSpan AvgEventDuration { get; set; }

        public double HighFreqCover { get; set; }

        public double MidFreqCover { get; set; }

        public double LowFreqCover { get; set; }

        public double AcousticComplexity { get; set; }

        public double TemporalEntropy { get; set; }

        public double EntropyOfAverageSpectrum { get; set; } // this is new more accurate name

        public double AvgEntropySpectrum { get; set; } // this is old name for EntropyOfAverageSpectrum

        public double EntropyOfVarianceSpectrum { get; set; }

        public double VarianceEntropySpectrum { get; set; } // this is old name for EntropyOfVarianceSpectrum

        public double EntropyOfPeaksSpectrum { get; set; }

        public double EntropyPeaks { get; set; } // this is old name for EntropyOfPeaksSpectrum

        public double EntropyOfCoVSpectrum { get; set; }

        // meaningless when calculated over short
        public double ClusterCount { get; set; }

        // public TimeSpan AvgClusterDuration { get; set; }

        public double ThreeGramCount { get; set; }

        // public double SptPerSecond { get; set; }

        //public TimeSpan AvgSptDuration { get; set; }

        // Normalised difference soundscape Index
        public double Ndsi { get; set; }

        public double SptDensity { get; set; }

        //public double RainIndex { get; set; }

        //public double CicadaIndex { get; set; }

        private static Dictionary<string, Func<SummaryIndexValues, object>> CachedSelectors { get; set; }
    }
}
