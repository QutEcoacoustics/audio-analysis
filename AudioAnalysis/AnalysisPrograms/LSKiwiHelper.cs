using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using AudioAnalysisTools;
using TowseyLib;

namespace AnalysisPrograms
{
    public static class LSKiwiHelper
    {

        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_MIN_HZ_MALE   = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE   = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE = "MAX_HZ_FEMALE";
        public static string key_FILTER_EVENTS = "DO_FILTER_EVENTS";

        //HEADER KEYS
        public static string key_EVENT_NAME = AudioAnalysisTools.Keys.EVENT_NAME;
        public static string key_INTENSITY_SCORE = AudioAnalysisTools.Keys.EVENT_INTENSITY;
        public static string key_EVENT_NORMSCORE = AudioAnalysisTools.Keys.EVENT_NORMSCORE;
        public static string key_SNR_SCORE = AudioAnalysisTools.Keys.SNR_SCORE;

        public static string key_CHIRP_SCORE     = "ChirpScore";
        public static string key_DELTA_SCORE     = "DeltaPeriodScore";
        public static string key_GRID_SCORE      = "GridScore";
        public static string key_PEAKS_SNR_SCORE = "PeaksSnrScore";
        public static string key_PEAKS_STD_SCORE = "PeaksStdScore";
        public static string key_BANDWIDTH_SCORE = "BandwidthScore";
        public static string key_COMBO_SCORE     = "ComboScore";


        //public static string[] DefaultRulesLSKiwi2 = {
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_BANDWIDTH_SCORE+"_LT_0.3",
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_INTENSITY_SCORE+"_LT_0.10",
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_PEAKS_SNR_SCORE+"_LT_0.40",
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_DELTA_SCORE+"_LT_0.25",
        //                                   "WEIGHT_"+LSKiwiHelper.key_BANDWIDTH_SCORE+"=0.30",
        //                                   "WEIGHT_"+LSKiwiHelper.key_INTENSITY_SCORE+"=0.40",
        //                                   "WEIGHT_"+LSKiwiHelper.key_PEAKS_SNR_SCORE+"=0.10",
        //                                   "WEIGHT_"+LSKiwiHelper.key_PEAKS_STD_SCORE+"=0.10",
        //                                   "WEIGHT_"+LSKiwiHelper.key_DELTA_SCORE+    "=0.10"
        //    };

        //public static string[] DefaultRulesLSKiwi3 = { 
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_BANDWIDTH_SCORE+"_LT_0.3",
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_INTENSITY_SCORE+"_LT_0.1",
        //                                   "EXCLUDE_IF_RULE="+LSKiwiHelper.key_DELTA_SCORE+"_LT_0.1",

        //                                   //ORIGINAL     WTS: (intensity1[r] * 0.3) + (gridScore[r] * 0.20) + (deltaPeriodScore[r] * 0.20) + (chirpScores[r] * 0.3);
        //                                   //CURRENT BEST WTS: (intensity1[r] * 0.3) + (gridScore[r] * 0.15) + (deltaPeriodScore[r] * 0.25) + (chirpScores[r] * 0.3);
        //                                   "WEIGHT_"+LSKiwiHelper.key_INTENSITY_SCORE+"=0.30",
        //                                   "WEIGHT_"+LSKiwiHelper.key_GRID_SCORE     +"=0.15",
        //                                   "WEIGHT_"+LSKiwiHelper.key_DELTA_SCORE    +"=0.20",
        //                                   "WEIGHT_"+LSKiwiHelper.key_CHIRP_SCORE    +"=0.35",
        //    };

        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";
        //public const string XLSViewer   = @"C:\Program Files (x86)\Microsoft Office\Office12\EXCEL.EXE";
        public const string XLSViewer   = @"C:\Program Files\Microsoft Office\Office14\EXCEL.EXE";


        public static Dictionary<string, double> GetFeatureWeights(string configPath)
        {
            
            List<string> lines = FileTools.ReadTextFile(configPath);
            var weights = new Dictionary<string, double>();
            foreach (string rule in lines)
            {
                string[] parts = rule.Split('_');
                if (parts.Length != 2) continue;
                if (parts[0] == "WEIGHT")
                {
                    string[] words = parts[1].Split('=');
                    weights.Add(words[0], Double.Parse(words[1]));
                }
            }
            return weights;
        } //GetFeatureWeights()

        public static List<string[]> GetExcludeRules(string configPath)
        {
            List<string> lines = FileTools.ReadTextFile(configPath);
            var excludeRules = new List<string[]>();
            foreach (string rule in lines)
            {
                string[] parts = rule.Split('=');
                if (parts.Length != 2) continue;
                if (parts[0] == "EXCLUDE_IF_RULE") excludeRules.Add(parts[1].Split('_'));
            }
            return excludeRules;
        } //GetExcludeRules()





        //filter events using the exclude rules - just changes the event's normalised score
        public static AcousticEvent FilterEvent(AcousticEvent ae, List<string[]> rules)
        {

            //loop through exclusion rules - DO NOT DELETE events - set score to zero so can check later what is happening.
            foreach (string[] rule in rules)
            {
                string feature = rule[0];
                string op = rule[1];
                double value = Double.Parse(rule[2]);
                if (feature == LSKiwiHelper.key_BANDWIDTH_SCORE)
                {
                    if ((op == "LT") && (ae.kiwi_bandWidthScore < value) || ((op == "GT") && (ae.kiwi_bandWidthScore > value)))
                    {
                        ae.kiwi_bandWidthScore = 0.0;
                        ae.ScoreNormalised = 0.0;
                        return ae;
                    }
                }
                else // end if key_BANDWIDTH_SCORE
                    if ((feature == LSKiwiHelper.key_INTENSITY_SCORE))
                    {
                        if ((op == "LT") && (ae.kiwi_intensityScore < value) || ((op == "GT") && (ae.kiwi_intensityScore > value)))
                        {
                            ae.ScoreNormalised = 0.0;
                            return ae;
                        }
                    } // end if key_INTENSITY_SCORE
            }
            return ae;
        }


        public static double CalculateAverageEventScore(AcousticEvent ae, double[] scoreArray)
        {
            int start = ae.oblong.r1;
            int end = ae.oblong.r2;
            if (end > scoreArray.Length) end = scoreArray.Length - 1;
            int length = end - start + 1;
            double sum = 0.0;
            for (int i = start; i <= end; i++) sum += scoreArray[i];
            return sum / (double)length;
        }


       public static DataTable MergeAdjacentPredictions(DataTable dt)
       {
           //DataTable newTable = DataTableTools.CreateTable(dt);
           string sortString = Keys.EVENT_START_ABS + " ASC";
           dt = DataTableTools.SortTable(dt, sortString);
           int rowCount = dt.Rows.Count;
           for (int i = rowCount-2; i >=0; i--)
           {
               DataRow row1 = dt.Rows[i];
               DataRow row2 = dt.Rows[i+1];
               string name1 = (string)row1[Keys.EVENT_NAME];
               string name2 = (string)row2[Keys.EVENT_NAME];
               string predictedSex1;
               if (name1.EndsWith("(m)"))      predictedSex1 = "M";
               else if (name1.EndsWith("(f)")) predictedSex1 = "F";
               else predictedSex1 = null;
               string predictedSex2;
               if (name2.EndsWith("(m)"))      predictedSex2 = "M";
               else if (name2.EndsWith("(f)")) predictedSex2 = "F";
               else predictedSex2 = null;
               double start1 = (double)row1[Keys.EVENT_START_ABS];
               double start2 = (double)row2[Keys.EVENT_START_ABS];

               if (((start2 - start1) < 15.0) && (predictedSex1 == predictedSex2)) dt.Rows.Remove(row2);
           }

           return dt;
       }


    } // class LSKiwiHelper
}
