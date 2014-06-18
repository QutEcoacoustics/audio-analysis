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

    using AnalysisBase.ResultBases;

    using TowseyLibrary;

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



        // other possible results to store
        public BaseSonogram Sg { get; set; }

        private double[,] hits = null;
        public double[,] Hits
        {
            get { return this.hits; }
            set { this.hits = value; }
        }

        private List<Plot> trackScores = new List<Plot>();
        public List<Plot> TrackScores
        {
            get { return this.trackScores; }
            set { this.trackScores = value; }
        }

        private List<SpectralTrack> tracks = null;
        public List<SpectralTrack> Tracks
        {
            get { return this.tracks; }
            set { this.tracks = value; }
        }


        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public IndexValues(int freqBinCount, TimeSpan wavDuration, FileInfo indexPropertiesConfig)
        {
            SummaryIndicesOfTypeDouble   = new Dictionary<string, double>();
            SummaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();


            // initialise with default values stored values in the dictionary of index properties.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);

            foreach (string key in dictOfIndexProperties.Keys)
            {
                IndexProperties index = dictOfIndexProperties[key];

                if (index.DataType == typeof(double[])) continue; // ignore the spectral indices

                if (index.DataType == typeof(TimeSpan))
                {
                    this.SummaryIndicesOfTypeTimeSpan.Add(key, TimeSpan.Zero);
                }
                else 
                {
                    this.SummaryIndicesOfTypeDouble.Add(key, dictOfIndexProperties[key].DefaultValue);
                } 

            }

            this.SpectralIndices = this.InitialiseSpectra(freqBinCount, dictOfIndexProperties);
        }



        /// <summary>
        /// Initialise all vectors of spectral indices  
        /// </summary>
        /// <param name="size"></param>
        public Dictionary<string, double[]> InitialiseSpectra(int size, Dictionary<string, IndexProperties> dictOfIndexProperties)
        {

            Dictionary<string, double[]> spectra = new Dictionary<string, double[]>();
            foreach (string key in dictOfIndexProperties.Keys)
            {
                if (dictOfIndexProperties[key].DataType != typeof(double[])) continue; // only want spectral indices
                double defaultValue = dictOfIndexProperties[key].DefaultValue;

                var spectrum = new double[size];
                if (defaultValue != 0.0)
                {
                    for (int i = 0; i < size; i++) spectrum[i] = defaultValue; 
                }

                spectra.Add(key, spectrum);
            }
            return spectra;
        }

    } // class IndexValues : IndexBase
}
