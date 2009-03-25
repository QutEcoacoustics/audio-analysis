using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
	public class Result_MMErgodic : BaseResult
	{
		#region Properties
        public double? MaxDisplayScore { get; set; }	// upper limit for diplay of scores 
        public int? VocalValid { get; set; }            // number of hits/vocalisations whose duration is valid for call
        public double? LLRThreshold { get; set; }       // significance threshold for display of LLR scores

        public static string[] KeyNames = {"RANK_SCORE","VOCAL_COUNT","VOCAL_VALID","TIMEOF_TOP_SCORE"};

		#endregion



        public Result_MMErgodic(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR and put info in Results object 
            AcousticMatrix = Template.AcousticModelConfig.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModelConfig.SyllSymbols;       //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModelConfig.SyllableIDs;       //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;
        }

        public override ResultItem GetResultItem(string key)
        {
            if (key.Equals("RANK_SCORE"))           return new ResultItem("RANK_SCORE", RankingScore, GetTopScoreInfo());
            else if (key.Equals("VOCAL_COUNT"))     return new ResultItem("VOCAL_COUNT", VocalCount, GetResultInfo("VOCAL_COUNT"));
            else if (key.Equals("VOCAL_VALID"))     return new ResultItem("VOCAL_VALID", VocalValid, GetResultInfo("VOCAL_VALID"));
            else if (key.Equals("TIMEOF_TOP_SCORE"))return new ResultItem("TIMEOF_TOP_SCORE", TimeOfTopScore, GetResultInfo("TIMEOF_TOP_SCORE"));
            return null;
        }

        public new static Dictionary<string, string> GetResultInfo(string key)
        {
            if (key.Equals("RANK_SCORE"))          return GetTopScoreInfo();
            else if (key.Equals("VOCAL_COUNT"))    return GetVocalCountInfo();
            else if (key.Equals("VOCAL_VALID"))    return GetVocalValidInfo();
            else if (key.Equals("TIMEOF_TOP_SCORE")) return GetTimeOfTopScoreInfo();
            return null;
        }

        private static Dictionary<string, string> GetTopScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table.Add("UNITS", "LLR");
            table.Add("COMMENT", "The log likelihood ratio, in this case, is to be interpreted as follows. " +
                               "Let str1 be a test vocalisation and let str2 be an 'average' vocalisation used to train the Markov Model. "+
                               "Let p1 = p(str1 | MM) and let p2 = p(str2 | MM). Then LLR = log (p1 /p2).  "+
                               "Note that the LLR takes negative values because p1 < p2. "+
                               "Note also that LRR's max value = 0.0 (i.e. test str has same prob has training sample.");

            table.Add("THRESHOLD", "3.0");
            return table;
        }
        private static Dictionary<string, string> GetVocalCountInfo()
        {
            var table = new Dictionary<string, string>();
            table.Add("COMMENT","The number of vocalisations that contained at least one recognised syllable.");
            return table;
        }
        private static Dictionary<string, string> GetVocalValidInfo()
        {
            var table = new Dictionary<string, string>();
            table.Add("COMMENT", "Number of accepted vocalisations that were within a valid time duration");
            return table;
        }
        private static Dictionary<string, string> GetTimeOfTopScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table.Add("UNITS", "seconds");
            table.Add("COMMENT", "Time from beginning of recording.");
            return table;
        }

        public string WriteResults()
        {
            StringBuilder sb = new StringBuilder("RESULTS OF SCANNING RECORDING FOR CALL <" + this.Template.CallName + ">\n");
            for (int i = 0; i < Result_MMErgodic.KeyNames.Length; i++)
            {
                ResultItem item = GetResultItem(KeyNames[i]);
                sb.AppendLine(KeyNames[i] + " = " + item.ToString());
                var info = GetResultInfo(KeyNames[i]);
                if (info == null) sb.AppendLine("\tNo information found for this result item.");
                else
                foreach (var pair in info)
                {
                    try
                    {
                        sb.AppendLine("\t" + pair.Key + " = " + pair.Value);
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine("\tNo information found for this key: " + pair.Key);
                        sb.AppendLine("\t" + e.ToString());
                    }
                }
            }
            return sb.ToString();
        }


		#region Comma Separated Summary Methods
		public static string GetSummaryHeader()
		{
			return "ID,Hits,MaxScr,MaxLoc";
		}

		#endregion
	}
}