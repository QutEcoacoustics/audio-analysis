// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexValues.cs" company="QutBioacoustics">
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

    using AnalysisBase.ResultBases;

    using CsvHelper.Configuration;

    using MathNet.Numerics.LinearAlgebra.Complex32.Solvers.Iterative;

    using NeuralNets;

    using TowseyLibrary;

    public class IndexCalculateResult
    {
        // other possible results to store
        public BaseSonogram Sg { get; set; }

        public double[,] Hits { get; set; }

        public List<Plot> TrackScores { get; set; }

        public IndexCalculateResult()
        {
            Hits = null;
            Tracks = null;
            TrackScores = new List<Plot>();
        }

        public List<SpectralTrack> Tracks { get; set; }

        public SummaryIndexValues SummaryIndexValues { get; set; }
        public SpectralIndexValues SpectralIndexValues { get; set; }
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

        public static Dictionary<string, Func<SummaryIndexValues, object>> CahcedSelectors { get;  private set; }

        public SummaryIndexValues(int freqBinCount, TimeSpan wavDuration, FileInfo indexPropertiesConfig)
        {
            this.SegmentDuration = wavDuration;

            // initialise with default values stored values in the dictionary of index properties.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);

            foreach (string key in dictOfIndexProperties.Keys)
            {
                // no-op, nothing useful left to do;
            }
        }

        static SummaryIndexValues()
        {
            CahcedSelectors = ReflectionExtensions.GetGetters<SummaryIndexValues, object>();
        }

/*        private class SummaryIndexValuesMap : CsvClassMap<SpectralIndexValues>
        {
            public SummaryIndexValuesMap()
            {
                this.AutoMap();
                this.Map(m => m.CachedSelectors).Ignore();
            }
        }*/
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
