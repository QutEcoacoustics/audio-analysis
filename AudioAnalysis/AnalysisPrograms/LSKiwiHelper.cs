using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms
{
    public static class LSKiwiHelper
    {

        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_MIN_HZ_MALE   = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE   = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE = "MAX_HZ_FEMALE";

        //HEADER KEYS
        public static string key_EVENT_NAME = AudioAnalysisTools.Keys.EVENT_NAME;
        public static string key_INTENSITY_SCORE = AudioAnalysisTools.Keys.EVENT_INTENSITY;
        public static string key_EVENT_NORMSCORE = AudioAnalysisTools.Keys.EVENT_NORMSCORE;
        public static string key_SNR_SCORE = AudioAnalysisTools.Keys.SNR_SCORE;

        public static string key_BANDWIDTH_SCORE = "BandwidthScore";
        public static string key_NEW_COMBO_SCORE = "NewComboScore";
        public static string key_CHIRP_SCORE     = "ChirpScore";
        public static string key_DELTA_SCORE     = "DeltaPeriodScore";
        public static string key_GRID_SCORE      = "GridScore";
        public static string key_PEAKS_SNR_SCORE = "PeaksSnrScore";
        public static string key_PEAKS_STD_SCORE = "PeaksStdScore";


        public static string[] DefaultRulesLSKiwi2 = {
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_BANDWIDTH_SCORE+"_LT_0.3",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_INTENSITY_SCORE+"_LT_0.12",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_PEAKS_SNR_SCORE+"_LT_0.12",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_DELTA_SCORE+"_LT_0.25",
                                           "WEIGHT_"+LSKiwiHelper.key_BANDWIDTH_SCORE+"=0.30",
                                           "WEIGHT_"+LSKiwiHelper.key_INTENSITY_SCORE+"=0.40",
                                           "WEIGHT_"+LSKiwiHelper.key_PEAKS_SNR_SCORE+"=0.10",
                                           "WEIGHT_"+LSKiwiHelper.key_PEAKS_STD_SCORE+"=0.10",
                                           "WEIGHT_"+LSKiwiHelper.key_DELTA_SCORE+    "=0.10"
            };

        public static string[] DefaultRulesLSKiwi3 = { //ORIGINAL WTS: (intensity1[r] * 0.3) + (gridScore[r] * 0.2) + (deltaPeriodScore[r] * 0.2) + (chirpScores[r] * 0.3);
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_BANDWIDTH_SCORE+"_LT_0.2",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_INTENSITY_SCORE+"_LT_0.12",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_PEAKS_SNR_SCORE+"_LT_0.12",
                                           "EXCLUDE_IF_RULE="+LSKiwiHelper.key_DELTA_SCORE+"_LT_0.25",

                                           "WEIGHT_"+LSKiwiHelper.key_INTENSITY_SCORE+"=0.30",
                                           "WEIGHT_"+LSKiwiHelper.key_GRID_SCORE     +"=0.20",
                                           "WEIGHT_"+LSKiwiHelper.key_DELTA_SCORE    +"=0.20",
                                           "WEIGHT_"+LSKiwiHelper.key_CHIRP_SCORE    +"=0.30",
                                           //"WEIGHT_"+LSKiwiHelper.key_BANDWIDTH_SCORE+"=0.30"
            };

        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";
        public const string XLSViewer   = @"C:\Program Files (x86)\Microsoft Office\Office12\EXCEL.EXE";

        public static Dictionary<string, double> GetFeatureWeights()
        {
            //string[] rules = DefaultRulesLSKiwi2;
            string[] rules = DefaultRulesLSKiwi3;
            var weights = new Dictionary<string, double>();
            foreach (string rule in rules)
            {
                string[] parts = rule.Split('_');
                if (parts[0] == "WEIGHT")
                {
                    string[] words = parts[1].Split('=');
                    weights.Add(words[0], Double.Parse(words[1]));
                }
            }
            return weights;
        } //GetFeatureWeights()

        public static List<string[]> GetExcludeRules()
        {
            //string[] rules = DefaultRulesLSKiwi2;
            string[] rules = DefaultRulesLSKiwi3;
            var excludeRules = new List<string[]>();
            foreach (string rule in rules)
            {
                string[] parts = rule.Split('=');
                if (parts[0] == "EXCLUDE_IF_RULE") excludeRules.Add(parts[1].Split('_'));
            }
            return excludeRules;
        } //GetExcludeRules()




    } // class LSKiwiHelper
}
