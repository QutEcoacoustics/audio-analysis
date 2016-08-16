// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryIndexValues.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

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
                                              StartOffset =
                                                  subsegmentOffsetFromStartOfSource,
                                              SegmentDuration =
                                                  durationOfResult
                                          };
            this.SpectralIndexValues = new SpectralIndexValues(freqBinCount, indexProperties)
                                           {
                                               // give the results object an offset value so it can be sorted. 
                                               StartOffset =
                                                   subsegmentOffsetFromStartOfSource, 
                                               SegmentDuration =
                                                   durationOfResult
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
        public double VarianceEntropySpectrum { get; set; }  // this is old name for EntropyOfVarianceSpectrum

        public double EntropyOfPeaksSpectrum { get; set; }
        public double EntropyPeaks { get; set; } // this is old name for EntropyOfPeaksSpectrum

        public double EntropyOfCoVSpectrum { get; set; }

        // meaningless when calculated over short
        public int ClusterCount { get; set; }

        // public TimeSpan AvgClusterDuration { get; set; }

        public int ThreeGramCount { get; set; }

        // public double SptPerSecond { get; set; }

        //public TimeSpan AvgSptDuration { get; set; }

		/// Normalised difference soundscape Index
        public double NDSI { get; set; }

        public double SptDensity { get; set; }

        //public double RainIndex { get; set; }

        //public double CicadaIndex { get; set; }

        private static Dictionary<string, Func<SummaryIndexValues, object>> CachedSelectors { get; set; }

        public SummaryIndexValues()
        {
            // serialization entry
        }

        public SummaryIndexValues(TimeSpan wavDuration, Dictionary<string, IndexProperties> indexProperties)
        {
            this.SegmentDuration = wavDuration;

            // initialise with default values stored values in the dictionary of index properties.
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

        static SummaryIndexValues()
        {
            CachedSelectors = ReflectionExtensions.GetGetters<SummaryIndexValues, object>();
        }
    }

    public class SpectralIndexValues : SpectralIndexBase
    {
        private static readonly Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectorsInternal;

        static SpectralIndexValues()
        {
            var getters = ReflectionExtensions.GetGetters<SpectralIndexValues, double[]>();

            CachedSelectorsInternal = new Dictionary<string, Func<SpectralIndexBase, double[]>>(getters.Count);
            foreach (var keyValuePair in getters)
            {
                var key = keyValuePair.Key;
                var selector = keyValuePair.Value;

                CachedSelectorsInternal.Add(
                    keyValuePair.Key, 
                    spectrumBase => selector((SpectralIndexValues)spectrumBase));
            }
        }

        public SpectralIndexValues()
        {
            // empty constructor important!
        }

        public SpectralIndexValues(int spectrumLength, Dictionary<string, IndexProperties> indexProperties)
        {
            foreach (var kvp in indexProperties)
            {
                if (!kvp.Value.IsSpectralIndex)
                {
                    continue;
                }

                double[] initArray = (new double[spectrumLength]).FastFill(kvp.Value.DefaultValue);

                // WARNING: Potential throw site
                // No need to give following warning because should call CheckExistenceOfSpectralIndexValues() method before entering loop.
                // This prevents multiple warnings through loop.
                this.SetPropertyValue(kvp.Key, initArray);
            }
        }

        /// <summary>
        /// Imports a dictionary of spectra.
        /// Assumes `CheckExistenceOfSpectralIndexValues` has already been called
        /// </summary>
        /// <param name="dictionaryOfSpectra">
        /// The dictionary to convert to spectral index base
        /// </param>
        public void ImportFromDictionary(Dictionary<string, double[,]> dictionaryOfSpectra)
        {
            // warning: default values won't be set!
            foreach (var spectrum in dictionaryOfSpectra)
            {
                // WARNING: Potential throw site
                this.SetPropertyValue(spectrum.Key, spectrum.Value);
            }
        }

        /// <summary>
        /// Used to check that the keys in the indexProperties dictionary correspond to Properties in the SpectralIndexValues class.
        /// Call this method before entering a loop because do not want the error message at every iteration through loop.
        /// </summary>
        /// <param name="indexProperties">
        /// </param>
        public static void CheckExistenceOfSpectralIndexValues(Dictionary<string, IndexProperties> indexProperties)
        {
            var siv = new SpectralIndexValues();
            double[] dummyArray = null;

            foreach (var kvp in indexProperties)
            {
                if (!kvp.Value.IsSpectralIndex)
                {
                    continue;
                }

                var success = siv.TrySetPropertyValue(kvp.Key, dummyArray);
                if (!success)
                {
                    LoggedConsole.WriteWarnLine(
                        "### WARNING: The PROPERTY <" + kvp.Key + "> does not exist in the SpectralIndexValues class!");
                }
            }
        }

        public static Dictionary<string, Func<SpectralIndexBase, double[]>> CachedSelectors
        {
            get
            {
                return CachedSelectorsInternal;
            }
        }

        public double[] ACI { get; set; }

        public double[] BGN { get; set; }

        public double[] CVR { get; set; }

        public double[] DIF { get; set; }

        public double[] ENT { get; set; }

        public double[] EVN { get; set; }

        public double[] POW { get; set; }

        /// Spectral Ridges Horizontal
        public double[] RHZ { get; set; } 

        /// Spectral Ridges Vertical
        public double[] RVT { get; set; } 

        /// Spectral Ridges Positive slope
        public double[] RPS { get; set; }

        /// Spectral Ridges Negative Slope
        public double[] RNG { get; set; }

        // Spectral Peak Tracks
        public double[] SPT { get; set; } 

        public double[] SUM { get; set; }

        public double[] CLS { get; set; }
        public override Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors()
        {
            return CachedSelectors;
        }
    }
}
