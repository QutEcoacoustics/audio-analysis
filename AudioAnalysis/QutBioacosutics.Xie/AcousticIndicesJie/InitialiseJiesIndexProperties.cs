using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.DSP;
using AudioAnalysisTools;


namespace AcousticIndicesJie
{
    ///
    /// TO CREATE AND IMPLEMENT A NEW ACOUSTIC SUMMARY INDEX, DO THE FOLLOWING:
    /// 1) Create a KEY or IDENTIFIER for the index in the list below.
    /// 2) Declare the new index and its properties in the method IndexConstants.InitialisePropertiesOfIndices();
    /// 3) Calculate the INDEX some where. In the case of Acoustic Indices, they are calculated in the class IndicesCalculate.cs.
    /// 4) Store the value of the index in the class IndexValues
    //==============

    /// <summary>
    /// This static class contains all the keys to identify available acoustic indices.
    /// The principle method, 
    ///         public static Dictionary<string, IndexProperties> InitialisePropertiesOfIndices()
    /// creates a dictionary of index properties.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
    /// To create a new index you initialise it in the above method and add it to the dictionary of indices. 
    /// </summary>
    public static class InitialiseJiesIndexProperties
    {
        public const double DEFAULT_SIGNAL_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //in decibels
        public static int bitsPerSample = 16;
        public static double epsilon = Math.Pow(0.5, bitsPerSample - 1);
        public static double CLIPPING_THRESHOLD = epsilon * 4; // estimate of fraction of clipped values in wave form
        public const double ZERO_SIGNAL_THRESHOLD = 0.001; // all values in zero signal are less than this value



        // KEYS for referring to indices. These should really be an enum                
        //KEYS FOR SUMMARY INDICES
        public const string keyCOUNT = "COUNT";
        public const string keySTART_MIN = "START-MIN";
        public const string keySEGMENT_DURATION = "SEGMENT-DUR";
        //public const string keyHIGH_SIGNAL_AMPLITUDE = "hiSIG-AMPL";
        //public const string keyCLIPPING = "CLIPPING";
        //public const string keySIG_AMPL = "SIGNAL-AMPL";
        //public const string keyBKGROUND = "BKGROUND";
        //public const string keySNR = "SNR";
        //public const string keySNR_ACTIVE = "SNR-ACTIVE";
        //public const string keyACTIVITY = "ACTIVITY";
        //public const string keyEVENT_RATE = "EVENTS-RATE";
        //public const string keyEVENT_DUR = "avEVENT-DUR";
        //public const string keyHF_CVR = "HF-CVR";
        //public const string keyMF_CVR = "MF-CVR";
        //public const string keyLF_CVR = "LF-CVR";
        //public const string keyHtemp = "H-TEMP";
        //public const string keyHpeak = "H-PEAK";
        //public const string keyHspec = "H-SPG";
        //public const string keyHvari = "H-VAR";
        //public const string keyACI = "AcousticComplexity";
        //public const string keyCLUSTER_COUNT = "CLUSTER-COUNT";
        //public const string keyCLUSTER_DUR = "avCLUST-DUR";
        //public const string key3GRAM_COUNT = "3GRAM-COUNT";
        //public const string keySPT_PER_SEC = "SPT-RATE";
        //public const string keySPT_DUR = "avSPT-DUR";
        //public const string keyRAIN = "RAIN";
        //public const string keyCICADA = "CICADA";

        //KEYS FOR SPECTRAL INDICES
        public const string spKEY_ACI = "ACI";
        public const string spKEY_Average = "AVG";  // average dB value in each frequency bin after noise removal
        public const string spKEY_BkGround = "BGN"; // modal dB value in each frequency bin calculated during noise removal
        public const string spKEY_Cluster = "CLS";
        public const string spKEY_BinCover = "CVR";
        public const string spKEY_BinEvents = "EVN";
        public const string spKEY_SpPeakTracks = "SPT";
        public const string spKEY_TemporalEntropy = "ENT";
        public const string spKEY_Harmonic = "HAR";

        public const string spKEY_Oscillation = "OSC";
        public const string spKEY_Track = "TRK";
        public const string spKEY_Energy = "ENG";
        /// <summary>
        /// Creates and returns all info about the currently calculated indices.
        /// </summary>
        /// <returns>A DICTIONARY OF INDICES</returns>
        public static Dictionary<string, IndexProperties> InitialisePropertiesOfIndices()
        {
            var properties = new Dictionary<string, IndexProperties>();
            // use one of following lines as template to create a new index.
            // ADD THE SUMMARY INDICES ABOVE HERE
            //==================================================================================================================================================
            //==================================================================================================================================================
            // ADD THE SPECTRAL INDICES BELOW HERE

            //IMPORTANT:  SPECTRAL INDCIES MUST BE OF TYPE Double[]

            //string key, string name, typeof(double[]), bool doDisplay, double normMin, double normMax, "dB", bool _includeInComboIndex, 

            properties.Add(spKEY_ACI,
                new IndexProperties
                {
                    Key = spKEY_ACI,
                    Name = "ACI",
                    DataType = typeof(double[]),
                    NormMin = 0.4,
                    NormMax = 0.7,
                    Units = "",
                    Comment = "Spectrum of ACI values, one value for each frequency bin."
                });

            properties.Add(spKEY_Average,
                new IndexProperties
                {
                    Key = spKEY_Average,
                    Name = "AVG",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 50.0,
                    Units = "dB",
                    Comment = "Average dB amplitude in each frequency bin after noise removal."
                });

            properties.Add(spKEY_BkGround,
                new IndexProperties
                {
                    Key = spKEY_BkGround,
                    Name = "BGN",
                    DataType = typeof(double[]),
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20, //-20 adds more contrast into BGN spectrogram
                    NormMax = -20.0, 
                    DefaultValue = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20,
                    Units = "dB",
                    Comment = "dB value of the bcakground 'noise' removed each frequency bin."
                });

            properties.Add(spKEY_BinCover,
                new IndexProperties
                {
                    Key = spKEY_BinCover,
                    Name = "CVR",
                    DataType = typeof(double[]),
                    NormMax = 30,
                    Units = "%",
                    Comment = "The percent of active elements in each frequency bin - i.e. where amplitude exceeds threshold = 3 dB."
                });

            properties.Add(spKEY_BinEvents,
                new IndexProperties
                {
                    Key = spKEY_BinEvents,
                    Name = "EVN",
                    DataType = typeof(double[]),
                    NormMax = 1.0,
                    Units = "events/s",
                    Comment = "Acoustic events per second (as defined above) within each frequency band."
                });

            properties.Add(spKEY_SpPeakTracks,
                new IndexProperties
                {
                    Key = spKEY_SpPeakTracks,
                    Name = "SPT",
                    DataType = typeof(double[]),
                    NormMax = 1.0,
                    Units = ""
                });

            properties.Add(spKEY_TemporalEntropy,
                new IndexProperties
                {
                    Key = spKEY_TemporalEntropy,
                    Name = "ENT",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 0.6,
                    DefaultValue = 0.0,
                    Comment = "Default value = 0.0 because index = 1-Ht. It is a meassure of concentration of acoustic energy instead of energy dispersal.",
                    Units = ""
                });

            properties.Add(spKEY_Harmonic,
                new IndexProperties
                {
                    Key = spKEY_Harmonic,
                    Name = "HAR",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 0.6,
                    DefaultValue = 0.0,
                    Comment = "",
                    Units = ""
                });

            //properties.Add(spKEY_Combined,
            //    new IndexProperties { Key = spKEY_Combined, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });

            return properties;
        }


        public static Dictionary<string, IndexProperties> GetDictionaryOfSpectralIndexProperties()
        {
            Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (ip.DataType == typeof(double[]))
                {
                    dict.Add(ip.Key, ip);
                }
            }
            return dict;
        }

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties()
        {
            Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (ip.DataType != typeof(double[]))
                {
                    dict.Add(ip.Key, ip);
                }
            }
            return dict;
        }


        public static Type[] GetArrayOfIndexTypes(Dictionary<string, IndexProperties> properties)
        {
            Type[] typeArray = new Type[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                typeArray[count] = ic.DataType;
                count++;
            }
            return typeArray;
        }

        public static string[] GetArrayOfIndexNames(Dictionary<string, IndexProperties> properties)
        {
            string[] nameArray = new string[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                nameArray[count] = ic.Name;
                count++;
            }
            return nameArray;
        }

        public static bool[] GetArrayOfDisplayBooleans(Dictionary<string, IndexProperties> properties)
        {
            bool[] doDisplayArray = new bool[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                doDisplayArray[count] = ic.DoDisplay;
                count++;
            }
            return doDisplayArray;
        }

        //public static double[] GetArrayOfComboWeights(Dictionary<string, IndexProperties> properties)
        //{
        //    double[] weightArray = new double[properties.Count];
        //    int count = 0;
        //    foreach (string key in properties.Keys)
        //    {
        //        IndexProperties ic = properties[key];
        //        weightArray[count] = ic.comboWeight;
        //        count++;
        //    }
        //    return weightArray;
        //}

    }
}
