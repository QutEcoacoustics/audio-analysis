using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    public class Result_1PS : BaseResult
    {
		#region Properties
        public double? MaxDisplayScore { get; set; }	        // upper limit for diplay of scores 

		public int? CallPeriodicity_frames { get; set; }
		public int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }

        public static string[] KeyNames = { "RANK_SCORE", "VOCAL_COUNT", "TIMEOF_TOP_SCORE" };

		#endregion



        public Result_1PS(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR
            AcousticMatrix = Template.AcousticModelConfig.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModelConfig.SyllSymbols;    //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModelConfig.SyllableIDs;    //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;

        }


        public override ResultItem GetResultItem(string key)
        {
            if (key.Equals(KeyNames[0]))      return new ResultItem(KeyNames[0], RankingScore,   GetResultInfo(KeyNames[0]));
            else if (key.Equals(KeyNames[1])) return new ResultItem(KeyNames[1], VocalCount,     GetResultInfo(KeyNames[1]));
            else if (key.Equals(KeyNames[2])) return new ResultItem(KeyNames[2], TimeOfTopScore, GetResultInfo(KeyNames[2]));
            return null;
        }

        public new static Dictionary<string, string> GetResultInfo(string key)
        {
            if (key.Equals("RANK_SCORE"))            return GetRankingScoreInfo();
            else if (key.Equals("VOCAL_COUNT"))      return GetVocalCountInfo();
            else if (key.Equals("TIMEOF_TOP_SCORE")) return GetTimeOfTopScoreInfo();
            return null;
        }

        private static Dictionary<string, string> GetRankingScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table["UNITS"] = "integer";
            table["COMMENT"] = "The count of the number of recognised syllables that have the correct time interval from " +
                               "the previous recognised syllable. ";
            table["PERIODICITY"] = "The required periodicity in ms for syllables of this call type.";
            return table;
        }
        private static Dictionary<string, string> GetVocalCountInfo()
        {
            var table = new Dictionary<string, string>();
            table["COMMENT"] = "Total number of vocalisations of the recognised syllable.";
            return table;
        }
        private static Dictionary<string, string> GetTimeOfTopScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table["UNITS"] = "seconds";
            table["COMMENT"] = "Time from beginning of recording.";
            return table;
        }


        public string GetOneLineSummary()
        {
            return string.Format("{0},{1},{2:F1},{3:F1}", recordingName, NumberOfPeriodicHits, RankingScore, TimeOfTopScore);
        }



    }
}
