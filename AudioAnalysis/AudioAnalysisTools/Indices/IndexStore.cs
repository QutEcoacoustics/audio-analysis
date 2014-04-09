using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using TowseyLibrary;
using AnalysisBase;
using AudioAnalysisTools;



namespace AudioAnalysisTools
{

    public class IndexStore
    {
        //these dictionaries used to store index values accessible by key
        private Dictionary<string, double>   summaryIndicesOfTypeDouble   = new Dictionary<string, double>();
        private Dictionary<string, int>      summaryIndicesOfTypeInt      = new Dictionary<string, int>();
        private Dictionary<string, TimeSpan> summaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();


        public BaseSonogram Sg { get; set; }

        private double[,] hits = null;
        public double[,] Hits
        {
            get { return hits; }
            set { hits = value; }
        }

        private List<Plot> trackScores = new List<Plot>();
        public List<Plot> TrackScores
        {
            get { return trackScores; }
            set { trackScores = value; }
        }

        private List<SpectralTrack> tracks = null;
        public List<SpectralTrack> Tracks
        {
            get { return tracks; }
            set { tracks = value; }
        }




        /// <summary>
        /// for storing spectral indices in a dictionary
        /// </summary>
        private Dictionary<string, double[]> spectra;
        public Dictionary<string, double[]> Spectra
        {
            get { return spectra; }
            set { spectra = value; }
        }

        public double[] GetSpectrum(string key)
        {
            return spectra[key];
        }
        public void AddSpectrum(string key, double[] spectrum)
        {
            if(this.spectra.ContainsKey(key))
            {
                this.spectra[key] = spectrum;
            }
            else
            this.spectra.Add(key, spectrum);
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public IndexStore(int freqBinCount, TimeSpan wavDuration)
        {
            // initiliase the stored values in the index dictionaries.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.InitialisePropertiesOfIndices();

            foreach (string key in dictOfIndexProperties.Keys)
            {
                IndexProperties index = dictOfIndexProperties[key];
                double defaultValue = dictOfIndexProperties[key].DefaultValue;

                if(index.DataType == typeof(double))
                {
                    this.summaryIndicesOfTypeDouble.Add(key, defaultValue);
                } else
                if(index.DataType == typeof(int))
                {
                    this.summaryIndicesOfTypeInt.Add(key, (int)defaultValue);
                }
                else
                if(index.DataType == typeof(TimeSpan))
                {
                    this.summaryIndicesOfTypeTimeSpan.Add(key, TimeSpan.Zero);
                }
            }

            this.Spectra = InitialiseSpectra(freqBinCount);
        }

        // store indices in relevant dictionaries
        public void StoreIndex(string key, double val)
        {
            summaryIndicesOfTypeDouble[key] = val;
        }
        public void StoreIndex(string key, int val)
        {
            summaryIndicesOfTypeInt[key] = val;
        }
        public void StoreIndex(string key, TimeSpan val)
        {
            summaryIndicesOfTypeTimeSpan[key] = val;
        }

        // get any index as a double
        public double GetIndex(string key)
        {
            if (summaryIndicesOfTypeDouble.ContainsKey(key))   return summaryIndicesOfTypeDouble[key];
            if (summaryIndicesOfTypeInt.ContainsKey(key))      return (double)summaryIndicesOfTypeInt[key];
            if (summaryIndicesOfTypeDouble.ContainsKey(key))   return summaryIndicesOfTypeDouble[key];
            if (summaryIndicesOfTypeTimeSpan.ContainsKey(key)) return summaryIndicesOfTypeTimeSpan[key].TotalMilliseconds;
            return 0.0;
        }

        // get indices from relevant dictionaries
        public double GetIndexAsDouble(string key)
        {
            return summaryIndicesOfTypeDouble[key];
        }
        public int GetIndexAsInteger(string key)
        {
            return summaryIndicesOfTypeInt[key];
        }
        public TimeSpan GetIndexAsTimeSpan(string key)
        {
             return summaryIndicesOfTypeTimeSpan[key];
        }

        //==============================================================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================
        //======= STATIC METHODS BELOW HERE=============================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================

        
        /// <summary>
        /// The following are vectors of spectral indices  
        /// NB: if you create a new spectrum of indices you need to put reference for saving it in AcousticIndices.Analyse() method, line 379
        /// double[] spectrum_BGN, spectrum_ACI, spectrum_AVG, spectrum_VAR, spectrum_CVR, spectrum_ENT, spectrum_CLS;
        /// </summary>
        /// <param name="size"></param>
        public static Dictionary<string, double[]> InitialiseSpectra(int size)
        {
            string[] keys = SpectrogramConstants.ALL_KNOWN_KEYS.Split('-');

            Dictionary<string, double[]> spectra = new Dictionary<string, double[]>();
            foreach (string key in keys)
            {
                var spectrum = new double[size];
                if (key == SpectrogramConstants.KEY_BackgroundNoise)
                {
                    for (int i = 0; i < size; i++) spectrum[i] = -150; // set rock bottom BGN level in decibels
                } else
                if (key == SpectrogramConstants.KEY_TemporalEntropy)
                {
                    for (int i = 0; i < size; i++) spectrum[i] = 1.0; // this is default for temporal entropy
                }

                spectra.Add(key, spectrum);
            }
            return spectra;
        }

        public static DataTable Indices2DataTable(IndexStore indicesStore)
        {
            Dictionary<string, IndexProperties> properties = IndexProperties.InitialisePropertiesOfIndices();
            var headers = IndexProperties.GetArrayOfIndexNames(properties);
            var types = IndexProperties.GetArrayOfIndexTypes(properties);
            string[] keys = properties.Keys.ToArray();

            var dt = DataTableTools.CreateTable(headers, types);

            DataRow row = dt.NewRow();
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                IndexProperties prop = properties[key];

                if (prop.DataType == typeof(double)) 
                {
                    row[headers[i]] = indicesStore.GetIndexAsDouble(key);              
                }
                else if(prop.DataType == typeof(TimeSpan)) 
                {
                    row[headers[i]] = indicesStore.GetIndexAsTimeSpan(key);
                }
                else if (prop.DataType == typeof(int)) 
                {
                    row[headers[i]] = indicesStore.GetIndexAsInteger(key);
                }
            }
            dt.Rows.Add(row);
            return dt;
        }


    }
}
