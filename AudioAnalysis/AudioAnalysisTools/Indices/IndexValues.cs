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
    using System.Reflection;

    using AnalysisBase.ResultBases;

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

        public IndexValues IndexValues { get; set; }
        public SpectralValues SpectralValues { get; set; }
    }

    /// <summary>
    /// This class is used to store the values of all indices regardless of type.
    /// They are stored in dictionaries in order to make them accessible by key without having to write a special method each time a new index is created.
    /// SOme of the functionality is in the parent class IndexBase.
    /// </summary>
    public class IndexValues : SummaryIndexBase
    {

        // store indices in relevant dictionaries
        // dictionary to store summary indices of type double and int
       /* public void StoreIndex(string key, double val)
        {
            SummaryIndicesOfTypeDouble[key] = val;
        }
        public void StoreIndex(string key, int val)
        {
            SummaryIndicesOfTypeDouble[key] = (double)val;
        }
        // dictionary to store summary indices of type TimeSpan
        public void StoreIndex(string key, TimeSpan val)
        {
            SummaryIndicesOfTypeTimeSpan[key] = val;
        }
        public double[] GetSpectrum(string key)
        {
            return SpectralIndices[key];
        }
        public void AddSpectrum(string key, double[] spectrum)
        {
            if (this.SpectralIndices.ContainsKey(key))
            {
                this.SpectralIndices[key] = spectrum;
            }
            else
                this.SpectralIndices.Add(key, spectrum);
        }*/




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

        public TimeSpan AvgSPTDuration { get; set; }

        public double SPTPerSecond { get; set; }

        public int ClusterCount { get; set; }

        public TimeSpan AvgClusterDuration { get; set; }

        public int ThreeGramCount { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public IndexValues(int freqBinCount, TimeSpan wavDuration, FileInfo indexPropertiesConfig)
        {
            this.SegmentDuration = wavDuration;

            // initialise with default values stored values in the dictionary of index properties.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);

            foreach (string key in dictOfIndexProperties.Keys)
            {
                // no-op, nothing useful left to do;
            }

        }


        /*
        /// <summary>
        /// Initialise all vectors of spectral indices  
        /// </summary>
        /// <param name="size"></param>
        public Dictionary<string, double[]> InitialiseSpectra(int size, Dictionary<string, IndexProperties> dictOfIndexProperties)
        {

            Dictionary<string, double[]> spectra = new Dictionary<string, double[]>();
            foreach (string key in dictOfIndexProperties.Keys)
            {
                if (dictOfIndexProperties[key].DataType != typeof(double[]))
                {
                    continue; // only want spectral indices
                }

                double defaultValue = dictOfIndexProperties[key].DefaultValue;

                var spectrum = new double[size];
                if (defaultValue != 0.0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        spectrum[i] = defaultValue;
                    }
                }

                spectra.Add(key, spectrum);
            }

            return spectra;
        }*/

    }

    public class SpectralValues : SpectrumBase
    {
        public double[] ACI { get; set; }

        public double[] ENT { get; set; }

        public double[] BGN { get; set; }

        public double[] AVG { get; set; }

        public double[] CVR { get; set; }

        public double[] EVN { get; set; }

        public double[] SPT { get; set; }

        public double[] CLS { get; set; }

        private static readonly Dictionary<string, Func<SpectrumBase, double[]>> cachedSelectors;

        static SpectralValues()
        {
             Type thisType = typeof(SpectralValues);
            /*var selectors = new Func<SpectrumBase, object>[]
                                {
                                    (x => ((SpectralValues)x).ACI), (x => ((SpectralValues)x).ENT), (x => ((SpectralValues)x).BGN),
                                    (x => ((SpectralValues)x).AVG), (x => ((SpectralValues)x).CVR), (x => ((SpectralValues)x).EVN),
                                    (x => ((SpectralValues)x).SPT), (x => ((SpectralValues)x).CLS)
                                };*/

            var props = thisType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            cachedSelectors = new Dictionary<string, Func<SpectrumBase, double[]>>(props.Length);
            foreach (var propertyInfo in props)
            {
 
                if (propertyInfo.PropertyType != typeof(double[]))
                {
                    continue;
                }
                
                var methodInfo = propertyInfo.GetGetMethod();
                var getDelegate = (Func<SpectrumBase, double[]>)Delegate.CreateDelegate(thisType, methodInfo);
                var name = propertyInfo.Name;

                cachedSelectors.Add(name, getDelegate);
            }
        }

        public override Dictionary<string, Func<SpectrumBase, double[]>> GetSelectors()
        {
            return cachedSelectors;
        }
    }
}
