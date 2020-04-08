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
    using AudioAnalysisTools.DSP;

    /*
    /// TO CREATE AND IMPLEMENT A NEW ACOUSTIC INDEX (BOTH SUMMARY AND SPECTRAL INDICES), DO THE FOLLOWING:
    /// 1) Create a KEY or IDENTIFIER for the index in the list below. Always use this key when referencing the index.
    /// 2) Declare the properties of the new index in the YAML file: C:\Work\GitHub\audio-analysis\src\AnalysisConfigFiles\IndexPropertiesConfig.yml
    /// 3) Modify the method SpectralIndexValues.CreateImageOfSpectralIndices(SpectralIndexValues spectralIndices) to incorporate the new index
    /// 4) Calculate the INDEX some where. In the case of Acoustic Indices, they are calculated in the class IndicesCalculate.cs.
    /// 5) Store the value of the index in the class IndexValues
    /// 5a) e.g. for spectral index:   indexValues.AddSpectrum(InitialiseIndexProperties.KEYspectralENT, spectrumOfENTvalues);
    /// 5b) e.g. for summary index:    indexValues.StoreIndex(InitialiseIndexProperties.KEYindexName, indexValue);
    /// 6) Add lines into IndexCalculateTest.TestOfSpectralIndices() to set up testing for the new index
    /// ==============
    */

    /// <summary>
    /// This static class contains all the keys to identify available acoustic indices.
    /// THIS CLASS DOES NOT STORE THE VALUE OF THE INDEX
    ///    1) the value of spectral indices is stored in class SpectralIndexValues.
    ///    2) the value of summary  indices is stored in class SummaryIndexValues.
    /// </summary>
    public static class InitialiseIndexProperties
    {
        public const double DefaultSignalMin = SNR.MinimumDbBoundForZeroSignal - 20; //in decibels
        public const double ZeroSignalThreshold = 0.001; // all values in zero signal are less than this value

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
        public const string KeyHVarSpectrum = "HVarSpectrum";
        public const string KeyAcousticComplexity = "AcousticComplexity";
        public const string KeyClusterCount = "ClusterCount";
        public const string KeyClusterDur = "AvClusterDuration";
        public const string Key3GramCount = "3GramCount";
        public const string KeySptPerSec = "SPTPerSec";
        public const string KeySptDur = "AvSPTDuration";

        //KEYS FOR SPECTRAL INDICES
        // Initially thought that these would be useful but as of February 2019 none used, so commented.

        //public const string KeYspectralAci = "ACI";
        //public const string KeySpectralBgn = "BGN";
        //public const string KeYspectralCvr = "CVR";
        //public const string KeYspectralEnt = "ENT";
        //public const string KeYspectralEvn = "EVN";
        //public const string KeySpectralOsc = "OSC";
        //public const string KeySpectralPmn = "PMN";
        //public const string KeySpectralRhz = "RHZ";
        //public const string KeySpectralRng = "RNG";
        //public const string KeySpectralRps = "RPS";
        //public const string KeySpectralRvt = "RVT";
        //public const string KeySpectralSpt = "SPT";

        public static double ClippingThreshold
        {
            get
            {
                const int bitsPerSample = 16;
                var epsilon = Math.Pow(0.5, bitsPerSample - 1);
                return epsilon * 4;
            }
        }

        public static Dictionary<string, IndexProperties> FilterIndexPropertiesForSpectralOnly(
            Dictionary<string, IndexProperties> indexProperties) => indexProperties
                .Where(kvp => kvp.Value.IsSpectralIndex)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties(Dictionary<string, IndexProperties> indexProperties)
        {
            var dict = new Dictionary<string, IndexProperties>();
            foreach (var ip in indexProperties.Values)
            {
                if (!ip.IsSpectralIndex)
                {
                    dict.Add(ip.Key, ip);
                }
            }

            return dict;
        }
    }
}