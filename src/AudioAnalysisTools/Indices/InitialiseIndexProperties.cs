// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitialiseIndexProperties.cs" company="QutEcoacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
    using DSP;

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
        public const string KeyRankOrder = "RankOrder";
        public const string KeyStartMinute = "StartMinute";
        public const string KeySegmentDuration = "AnalysisIdealSegmentDuration";
        public const string KeyHighAmplitudeIndex = "HighAmplitudeIndex";
        public const string KeyClippingIndex = "ClippingIndex";
        public const string KeyAvSignalAmplitude = "AvSignalAmplitude";
        public const string KeyBackgroundNoise = "BackgroundNoise";
        public const string KeySnr = "SNR";
        public const string KeyAvgSnrOfActiveFrames = "AvSNROfActiveFrames";
        public const string KeyActivity = "Activity";
        public const string KeyEventsPerSec = "EventsPerSec";
        public const string KeyAvEventDuration = "AvEventDuration";
        public const string KeyHfCvr = "HF_CVR";
        public const string KeyMfCvr = "MF_CVR";
        public const string KeyLfCvr = "LF_CVR";
        public const string KeyHtemporal = "Htemporal";
        public const string KeyHpeak = "Hpeaks";
        public const string KeyHAvSpectrum = "HAvSpectrum";
        public const string KeyhVarSpectrum = "HVarSpectrum";
        public const string KeyAcousticComplexity = "AcousticComplexity";
        public const string KeyClusterCount = "ClusterCount";
        public const string KeyClusterDur = "AvClusterDuration";
        public const string Key3GramCount = "3GramCount";
        public const string KeySptPerSec = "SPTPerSec";
        public const string KeySptDur = "AvSPTDuration";
        public const string KeyRain = "RainIndex";
        public const string KeyCicada = "CicadaIndex";

        //KEYS FOR SPECTRAL INDICES
        public const string KeYspectralAci = "ACI";
        public const string KeYspectralAvg = "AVG";  // average dB value in each frequency bin after noise removal
        public const string KeYspectralBgn = "BGN"; // modal dB value in each frequency bin calculated during noise removal
        public const string KeYspectralCls = "CLS";
        public const string KeYspectralCvr = "CVR";
        public const string KeYspectralEnt = "ENT";
        public const string KeYspectralEvn = "EVN";
        public const string KeYspectralSpt = "SPT";
        public const double DefaultSignalMin = SNR.MinimumDbBoundForZeroSignal - 20; //in decibels

        public static double ClippingThreshold
        {
            get
            {
                int bitsPerSample = 16;
                double epsilon = Math.Pow(0.5, bitsPerSample - 1);
                return epsilon * 4;
            }
        }

        public const double ZeroSignalThreshold = 0.001; // all values in zero signal are less than this value

        public static Dictionary<string, IndexProperties> FilterIndexPropertiesForSpectralOnly(
            Dictionary<string, IndexProperties> indexProperties)
        {
            return indexProperties
                .Where(kvp => kvp.Value.IsSpectralIndex)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties(Dictionary<string, IndexProperties> indexProperties)
        {
            var dict = new Dictionary<string, IndexProperties>();
            foreach (var ip in indexProperties.Values)
            {
                if (!ip.IsSpectralIndex)
                {
                    // summary indices are never of type double[]
                    dict.Add(ip.Key, ip);
                }
            }

            return dict;
        }

        public static Dictionary<string, string> GetKeyTranslationDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                { "IndexCount", "RankOrder" },
                { "COUNT", "RankOrder" },
                { "START-MIN", "StartMinute" },
                { "SEGMENT-DUR", "AnalysisIdealSegmentDuration" },
                { "hiSIG-AMPL", "HighAmplitudeIndex" },
                { "CLIPPING", "ClippingIndex" },
                { "SIGNAL-AMPL", "AvSignalAmplitude" },
                { "BKGROUND", "BackgroundNoise" },
                { "SNR", "SNR" },
                { "SNR-ACTIVE", "AvSNRActive" },
                { "AvSNROfActiveFrames", "AvSNRActive" },
                { "ACTIVITY", "Activity" },
                { "EVENTS-RATE", "EventsPerSec" },
                { "avEVENT-DUR", "AvEventDuration" },
                { "HF-CVR", "HF_CVR" },
                { "MF-CVR", "MF_CVR" },
                { "LF-CVR", "LF_CVR" },
                { "H-TEMP", "Htemp" },
                { "Htemporal", "Htemp" },
                { "H-PEAK", "Hpeak" },
                { "H-SPG", "HAvSpectrum" },
                { "H-VAR", "HVarSpectrum" },
                { "ACI", "AcousticComplexity" },
                { "CLUSTER-COUNT", "ClusterCount" },
                { "avCLUST-DUR", "AvClusterDuration" },
                { "3GRAM-COUNT", "3GramCount" },
                { "SPT-RATE", "SPTPerSec" },
                { "avSPT-DUR", "AvSPTDuration" },
                { "RAIN", "RainIndex" },
                { "CICADA", "CicadaIndex" },
            };

            return dict;
        }
    }
}
