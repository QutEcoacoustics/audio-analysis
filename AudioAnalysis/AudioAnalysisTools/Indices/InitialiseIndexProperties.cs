// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitialiseIndexProperties.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   This static class contains all the keys to identify available acoustic indices.
//   THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX - the value is stored in class IndexValues.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AudioAnalysisTools.DSP;

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

        public static Dictionary<string, IndexProperties> FilterIndexPropertiesForSpectralOnly
            (Dictionary<string, IndexProperties> indexProperties)
        {
            return indexProperties
                .Where((kvp, i) => kvp.Value.IsSpectralIndex)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties(Dictionary<string, IndexProperties> indexProperties)
        {
            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (!ip.IsSpectralIndex)
                {
                    // summary indices are never of type double[]
                    dict.Add(ip.Key, ip);
                }
            }
            return dict;
        }


        public static string[] GetArrayOfIndexTypes(Dictionary<string, IndexProperties> properties)
        {
            string[] typeArray = new string[properties.Count];
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
            dict.Add("IndexCount", "RankOrder");
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
