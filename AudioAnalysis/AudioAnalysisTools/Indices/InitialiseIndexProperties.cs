using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.DSP;


namespace AudioAnalysisTools.Indices
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
    /// To create a new index you initilliase it in the above method and add it to the dictionary of indices. 
    /// </summary>
    public static class InitialiseIndexProperties
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
        public const string keyHIGH_SIGNAL_AMPLITUDE = "hiSIG-AMPL";
        public const string keyCLIPPING = "CLIPPING";
        public const string keySIG_AMPL = "SIGNAL-AMPL";
        public const string keyBKGROUND = "BKGROUND";
        public const string keySNR = "SNR";
        public const string keySNR_ACTIVE = "SNR-ACTIVE";
        public const string keyACTIVITY = "ACTIVITY";
        public const string keyEVENT_RATE = "EVENTS-RATE";
        public const string keyEVENT_DUR = "avEVENT-DUR";
        public const string keyHF_CVR = "HF-CVR";
        public const string keyMF_CVR = "MF-CVR";
        public const string keyLF_CVR = "LF-CVR";
        public const string keyHtemp = "H-TEMP";
        public const string keyHpeak = "H-PEAK";
        public const string keyHspec = "H-SPG";
        public const string keyHvari = "H-VAR";
        public const string keyACI = "AcousticComplexity";
        public const string keyCLUSTER_COUNT = "CLUSTER-COUNT";
        public const string keyCLUSTER_DUR = "avCLUST-DUR";
        public const string key3GRAM_COUNT = "3GRAM-COUNT";
        public const string keySPT_PER_SEC = "SPT-RATE";
        public const string keySPT_DUR = "avSPT-DUR";
        public const string keyRAIN = "RAIN";
        public const string keyCICADA = "CICADA";

        //KEYS FOR SPECTRAL INDICES
        public const string spKEY_ACI = "ACI";
        public const string spKEY_Average = "AVG";  // average dB value in each frequency bin after noise removal
        public const string spKEY_BkGround = "BGN"; // modal dB value in each frequency bin calculated during noise removal
        public const string spKEY_Cluster = "CLS";
        public const string spKEY_BinCover = "CVR";
        public const string spKEY_BinEvents = "EVN";
        public const string spKEY_SpPeakTracks = "SPT";
        public const string spKEY_TemporalEntropy = "ENT";




        /// <summary>
        /// Creates and returns all info about the currently calculated indices.
        /// </summary>
        /// <returns>A DICTIONARY OF INDICES</returns>
        public static Dictionary<string, IndexProperties> InitialisePropertiesOfIndices()
        {
            var properties = new Dictionary<string, IndexProperties>();
            // use one of following lines as template to create a new index.
            properties.Add(keyCOUNT,
                new IndexProperties
                {
                    Key = keyCOUNT,
                    Name = AudioAnalysisTools.AnalysisKeys.INDICES_COUNT,
                    DataType = typeof(int),
                    DoDisplay = false,
                    Comment = "Order ID of minute segment in temporal order from start of recording."
                });

            properties.Add(keySTART_MIN,
                new IndexProperties
                {
                    Key = keySTART_MIN,
                    Name = AudioAnalysisTools.AnalysisKeys.START_MIN,
                    DoDisplay = false,
                    Comment = "Exact time span (total minutes) from start of recording to start of this segment."
                });

            properties.Add(keySEGMENT_DURATION,
                new IndexProperties
                {
                    Key = keySEGMENT_DURATION,
                    Name = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION,
                    DataType = typeof(TimeSpan),
                    DoDisplay = false,
                    Comment = "Exact time span (total minutes) of this audio segment - typically 1.0 minutes."
                });

            properties.Add(keyHIGH_SIGNAL_AMPLITUDE,
                new IndexProperties
                {
                    Key = keyHIGH_SIGNAL_AMPLITUDE,
                    Name = "High Signal Ampl",
                    NormMax = 10.0,
                    Units = "av/s",
                    Comment = "Av number of samples/sec where abs. amplitude is within 10*epislon of the max signal value."
                });

            properties.Add(keyCLIPPING,
                new IndexProperties
                {
                    Key = keyCLIPPING,
                    Name = "Clipping",
                    NormMax = 1.0,
                    Units = "avClips/s",
                    Comment = "Av number of clipped samples/sec i.e. where the abs. amplitude of two conscutive samples is within 4*epislon of the max signal value."
                });

            properties.Add(keySIG_AMPL,
                new IndexProperties
                {
                    Key = keySIG_AMPL,
                    Name = "av Signal Ampl",
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL,
                    NormMax = -5.0,
                    Units = "dB",
                    DefaultValue = DEFAULT_SIGNAL_MIN,
                    Comment = "Av amplitude of the signal envelope in dB."
                });

            properties.Add(keyBKGROUND,
                new IndexProperties
                {
                    Key = keyBKGROUND,
                    Name = "Background",
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL,
                    NormMax = -20.0,
                    Units = "dB",
                    DefaultValue = DEFAULT_SIGNAL_MIN,
                    Comment = "Av amplitude of the noise removed from the audio segment using the method of Lamel et al."
                });

            properties.Add(keySNR,
                new IndexProperties
                {
                    Key = keySNR,
                    Name = "SNR",
                    NormMin = 0.0,
                    NormMax = 50.0,
                    Units = "dB",
                    Comment = "Max amplitude of signal envelope after noise removal."
                });

            properties.Add(keySNR_ACTIVE,
                new IndexProperties
                {
                    Key = keySNR_ACTIVE,
                    Name = "avSNRActive",
                    NormMin = 0.0,
                    NormMax = 30.0,
                    Units = "dB",
                    Comment = "Av amplitude of active frames in signal envelope after noise removal. Active frames are those with amplitude > threshold 3 dB."
                });

            properties.Add(keyACTIVITY,
                new IndexProperties
                {
                    Key = keyACTIVITY,
                    Name = "%Activity",
                    DataType = typeof(int),
                    NormMax = 100.0,
                    Units = "%",
                    Comment = "% of active frames i.e. where SNR exceeds threshold = 3 dB."
                });

            properties.Add(keyEVENT_RATE,
                new IndexProperties
                {
                    Key = keyEVENT_RATE,
                    Name = "Events/s",
                    NormMax = 1.0,
                    Units = "",
                    Comment = "Av number of events persecond. An event is any consecutive sequence of active frames having duration > threshold = 100 ms."
                });

            properties.Add(keyEVENT_DUR,
                new IndexProperties
                {
                    Key = keyEVENT_DUR,
                    Name = "av Event Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 1000,
                    Units = "ms",
                    Comment = "Av duration in ms of the events in an audio segment."
                });

            properties.Add(keyHF_CVR,
                new IndexProperties
                {
                    Key = keyHF_CVR,
                    Name = "hf Cover",
                    NormMax = 30,
                    DataType = typeof(int),
                    Units = "%"
                });

            properties.Add(keyMF_CVR,
                new IndexProperties
                {
                    Key = keyMF_CVR,
                    Name = "mf Cover",
                    NormMax = 30,
                    DataType = typeof(int),
                    Units = "%"
                });

            properties.Add(keyLF_CVR,
                new IndexProperties
                {
                    Key = keyLF_CVR,
                    Name = "lf Cover",
                    NormMax = 30,
                    DataType = typeof(int),
                    Units = "%"
                });

            properties.Add(keyHtemp,
                new IndexProperties
                {
                    Key = keyHtemp,
                    Name = "1-H[t]",
                    Comment = "1-Ht is a meassure of concentration of acoustic energy instead of energy dispersal.",
                    NormMin = 0.0,
                    NormMax = 0.5,
                    DefaultValue = 0.0,
                    includeInComboIndex = true,
                    comboWeight = 0.3
                });

            properties.Add(keyHpeak,
                new IndexProperties
                {
                    Key = keyHpeak,
                    Name = "H[peak freq]",
                    NormMin = 0.4,
                    NormMax = 1.0,
                    DefaultValue = 1.0
                });

            properties.Add(keyHspec,
                new IndexProperties
                {
                    Key = keyHspec,
                    Name = "H[spectral]",
                    NormMin = 0.4,
                    NormMax = 1.0,
                    DefaultValue = 1.0
                });

            properties.Add(keyHvari,
                new IndexProperties
                {
                    Key = keyHvari,
                    Name = "H[spectral var]",
                    NormMin = 0.4,
                    NormMax = 1.0,
                    DefaultValue = 1.0
                });

            properties.Add(keyACI,
                new IndexProperties
                {
                    Key = keyACI,
                    Name = "ACI",
                    NormMin = 0.4,
                    NormMax = 0.7,
                    includeInComboIndex = true,
                    comboWeight = 0.2
                });

            properties.Add(keyCLUSTER_COUNT,
                new IndexProperties
                {
                    Key = keyCLUSTER_COUNT,
                    Name = "Cluster Count",
                    DataType = typeof(int),
                    NormMax = 20,
                    includeInComboIndex = true,
                    comboWeight = 0.3,
                    Comment = "Number of spectrum clusters in one minute audio segment as determined by a clustering algorithm."
               });

            properties.Add(keyCLUSTER_DUR,
                new IndexProperties
                {
                    Key = keyCLUSTER_DUR,
                    Name = "av Cluster Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 500,
                    Units = "ms",
                    Comment = "Average duration in ms of the spectrum cluster sequences."
                });

            properties.Add(key3GRAM_COUNT,
                new IndexProperties
                {
                    Key = key3GRAM_COUNT,
                    Name = "3gramCount",
                    DataType = typeof(int),
                    NormMax = 50,
                    Comment = "Number of different tri-gram cluster sequences."
                });

            properties.Add(keySPT_PER_SEC,
                new IndexProperties
                {
                    Key = keySPT_PER_SEC,
                    Name = "av Tracks/Sec",
                    NormMax = 10,
                    Comment = "Average number of spectral tracks per second."
                });

            properties.Add(keySPT_DUR,
                new IndexProperties
                {
                    Key = keySPT_DUR,
                    Name = "av Track Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 1.0,
                    Units = "s",
                    Comment = "Average duration of a spectral track."
                });

            properties.Add(keyRAIN,
                new IndexProperties
                {
                    Key = keyRAIN,
                    Name = "Rain Index",
                    NormMax = 1.0,
                    Units = "",
                    Comment = "Rain score calculated every 5 sec and averaged over the minute."
                });

            properties.Add(keyCICADA,
                new IndexProperties
                {
                    Key = keyCICADA,
                    Name = "Cicada Index",
                    NormMax = 1.0,
                    Units = "",
                    Comment = "Cicada score calculated every 10 sec and 6 values averaged over the minute."
                });

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
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20,
                    NormMax = -20.0, //-20 adds more contrast into bgn image
                    DefaultValue = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20,
                    Units = "dB",
                    Comment = "dB value of the bcakground 'noise' removed each frequency bin."
                });

            properties.Add(spKEY_Cluster,
                new IndexProperties
                {
                    Key = spKEY_Cluster,
                    Name = "CLS",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 30.0,
                    Units = "ms",
                    Comment = "The number of spectral clusters in which each frequency bin is included."
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
                    NormMax = 100.0,
                    Units = "%",
                    Comment = "The % of frames included within an acoustic event as defined above."
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

        /// <summary>
        /// This method converts a csv file header into an appropriate key for the given index.
        /// The headers in the csv fiels have changed over the years so there may be several headers for any one index.
        /// Enter any new header you come across into the file.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        //public static Dictionary<string, string> GetDictionaryOfName2Key()
        //{
        //    Dictionary<string, string> mapName2Key = new Dictionary<string, string>();

        //    Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

        //    foreach (string key in indexProperties.Keys)
        //    {
        //        IndexProperties ip = indexProperties[key];
        //        mapName2Key.Add(ip.Name, key);
        //    }

        //    //now add in historical names from previous incarnations of csv file headers
        //    mapName2Key.Add("Start-min", keySTART_MIN);
        //    mapName2Key.Add("bg-dB", keyBKGROUND);
        //    mapName2Key.Add("snr-dB", keySNR);
        //    mapName2Key.Add("activeSnr-dB", keySNR_ACTIVE);
        //    mapName2Key.Add("activity", keyACTIVITY);
        //    mapName2Key.Add("segCount", keyEVENT_RATE);
        //    mapName2Key.Add("ACI", keyACI);
        //    mapName2Key.Add("clusterCount", keyCLUSTER_COUNT);
        //    mapName2Key.Add("rain", keyRAIN);
        //    mapName2Key.Add("cicada", keyCICADA);

        //    return mapName2Key;
        //}


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

        public static double[] GetArrayOfComboWeights(Dictionary<string, IndexProperties> properties)
        {
            double[] weightArray = new double[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                weightArray[count] = ic.comboWeight;
                count++;
            }
            return weightArray;
        }


    }
}
