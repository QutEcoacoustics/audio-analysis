﻿using System;
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

    public static class InitialiseIndexProperties
    {
        public const double DEFAULT_SIGNAL_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //in decibels

        // KEYS for referring to indices. These should really be an enum                
        //KEYS FOR SUMMARY INDICES
        public const string keyCOUNT = "COUNT";
        public const string keySTART_MIN = "START-MIN";
        public const string keySEGMENT_DURATION = "SEGMENT-DUR";
        public const string keyCLIP1 = "hiSIG-AMPL";
        public const string keyCLIP2 = "CLIPPING";
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
        public const string spKEY_BkGround = "BGN"; // modal dB value in each frequency bin prior to noise removal
        public const string spKEY_Cluster = "CLS";
        public const string spKEY_BinCover = "CVR";
        public const string spKEY_BinEvents = "EVN";
        public const string spKEY_SpPeakTracks = "SPT";
        public const string spKEY_TemporalEntropy = "ENT";
        public const string spKEY_Variance = "VAR";
        //public const string spKEY_Combined = "CMB"; //discontinued - replaced by false colour spectrograms
        //public const string spKEY_Colour = "COL"; //discontinued - 


        // NORMALISING CONSTANTS FOR INDICES
        //public const double ACI_MIN = 0.4;
        //public const double ACI_MAX = 0.8;
        //public const double AVG_MIN = 0.0;
        //public const double AVG_MAX = 50.0;
        //public const double BGN_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //-20 adds more contrast into bgn image
        //public const double BGN_MAX = -20.0;
        //public const double CLS_MIN = 0.0;
        //public const double CLS_MAX = 30.0;
        //public const double CVR_MIN = 0.0;
        //public const double CVR_MAX = 0.3;
        //public const double EVN_MIN = 0.0;
        //public const double EVN_MAX = 0.8;
        //public const double TEN_MIN = 0.4;
        //public const double TEN_MAX = 0.95;
        //public const double SDV_MIN = 0.0; // for the variance bounds
        //public const double SDV_MAX = 100.0;
        //public const double VAR_MIN = SDV_MIN * SDV_MIN;
        //public const double VAR_MAX = SDV_MAX * SDV_MAX; // previously 30000.0




        // do not change headers unnecessarily - otherwise will lose compatibility with previous csv files
        // if change a header record the old header in method below:         public static string ConvertHeaderToKey(string header)
        //public static string header_count = AudioAnalysisTools.AnalysisKeys.INDICES_COUNT;
        //public const string header_startMin = AudioAnalysisTools.AnalysisKeys.START_MIN;
        //public const string header_SecondsDuration = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION;
        //public const string header_Clipping1 = "Clipping1";
        //public const string header_Clipping2 = "Clipping2";
        //public const string header_avAmpdB = "avAmp-dB";
        //public const string header_snr = "SNR";
        //public const string header_activeSnr = "ActiveSNR";
        //public const string header_bgdB = "Background";
        //public const string header_activity = "Activity";
        //public const string header_segPerSec = "Seg/Sec";
        //public const string header_avSegDur = "avSegDur";
        //public const string header_hfCover = "hfCover";
        //public const string header_mfCover = "mfCover";
        //public const string header_lfCover = "lfCover";
        //public const string header_HAmpl = "H[temporal]";
        //public const string header_HPeakFreq = "H[peakFreq]";
        //public const string header_HAvSpectrum = "H[spectral]";
        //public const string header_HVarSpectrum = "H[spectralVar]";
        //public const string header_AcComplexity = "AcComplexity";
        //public const string header_NumClusters = "ClusterCount";
        //public const string header_avClustDuration = "avClustDur";
        //public const string header_TrigramCount = "3gramCount";
        //public const string header_SPTracksPerSec = "Tracks/Sec";
        //public const string header_SPTracksDur = "avTrackDur";
        public const string header_rain = "Rain";
        public const string header_cicada = "Cicada";





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
                    DoDisplay = false
                });

            properties.Add(keySTART_MIN,
                new IndexProperties
                {
                    Key = keySTART_MIN,
                    Name = AudioAnalysisTools.AnalysisKeys.START_MIN,
                    DoDisplay = false
                });

            properties.Add(keySEGMENT_DURATION,
                new IndexProperties
                {
                    Key = keySEGMENT_DURATION,
                    Name = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION,
                    DataType = typeof(TimeSpan),
                    DoDisplay = false
                });

            properties.Add(keyCLIP1,
                new IndexProperties
                {
                    Key = keyCLIP1,
                    Name = "High Signal Ampl",
                    NormMax = 10.0,
                    Units = "av/s"
                });

            properties.Add(keyCLIP2,
                new IndexProperties
                {
                    Key = keyCLIP2,
                    Name = "Clipping",
                    NormMax = 1.0,
                    Units = "avClips/s"
                });

            properties.Add(keySIG_AMPL,
                new IndexProperties
                {
                    Key = keySIG_AMPL,
                    Name = "av Signal Ampl",
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL,
                    NormMax = -5.0,
                    Units = "dB",
                    DefaultValue = DEFAULT_SIGNAL_MIN
                });

            properties.Add(keyBKGROUND,
                new IndexProperties
                {
                    Key = keyBKGROUND,
                    Name = "Background",
                    NormMin = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL,
                    NormMax = -20.0,
                    Units = "dB",
                    DefaultValue = DEFAULT_SIGNAL_MIN
                });

            properties.Add(keySNR,
                new IndexProperties
                {
                    Key = keySNR,
                    Name = "SNR",
                    NormMin = 0.0,
                    NormMax = 50.0,
                    Units = "dB"
                });

            properties.Add(keySNR_ACTIVE,
                new IndexProperties
                {
                    Key = keySNR_ACTIVE,
                    Name = "avSNRActive",
                    NormMin = 0.0,
                    NormMax = 50.0,
                    Units = "dB"
                });

            properties.Add(keyACTIVITY,
                new IndexProperties
                {
                    Key = keyACTIVITY,
                    Name = "Activity",
                    NormMax = 0.8,
                    Units = String.Empty
                });

            properties.Add(keyEVENT_RATE,
                new IndexProperties
                {
                    Key = keyEVENT_RATE,
                    Name = "Events/s",
                    NormMax = 1.0,
                    Units = ""
                });

            properties.Add(keyEVENT_DUR,
                new IndexProperties
                {
                    Key = keyEVENT_DUR,
                    Name = "av Event Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 500,
                    Units = "ms"
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
                    Name = "H[temporal]",
                    NormMin = 0.4,
                    NormMax = 0.95,
                    DefaultValue = 1.0,
                    includeInComboIndex = true,
                    comboWeight = 0.3
                });

            properties.Add(keyHpeak,
                new IndexProperties
                {
                    Key = keyHpeak,
                    Name = "H[peak freq]",
                    NormMin = 0.4,
                    NormMax = 0.95,
                    DefaultValue = 1.0
                });

            properties.Add(keyHspec,
                new IndexProperties
                {
                    Key = keyHspec,
                    Name = "H[spectral]",
                    NormMin = 0.4,
                    NormMax = 0.95,
                    DefaultValue = 1.0
                });

            properties.Add(keyHvari,
                new IndexProperties
                {
                    Key = keyHvari,
                    Name = "H[spectral var]",
                    NormMin = 0.4,
                    NormMax = 0.95,
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
                    NormMax = 50,
                    includeInComboIndex = true,
                    comboWeight = 0.3
                });

            properties.Add(keyCLUSTER_DUR,
                new IndexProperties
                {
                    Key = keyCLUSTER_DUR,
                    Name = "av Cluster Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 200,
                    Units = "ms"
                });

            properties.Add(key3GRAM_COUNT,
                new IndexProperties
                {
                    Key = key3GRAM_COUNT,
                    Name = "3gramCount",
                    DataType = typeof(int),
                    NormMax = 50
                });

            properties.Add(keySPT_PER_SEC,
                new IndexProperties
                {
                    Key = keySPT_PER_SEC,
                    Name = "av Tracks/Sec",
                    NormMax = 10
                });

            properties.Add(keySPT_DUR,
                new IndexProperties
                {
                    Key = keySPT_DUR,
                    Name = "av Track Duration",
                    DataType = typeof(TimeSpan),
                    NormMax = 200,
                    Units = "ms"
                });


            // ADD THE SUMMARY INDICES ABOVE HERE
            //==================================================================================================================================================
            //==================================================================================================================================================
            // ADD THE SPECTRAL INDICES BELOW HERE

            //string key, string name, typeof(double[]), bool doDisplay, double normMin, double normMax, "dB", bool _includeInComboIndex, 

            properties.Add(spKEY_ACI,
                new IndexProperties
                {
                    Key = spKEY_ACI,
                    Name = "ACI",
                    DataType = typeof(double[]),
                    NormMin = 0.4,
                    NormMax = 0.8,
                    Units = ""
                });

            properties.Add(spKEY_Average,
                new IndexProperties
                {
                    Key = spKEY_Average,
                    Name = "AVG",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 50.0,
                    Units = "dB"
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
                    Units = "dB"
                });

            properties.Add(spKEY_Cluster,
                new IndexProperties
                {
                    Key = spKEY_Cluster,
                    Name = "CLS",
                    DataType = typeof(double[]),
                    NormMin = 0.0,
                    NormMax = 30.0,
                    Units = "ms"
                });

            properties.Add(spKEY_BinCover,
                new IndexProperties
                {
                    Key = spKEY_BinCover,
                    Name = "CVR",
                    DataType = typeof(double[]),
                    NormMax = 0.3,
                    Units = ""
                });

            properties.Add(spKEY_BinEvents,
                new IndexProperties
                {
                    Key = spKEY_BinEvents,
                    Name = "EVN",
                    DataType = typeof(double[]),
                    NormMax = 0.5,
                    Units = ""
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
                    NormMin = 0.4,
                    NormMax = 0.95,
                    DefaultValue = 1.0,
                    Units = ""
                });

            properties.Add(spKEY_Variance,
                new IndexProperties
                {
                    Key = spKEY_Variance,
                    Name = "VAR",
                    DataType = typeof(double[]),
                    NormMax = 100 * 100,  // square of the expected maximum standard deviation // for the variance bounds previously 30000.0
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
