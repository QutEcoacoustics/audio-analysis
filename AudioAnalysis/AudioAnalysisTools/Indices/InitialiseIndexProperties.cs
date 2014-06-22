using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools.DSP;


namespace AudioAnalysisTools.Indices
{
    ///
    /// TO CREATE AND IMPLEMENT A NEW ACOUSTIC INDEX (BOTH SUMMARY AND SPECTRAL INDICES), DO THE FOLLOWING:
    /// 1) Create a KEY or IDENTIFIER for the index in the list below. Always use this key when referencing the index.
    /// 2) Declare the properties of the new index in the YAML file: C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml
    /// 3) Calculate the INDEX some where. In the case of Acoustic Indices, they are calculated in the class IndicesCalculate.cs.
    /// 4) Store the value of the index in the class IndexValues
    /// 4a) e.g. for spectral index:   indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralENT, spectrumOfENTvalues);
    /// 4b) e.g. for summary index:    indexValues.StoreIndex(InitialiseIndexProperties.KEYindexName, indexValue); 
    /// ==============


    /// <summary>
    /// This static class contains all the keys to identify available acoustic indices.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
    /// </summary>
    public static class InitialiseIndexProperties
    {

        // KEYS FOR SUMMARY INDICES
        // WARNING!!! DO NOT change the below keys without ALSO changing in the IndexPropertiesConfig.yml file.
        public const string KEYRankOrder = "RankOrder";
        public const string KEYStartMinute = "StartMinute";
        public const string KEYSegmentDuration = "SegmentDuration";
        public const string KEYHighAmplitudeIndex = "HighAmplitudeIndex";
        public const string KEYClippingIndex = "ClippingIndex";
        public const string KEYAvSignalAmplitude = "AvSignalAmplitude";
        public const string KEYBackgroundNoise = "BackgroundNoise";
        public const string KEYSNR = "SNR";
        public const string KEYAvSNROfActiveFrames = "AvSNROfActiveFrames";
        public const string KEYActivity = "Activity";
        public const string KEYEventsPerSec = "EventsPerSec";
        public const string KEYAvEventDuration = "AvEventDuration";
        public const string KEYHF_CVR = "HF_CVR";
        public const string KEYMF_CVR = "MF_CVR";
        public const string KEYLF_CVR = "LF_CVR";
        public const string KEYHtemporal = "Htemporal";
        public const string KEYHpeak = "Hpeaks";
        public const string KEYHAvSpectrum = "HAvSpectrum";
        public const string KEYHVarSpectrum = "HVarSpectrum";
        public const string KEYAcousticComplexity = "AcousticComplexity";
        public const string keyCLUSTER_COUNT = "ClusterCount";
        public const string keyCLUSTER_DUR = "AvClusterDuration";
        public const string key3GRAM_COUNT = "3GramCount";
        public const string keySPT_PER_SEC = "SPTPerSec";
        public const string keySPT_DUR = "AvSPTDuration";
        public const string keyRAIN = "RainIndex";
        public const string keyCICADA = "CicadaIndex";

        //KEYS FOR SPECTRAL INDICES
        public const string KEYspectralACI = "ACI";
        public const string KEYspectralAVG = "AVG";  // average dB value in each frequency bin after noise removal
        public const string KEYspectralBGN = "BGN"; // modal dB value in each frequency bin calculated during noise removal
        public const string KEYspectralCLS = "CLS";
        public const string KEYspectralCVR = "CVR";
        public const string KEYspectralENT = "ENT";
        public const string KEYspectralEVN = "EVN";
        public const string KEYspectralSPT = "SPT";

       
        public const double DEFAULT_SIGNAL_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //in decibels
        public static int bitsPerSample = 16;
        public static double epsilon = Math.Pow(0.5, bitsPerSample - 1);
        public static double CLIPPING_THRESHOLD = epsilon * 4; // estimate of fraction of clipped values in wave form
        public const double ZERO_SIGNAL_THRESHOLD = 0.001; // all values in zero signal are less than this value





        /// <summary>
        /// ///// THIS METHOD IS TO BE DELETED. NOW REPLACED BY A YAML FILE
        /// 
        /// Creates and returns all info about the currently calculated indices.
        /// </summary>
        /// <returns>A DICTIONARY OF INDICES</returns>
        //public static Dictionary<string, IndexProperties> InitialisePropertiesOfIndices()
        //{
        //    var properties = new Dictionary<string, IndexProperties>();
        //    // use one of following lines as template to create a new index.
        //    properties.Add(KEYRankOrder,
        //        new IndexProperties
        //        {
        //            Key = KEYRankOrder,
        //            Name = AudioAnalysisTools.AnalysisKeys.KEY_RankOrder,
        //            DataType = typeof(int),
        //            DoDisplay = false,
        //            Comment = "Order ID of minute segment in temporal order from start of recording."
        //        });

        //    properties.Add(KEYStartMinute,
        //        new IndexProperties
        //        {
        //            Key = KEYStartMinute,
        //            Name = AudioAnalysisTools.AnalysisKeys.KEY_StartMinute,
        //            DoDisplay = false,
        //            Comment = "Exact time span (total minutes) from start of recording to start of this segment."
        //        });

        //    properties.Add(KEYSegmentDuration,
        //        new IndexProperties
        //        {
        //            Key = KEYSegmentDuration,
        //            Name = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION,
        //            DataType = typeof(TimeSpan),
        //            DoDisplay = false,
        //            Comment = "Exact time span (total minutes) of this audio segment - typically 1.0 minutes."
        //        });

        //    properties.Add(KEYHighAmplitudeIndex,
        //        new IndexProperties
        //        {
        //            Key = KEYHighAmplitudeIndex,
        //            Name = "High Signal Ampl",
        //            NormMax = 10.0,
        //            Units = "av/s",
        //            Comment = "Av number of samples/sec where abs. amplitude is within 10*epislon of the max signal value."
        //        });

        //    properties.Add(KEYClippingIndex,
        //        new IndexProperties
        //        {
        //            Key = KEYClippingIndex,
        //            Name = "Clipping",
        //            NormMax = 1.0,
        //            Units = "avClips/s",
        //            Comment = "Av number of clipped samples/sec i.e. where the abs. amplitude of two conscutive samples is within 4*epislon of the max signal value."
        //        });

        //    properties.Add(KEYAvSignalAmplitude,
        //        new IndexProperties
        //        {
        //            Key = KEYAvSignalAmplitude,
        //            Name = "av Signal Ampl",
        //            NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL + 10, //-70 dB is typical level for enironmental BGN
        //            NormMax = -10.0,
        //            Units = "dB",
        //            DefaultValue = DEFAULT_SIGNAL_MIN,
        //            Comment = "Av amplitude of the signal envelope in dB."
        //        });

        //    properties.Add(KEYBackgroundNoise,
        //        new IndexProperties
        //        {
        //            Key = KEYBackgroundNoise,
        //            Name = "Background",
        //            NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL + 10,
        //            NormMax = -10.0,
        //            Units = "dB",
        //            DefaultValue = DEFAULT_SIGNAL_MIN,
        //            Comment = "Av amplitude of the noise removed from the audio segment using the method of Lamel et al."
        //        });

        //    properties.Add(KEYSNR,
        //        new IndexProperties
        //        {
        //            Key = KEYSNR,
        //            Name = "SNR",
        //            NormMin = 0.0,
        //            NormMax = 50.0,
        //            Units = "dB",
        //            Comment = "Max amplitude of signal envelope after noise removal."
        //        });

        //    properties.Add(KEYAvSNROfActiveFrames,
        //        new IndexProperties
        //        {
        //            Key = KEYAvSNROfActiveFrames,
        //            Name = "avSNRActive",
        //            NormMin = 0.0,
        //            NormMax = 30.0,
        //            Units = "dB",
        //            Comment = "Av amplitude of active frames in signal envelope after noise removal. Active frames are those with amplitude > threshold 3 dB."
        //        });

        //    properties.Add(KEYActivity,
        //        new IndexProperties
        //        {
        //            Key = KEYActivity,
        //            Name = "%Activity",
        //            DataType = typeof(int),
        //            NormMax = 100.0,
        //            Units = "%",
        //            Comment = "% of active frames i.e. where SNR exceeds threshold = 3 dB."
        //        });

        //    properties.Add(KEYEventsPerSec,
        //        new IndexProperties
        //        {
        //            Key = KEYEventsPerSec,
        //            Name = "Events/s",
        //            NormMax = 1.0,
        //            Units = "",
        //            Comment = "Av number of events persecond. An event is any consecutive sequence of active frames having duration > threshold = 100 ms."
        //        });

        //    properties.Add(KEYAvEventDuration,
        //        new IndexProperties
        //        {
        //            Key = KEYAvEventDuration,
        //            Name = "av Event Duration",
        //            DataType = typeof(TimeSpan),
        //            NormMax = 1000,
        //            Units = "ms",
        //            Comment = "Av duration in ms of the events in an audio segment."
        //        });

        //    properties.Add(KEYHF_CVR,
        //        new IndexProperties
        //        {
        //            Key = KEYHF_CVR,
        //            Name = "hf Cover",
        //            NormMax = 50,
        //            DataType = typeof(int),
        //            Units = "%"
        //        });

        //    properties.Add(KEYMF_CVR,
        //        new IndexProperties
        //        {
        //            Key = KEYMF_CVR,
        //            Name = "mf Cover",
        //            NormMax = 50,
        //            DataType = typeof(int),
        //            Units = "%"
        //        });

        //    properties.Add(KEYLF_CVR,
        //        new IndexProperties
        //        {
        //            Key = KEYLF_CVR,
        //            Name = "lf Cover",
        //            NormMax = 50,
        //            DataType = typeof(int),
        //            Units = "%"
        //        });

        //    properties.Add(KEYHtemporal,
        //        new IndexProperties
        //        {
        //            Key = KEYHtemporal,
        //            Name = "1-H[t]",
        //            Comment = "1-Ht is a meassure of concentration of acoustic energy instead of energy dispersal.",
        //            NormMin = 0.0,
        //            NormMax = 0.5,
        //            DefaultValue = 0.0,
        //            includeInComboIndex = true,
        //            comboWeight = 0.3
        //        });

        //    properties.Add(KEYHpeak,
        //        new IndexProperties
        //        {
        //            Key = KEYHpeak,
        //            Name = "1-H[peak freq]",
        //            NormMin = 0.0,
        //            NormMax = 1.0,
        //            DefaultValue = 0.0
        //        });

        //    properties.Add(KEYHAvSpectrum,
        //        new IndexProperties
        //        {
        //            Key = KEYHAvSpectrum,
        //            Name = "1-H[spectral avg]",
        //            NormMin = 0.0,
        //            NormMax = 1.0,
        //            DefaultValue = 0.0
        //        });

        //    properties.Add(KEYHVarSpectrum,
        //        new IndexProperties
        //        {
        //            Key = KEYHVarSpectrum,
        //            Name = "1-H[spectral var]",
        //            NormMin = 0.0,
        //            NormMax = 1.0,
        //            DefaultValue = 0.0
        //        });

        //    properties.Add(KEYAcousticComplexity,
        //        new IndexProperties
        //        {
        //            Key = KEYAcousticComplexity,
        //            Name = "ACI",
        //            NormMin = 0.4,
        //            NormMax = 0.7,
        //            includeInComboIndex = true,
        //            comboWeight = 0.2
        //        });

        //    properties.Add(keyCLUSTER_COUNT,
        //        new IndexProperties
        //        {
        //            Key = keyCLUSTER_COUNT,
        //            Name = "Cluster Count",
        //            DataType = typeof(int),
        //            NormMax = 20,
        //            includeInComboIndex = true,
        //            comboWeight = 0.3,
        //            Comment = "Number of spectrum clusters in one minute audio segment as determined by a clustering algorithm."
        //       });

        //    properties.Add(keyCLUSTER_DUR,
        //        new IndexProperties
        //        {
        //            Key = keyCLUSTER_DUR,
        //            Name = "av Cluster Duration",
        //            DataType = typeof(TimeSpan),
        //            NormMax = 500,
        //            Units = "ms",
        //            Comment = "Average duration in ms of the spectrum cluster sequences."
        //        });

        //    properties.Add(key3GRAM_COUNT,
        //        new IndexProperties
        //        {
        //            Key = key3GRAM_COUNT,
        //            Name = "3gramCount",
        //            DataType = typeof(int),
        //            NormMax = 50,
        //            Comment = "Number of different tri-gram cluster sequences."
        //        });

        //    properties.Add(keySPT_PER_SEC,
        //        new IndexProperties
        //        {
        //            Key = keySPT_PER_SEC,
        //            Name = "av Tracks/Sec",
        //            NormMax = 10,
        //            Comment = "Average number of spectral tracks per second."
        //        });

        //    properties.Add(keySPT_DUR,
        //        new IndexProperties
        //        {
        //            Key = keySPT_DUR,
        //            Name = "av Track Duration",
        //            DataType = typeof(TimeSpan),
        //            NormMax = 1.0,
        //            Units = "s",
        //            Comment = "Average duration of a spectral track."
        //        });

        //    properties.Add(keyRAIN,
        //        new IndexProperties
        //        {
        //            Key = keyRAIN,
        //            Name = "Rain Index",
        //            NormMax = 1.0,
        //            Units = "",
        //            Comment = "Rain score calculated every 5 sec and averaged over the minute."
        //        });

        //    properties.Add(keyCICADA,
        //        new IndexProperties
        //        {
        //            Key = keyCICADA,
        //            Name = "Cicada Index",
        //            NormMax = 1.0,
        //            Units = "",
        //            Comment = "Cicada score calculated every 10 sec and 6 values averaged over the minute."
        //        });

        //    // ADD THE SUMMARY INDICES ABOVE HERE
        //    //==================================================================================================================================================
        //    //==================================================================================================================================================
        //    // ADD THE SPECTRAL INDICES BELOW HERE

        //    //IMPORTANT:  SPECTRAL INDCIES MUST BE OF TYPE Double[]

        //    //string key, string name, typeof(double[]), bool doDisplay, double normMin, double normMax, "dB", bool _includeInComboIndex, 

        //    properties.Add(KEYspectralACI,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralACI,
        //            Name = "ACI",
        //            DataType = typeof(double[]),
        //            NormMin = 0.4,
        //            NormMax = 0.7,
        //            Units = "",
        //            Comment = "Spectrum of ACI values, one value for each frequency bin."
        //        });

        //    properties.Add(KEYspectralAVG,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralAVG,
        //            Name = "AVG",
        //            DataType = typeof(double[]),
        //            NormMin = 0.0,
        //            NormMax = 70.0,
        //            Units = "dB",
        //            Comment = "Average dB amplitude in each frequency bin after noise removal."
        //        });

        //    properties.Add(KEYspectralBGN,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralBGN,
        //            Name = "BGN",
        //            DataType = typeof(double[]),
        //            NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20, //-20 adds more contrast into BGN spectrogram
        //            NormMax = -20.0, 
        //            DefaultValue = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20,
        //            Units = "dB",
        //            Comment = "dB value of the bcakground 'noise' removed each frequency bin."
        //        });

        //    properties.Add(KEYspectralCLS,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralCLS,
        //            Name = "CLS",
        //            DataType = typeof(double[]),
        //            NormMin = 0.0,
        //            NormMax = 10.0,
        //            Units = "ms",
        //            Comment = "The number of spectral clusters in which each frequency bin is included."
        //        });

        //    properties.Add(KEYspectralCVR,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralCVR,
        //            Name = "CVR",
        //            DataType = typeof(double[]),
        //            NormMax = 50,
        //            Units = "%",
        //            Comment = "The percent of active elements in each frequency bin - i.e. where amplitude exceeds threshold = 3 dB."
        //        });

        //    properties.Add(KEYspectralEVN,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralEVN,
        //            Name = "EVN",
        //            DataType = typeof(double[]),
        //            NormMax = 2.0,
        //            Units = "events/s",
        //            Comment = "Acoustic events per second (as defined above) within each frequency band."
        //        });

        //    properties.Add(KEYspectralSPT,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralSPT,
        //            Name = "SPT",
        //            DataType = typeof(double[]),
        //            NormMax = 0.5,
        //            Units = ""
        //        });

        //    properties.Add(KEYspectralENT,
        //        new IndexProperties
        //        {
        //            Key = KEYspectralENT,
        //            Name = "ENT",
        //            DataType = typeof(double[]),
        //            NormMin = 0.0,
        //            NormMax = 0.6,
        //            DefaultValue = 0.0,
        //            Comment = "Default value = 0.0 because index = 1-Ht. It is a meassure of concentration of acoustic energy instead of energy dispersal.",
        //            Units = ""
        //        });

        //    //properties.Add(spKEY_Combined,
        //    //    new IndexProperties { Key = spKEY_Combined, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });

        //    return properties;
        //}


        //public static Dictionary<string, IndexProperties> GetDictionaryOfSpectralIndexProperties()
        //{
        //    Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

        //    return GetDictionaryOfSpectralIndexProperties(indexProperties);
        //}

        public static Dictionary<string, IndexProperties> GetDictionaryOfSpectralIndexProperties(Dictionary<string, IndexProperties> indexProperties)
        {
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

        //public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties()
        //{
        //    Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

        //    var dict = new Dictionary<string, IndexProperties>();
        //    foreach (IndexProperties ip in indexProperties.Values)
        //    {
        //        if (ip.DataType != typeof(double[]))
        //        {
        //            dict.Add(ip.Key, ip);
        //        }
        //    }
        //    return dict;
        //}

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties(Dictionary<string, IndexProperties> indexProperties)
        {
            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (ip.DataType != typeof(double[])) //summary indices are never of type double[]
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

        public static double[] GetArrayOfComboWeights(Dictionary<string, IndexProperties> properties)
        {
            double[] weightArray = new double[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                weightArray[count] = ic.ComboWeight;
                count++;
            }
            return weightArray;
        }


        public static Dictionary<string, string> GetKeyTranslationDictionary()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("COUNT", "RankOrder");
            dict.Add("START-MIN", "StartMinute");
            dict.Add("SEGMENT-DUR", "SegmentDuration");
            dict.Add("hiSIG-AMPL", "HighAmplitudeIndex");
            dict.Add("CLIPPING", "ClippingIndex");
            dict.Add("SIGNAL-AMPL", "AvSignalAmplitude");
            dict.Add("BKGROUND", "BackgroundNoise");
            dict.Add("SNR", "SNR");
            dict.Add("SNR-ACTIVE", "AvSNRActive");
            dict.Add("AvSNROfActiveFrames", "AvSNRActive");
            dict.Add("ACTIVITY", "Activity");
            dict.Add("EVENTS-RATE", "EventsPerSec");
            dict.Add("avEVENT-DUR", "AvEventDuration");
            dict.Add("HF-CVR", "HF_CVR");
            dict.Add("MF-CVR", "MF_CVR");
            dict.Add("LF-CVR", "LF_CVR");
            dict.Add("H-TEMP", "Htemp");
            dict.Add("Htemporal", "Htemp");
            dict.Add("H-PEAK", "Hpeak");
            dict.Add("H-SPG", "HAvSpectrum");
            dict.Add("H-VAR", "HVarSpectrum");
            dict.Add("ACI", "AcousticComplexity");
            dict.Add("CLUSTER-COUNT", "ClusterCount");
            dict.Add("avCLUST-DUR", "AvClusterDuration");
            dict.Add("3GRAM-COUNT", "3GramCount");
            dict.Add("SPT-RATE", "SPTPerSec");
            dict.Add("avSPT-DUR", "AvSPTDuration");
            dict.Add("RAIN", "RainIndex");
            dict.Add("CICADA", "CicadaIndex");



            return dict;
        }

    }
}
