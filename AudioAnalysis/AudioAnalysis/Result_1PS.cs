using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    public class Result_1PS : BaseResult
    {
		#region Properties
		public int? CallPeriodicity_frames { get; set; }
		public static int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }

        private string[] resultItemKeys = { "PERIODIC_HITS", "VOCAL_COUNT", BaseResult.TIME_OF_TOP_SCORE };

        public override string[] ResultItemKeys
        {
            get
            {
                return resultItemKeys;
            }
        }

        public override string RankingScoreName
        {
            get
            {
                return "PERIODIC_HITS";
            }

        }

		#endregion



        public Result_1PS(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR
            AcousticMatrix = Template.AcousticModel.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModel.SyllSymbols;    //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModel.SyllableIDs;    //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;

        }


        public override ResultItem GetResultItem(string key)
        {
            if (key.Equals(resultItemKeys[0])) return new ResultItem(resultItemKeys[0], RankingScoreValue, GetResultInfo(resultItemKeys[0]));
            else if (key.Equals(resultItemKeys[1])) return new ResultItem(resultItemKeys[1], VocalCount, GetResultInfo(resultItemKeys[1]));
            else if (key.Equals(resultItemKeys[2])) return new ResultItem(resultItemKeys[2], TimeOfMaxScore, GetResultInfo(resultItemKeys[2]));
            return null;
        }

        public new static Dictionary<string, string> GetResultInfo(string key)
        {
            if (key.Equals("PERIODIC_HITS")) return GetRankingScoreInfo();
            else if (key.Equals("VOCAL_COUNT"))      return GetVocalCountInfo();
            else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE)) return GetTimeOfTopScoreInfo();
            return null;
        }

        private static Dictionary<string, string> GetRankingScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table["UNITS"] = "integer";
            table["COMMENT"] = "The count of the number of recognised syllables that have the correct time interval from " +
                               "the previous recognised syllable. ";
            table["PERIODICITY"] = "The required periodicity in ms for syllables of this call type = " + CallPeriodicity_ms + ".";
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
            return string.Format("{0},{1},{2:F1},{3:F1}", recordingName, NumberOfPeriodicHits, RankingScoreValue, TimeOfMaxScore);
        }



    }
}
