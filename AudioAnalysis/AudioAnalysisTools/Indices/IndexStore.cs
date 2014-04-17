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
            SummaryIndicesOfTypeTimeSpan = new Dictionary<string, TimeSpan>();


            // initialise the stored values in the index dictionaries.
            Dictionary<string, IndexProperties> dictOfIndexProperties = IndexProperties.InitialisePropertiesOfIndices();

            foreach (string key in dictOfIndexProperties.Keys)
            {
                IndexProperties index = dictOfIndexProperties[key];

                if(index.DataType == typeof(TimeSpan))
                {
                    this.SummaryIndicesOfTypeTimeSpan.Add(key, TimeSpan.Zero);
                }
                else 
                {
                    this.SummaryIndicesOfTypeDouble.Add(key, dictOfIndexProperties[key].DefaultValue);
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
            SummaryIndicesOfTypeDouble[key] = (double)val;
        }
        public void StoreIndex(string key, TimeSpan val)
        {
            SummaryIndicesOfTypeTimeSpan[key] = val;
        }


        //==============================================================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================
        //======= STATIC METHODS BELOW HERE=============================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================
        //==============================================================================================================================================

        
        /// <summary>
        /// Initialise all vectors of spectral indices  
        /// </summary>
        /// <param name="size"></param>
        public static Dictionary<string, double[]> InitialiseSpectra(int size)
        {
            Dictionary<string, IndexProperties> dict = IndexProperties.GetDictionaryOfSpectralIndexProperties();
            string[] keys = dict.Keys.ToArray();

            //string[] keys = IndexProperties.ALL_KNOWN_SPECTRAL_KEYS.Split('-');

            Dictionary<string, double[]> spectra = new Dictionary<string, double[]>();
            foreach (string key in keys)
            {
                var spectrum = new double[size];
                if (key == IndexProperties.spKEY_BkGround)
                {
                    for (int i = 0; i < size; i++) spectrum[i] = -150; // set rock bottom BGN level in decibels
                } else
                    if (key == IndexProperties.spKEY_TemporalEntropy)
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
