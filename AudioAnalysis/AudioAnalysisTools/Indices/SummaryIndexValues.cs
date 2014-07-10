// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryIndexValues.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   This class is used to store the values of all indices regardless of type.
//   They are stored in dictionaries in order to make them accessible by key without having to write a special method each time a new index is created.
//   SOme of the functionality is in the parent class IndexBase.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security;

    using AnalysisBase.ResultBases;

    using CsvHelper.Configuration;

    using Fasterflect;

    using MathNet.Numerics.LinearAlgebra.Complex32.Solvers.Iterative;

    using NeuralNets;

    using TowseyLibrary;

    public class IndexCalculateResult
    {
        public IndexCalculateResult(TimeSpan wavDuration, int freqBinCount, Dictionary<string, IndexProperties> indexProperties, TimeSpan startOffset)
        {
            this.Hits = null;
            this.Tracks = null;
            this.TrackScores = new List<Plot>();

            this.SummaryIndexValues = new SummaryIndexValues(wavDuration, indexProperties)
                                          {
                                              // give the index a offset value so it can be sorted. 
                                              StartOffset = startOffset
                                          };
            this.SpectralIndexValues = new SpectralIndexValues(freqBinCount, indexProperties)
                                           {
                                               // give the index a offset value so it can be sorted. 
                                               StartOffset = startOffset
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
    /// SOme of the functionality is in the parent class IndexBase.
    /// </summary>
    public class SummaryIndexValues : SummaryIndexBase
    {
        public double TemporalEntropy { get; set; }

        public double HighAmplitudeIndex { get; set; }

        public double ClippingIndex { get; set; }

        public double Activity { get; set; }

        public double BackgroundNoise { get; set; }

        public double Snr { get; set; }

        public double AvgSnrOfActiveFrames { get; set; }

        public double AvgSignalAmplitude { get; set; }

        public double EventsPerSecond { get; set; }

        public TimeSpan AvgEventDuration { get; set; }

        public double AcousticComplexity { get; set; }

        public double AvgEntropySpectrum { get; set; }

        public double VarianceEntropySpectrum { get; set; }

        public double EntropyPeaks { get; set; }

        public double RainIndex { get; set; }

        public double CicadaIndex { get; set; }

        public double HighFreqCover { get; set; }

        public double MidFreqCover { get; set; }

        public double LowFreqCover { get; set; }

        public TimeSpan AvgSptDuration { get; set; }

        public double SptPerSecond { get; set; }

        public int ClusterCount { get; set; }

        public TimeSpan AvgClusterDuration { get; set; }

        public int ThreeGramCount { get; set; }

        private static Dictionary<string, Func<SummaryIndexValues, object>> CahcedSelectors { get;  set; }

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
            CahcedSelectors = ReflectionExtensions.GetGetters<SummaryIndexValues, object>();
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

                CachedSelectorsInternal.Add(keyValuePair.Key, spectrumBase  => selector((SpectralIndexValues)spectrumBase));
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
                this.SetPropertyValue(kvp.Key, initArray);
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

        public double[] ENT { get; set; }

        public double[] BGN { get; set; }

        public double[] AVG { get; set; }

        public double[] CVR { get; set; }

        public double[] EVN { get; set; }

        public double[] SPT { get; set; }

        public double[] CLS { get; set; }

        public override Dictionary<string, Func<SpectralIndexBase, double[]>> GetSelectors()
        {
            return CachedSelectors;
        }
    }
}
