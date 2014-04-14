using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using TowseyLibrary;
using AnalysisBase;
//using AudioAnalysisTools;



namespace AudioAnalysisTools
{

    public class IndexStore : IndexBase
    {
        ////these dictionaries used to store index values accessible by key
        //private Dictionary<string, double>   summaryIndicesOfTypeDouble   = new Dictionary<string, double>();
        //public Dictionary<string, double>    SummaryIndicesOfTypeDouble   { get; private set; } 
        //private Dictionary<string, int>      summaryIndicesOfTypeInt      = new Dictionary<string, int>();
        //public Dictionary<string, int>       SummaryIndicesOfTypeInt      { get; private set; } 
        //private Dictionary<string, TimeSpan> summaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();
        //public Dictionary<string, TimeSpan>  SummaryIndicesOfTypeTimeSpan { get; private set; }

        ///// <summary>
        ///// for storing spectral indices in a dictionary
        ///// </summary>
        //private Dictionary<string, double[]> spectralIndices;
        //public Dictionary<string, double[]> SpectralIndices
        //{
        //    get { return spectralIndices; }
        //    set { spectralIndices = value; }
        //}


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



        public double[] GetSpectrum(string key)
        {
            return SpectralIndices[key];
        }
        public void AddSpectrum(string key, double[] spectrum)
        {
            if(this.SpectralIndices.ContainsKey(key))
            {
                this.SpectralIndices[key] = spectrum;
            }
            else
            this.SpectralIndices.Add(key, spectrum);
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public IndexStore(int freqBinCount, TimeSpan wavDuration)
        {
            SummaryIndicesOfTypeDouble   = new Dictionary<string, double>();
            SummaryIndicesOfTypeInt      = new Dictionary<string, int>();
            SummaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();


            // initiliase the stored values in the index dictionaries.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.InitialisePropertiesOfIndices();

            foreach (string key in dictOfIndexProperties.Keys)
            {
                IndexProperties index = dictOfIndexProperties[key];
                double defaultValue = dictOfIndexProperties[key].DefaultValue;

                if(index.DataType == typeof(double))
                {
                    this.SummaryIndicesOfTypeDouble.Add(key, defaultValue);
                } else
                if(index.DataType == typeof(int))
                {
                    this.SummaryIndicesOfTypeInt.Add(key, (int)defaultValue);
                }
                else
                if(index.DataType == typeof(TimeSpan))
                {
                    this.SummaryIndicesOfTypeTimeSpan.Add(key, TimeSpan.Zero);
                }
            }

            this.SpectralIndices = InitialiseSpectra(freqBinCount);
        }

        // store indices in relevant dictionaries
        public void StoreIndex(string key, double val)
        {
            SummaryIndicesOfTypeDouble[key] = val;
        }
        public void StoreIndex(string key, int val)
        {
            SummaryIndicesOfTypeInt[key] = val;
        }
        public void StoreIndex(string key, TimeSpan val)
        {
            SummaryIndicesOfTypeTimeSpan[key] = val;
        }

        //// get any index as a double
        //public double GetIndex(string key)
        //{
        //    if (SummaryIndicesOfTypeDouble.ContainsKey(key))   return SummaryIndicesOfTypeDouble[key];
        //    if (SummaryIndicesOfTypeInt.ContainsKey(key))      return (double)SummaryIndicesOfTypeInt[key];
        //    if (SummaryIndicesOfTypeDouble.ContainsKey(key))   return SummaryIndicesOfTypeDouble[key];
        //    if (SummaryIndicesOfTypeTimeSpan.ContainsKey(key)) return SummaryIndicesOfTypeTimeSpan[key].TotalMilliseconds;
        //    return 0.0;
        //}

        //// get indices from relevant dictionaries
        //public double GetIndexAsDouble(string key)
        //{
        //    return SummaryIndicesOfTypeDouble[key];
        //}
        //public int GetIndexAsInteger(string key)
        //{
        //    return SummaryIndicesOfTypeInt[key];
        //}
        //public TimeSpan GetIndexAsTimeSpan(string key)
        //{
        //     return SummaryIndicesOfTypeTimeSpan[key];
        //}

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
            //DataTableTools.WriteTable2ConsoleInLongLayout(dt); // DEBUG
            return dt;
        }


    }
}
