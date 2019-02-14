// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryIndexValues.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnalysisBase.ResultBases;
    using DSP;
    using Fasterflect;
    using StandardSpectrograms;
    using TowseyLibrary;

    public class IndexCalculateResult
    {
        public IndexCalculateResult(
            int freqBinCount,
            Dictionary<string, IndexProperties> indexProperties,
            TimeSpan indexCalculationDuration,
            TimeSpan subsegmentOffset,
            IndexCalculateConfig configuration)
        {
            TimeSpan durationOfResult = indexCalculationDuration; // subsegment TimeSpan
            TimeSpan subsegmentOffsetFromStartOfSource = subsegmentOffset; // offset from beginning of source audio

            this.Hits = null;
            this.Tracks = null;
            this.TrackScores = new List<Plot>();
            this.AmplitudeSpectrogram = null;

            this.SummaryIndexValues = new SummaryIndexValues(durationOfResult, indexProperties)
                                          {
                                              // give the results object an offset value so it can be sorted.
                                              ResultStartSeconds =
                                                  subsegmentOffsetFromStartOfSource.TotalSeconds,
                                              SegmentDurationSeconds =
                                                  durationOfResult.TotalSeconds,
                                          };

            this.SpectralIndexValues = new SpectralIndexValues(freqBinCount, indexProperties, configuration)
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

        public double[,] AmplitudeSpectrogram { get; set; }
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
        /// All summary indices initialised to zero except BackgroundNoise and AvgSignalAmplitude both = -100 dB.
        /// </summary>
        public SummaryIndexValues()
        {
            // serialization entry
            this.BackgroundNoise = SNR.MinimumDbBoundForZeroSignal;    // equals -100 dB, Feb 2019;
            this.AvgSignalAmplitude = SNR.MinimumDbBoundForZeroSignal;
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

        /// <summary>
        /// Put SUMMARY indices into dictionary.
        /// ################# WARNING: THIS METHOD ONLY GETS A "HARD CODED" LIST OF SUMMARY INDICES. See the method.
        /// TODO need to generalise the following method.
        /// </summary>
        /// <param name="summaryIndices">a list of summary index values ordered by minute segments and not by name of index.</param>
        /// <returns>a dictionary whose keys are summary index names and values are arrays of double.</returns>
        public static Dictionary<string, double[]> ConvertToDictionaryOfSummaryIndices(List<SummaryIndexValues> summaryIndices)
        {
            // Put SUMMARY indices into dictionary.
            var dictionaryOfSummaryIndices = new Dictionary<string, double[]>
            {
                { GapsAndJoins.KeyZeroSignal, summaryIndices.Select(x => x.ZeroSignal).ToArray() },
                { "ClippingIndex", summaryIndices.Select(x => x.ClippingIndex).ToArray() },
                //{ "HighAmplitudeIndex", summaryIndices.Select(x => x.HighAmplitudeIndex).ToArray() },
                //{ "AvgSignalAmplitude", summaryIndices.Select(x => x.AvgSignalAmplitude).ToArray() },
                { "BackgroundNoise", summaryIndices.Select(x => x.BackgroundNoise).ToArray() },
                { "Snr", summaryIndices.Select(x => x.Snr).ToArray() },
                { "AvgSnrOfActiveFrames", summaryIndices.Select(x => x.AvgSnrOfActiveFrames).ToArray() },
                { "Activity", summaryIndices.Select(x => x.Activity).ToArray() },
                { "EventsPerSecond", summaryIndices.Select(x => x.EventsPerSecond).ToArray() },
                { "HighFreqCover", summaryIndices.Select(x => x.HighFreqCover).ToArray() },
                { "MidFreqCover", summaryIndices.Select(x => x.MidFreqCover).ToArray() },
                { "LowFreqCover", summaryIndices.Select(x => x.LowFreqCover).ToArray() },
                { "AcousticComplexity", summaryIndices.Select(x => x.AcousticComplexity).ToArray() },
                { "TemporalEntropy", summaryIndices.Select(x => x.TemporalEntropy).ToArray() },
                { "EntropyOfAverageSpectrum", summaryIndices.Select(x => x.EntropyOfAverageSpectrum).ToArray() },
                { "EntropyOfPeaksSpectrum", summaryIndices.Select(x => x.EntropyOfPeaksSpectrum).ToArray() },
                { "ClusterCount", summaryIndices.Select(x => x.ClusterCount).ToArray() },
                { "ThreeGramCount", summaryIndices.Select(x => x.ThreeGramCount).ToArray() },
                { "SptDensity", summaryIndices.Select(x => x.SptDensity).ToArray() },
                { "Ndsi", summaryIndices.Select(x => x.Ndsi).ToArray() },
            };

            // Now add in derived indices i.e. NCDI etc
            // Decided NOT to do this anymore
            // dictionaryOfSummaryIndices = IndexMatrices.AddDerivedIndices(dictionaryOfSummaryIndices);

            // return the dictionary - it will be used later to produce an index tracks image.
            return dictionaryOfSummaryIndices;
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

        public double HighFreqCover { get; set; }

        public double MidFreqCover { get; set; }

        public double LowFreqCover { get; set; }

        public double AcousticComplexity { get; set; }

        public double TemporalEntropy { get; set; }

        public double EntropyOfAverageSpectrum { get; set; }

        public double EntropyOfVarianceSpectrum { get; set; }

        public double EntropyOfPeaksSpectrum { get; set; }

        public double EntropyOfCoVSpectrum { get; set; }

        // meaningless when calculated over short
        public double ClusterCount { get; set; }

        public double ThreeGramCount { get; set; }

        // Normalised difference soundscape Index
        public double Ndsi { get; set; }

        public double SptDensity { get; set; }

        private static Dictionary<string, Func<SummaryIndexValues, object>> CachedSelectors { get; set; }
    }
}
