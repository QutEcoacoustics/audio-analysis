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
        public int? VocalValid { get; set; }            // number of hits/vocalisations whose duration is valid for call
        public double? LLRThreshold { get; set; }       // significance threshold for display of LLR scores

        private string[] resultItemKeys = new string[] { "LLR_VALUE", "VOCAL_COUNT", "VOCAL_VALID", BaseResult.TIME_OF_TOP_SCORE };
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
                return "LLR_VALUE";
            }
        }

		#endregion



        public Result_MMErgodic(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR and put info in Results object 
            AcousticMatrix = Template.AcousticModel.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModel.SyllSymbols;       //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModel.SyllableIDs;       //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;
        }

        public override ResultItem GetResultItem(string key)
        {
            if (key.Equals("LLR_VALUE")) return new ResultItem("LLR_VALUE", RankingScoreValue, GetTopScoreInfo());
            else if (key.Equals("VOCAL_COUNT"))     return new ResultItem("VOCAL_COUNT", VocalCount, GetResultInfo("VOCAL_COUNT"));
            else if (key.Equals("VOCAL_VALID"))     return new ResultItem("VOCAL_VALID", VocalValid, GetResultInfo("VOCAL_VALID"));
            else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE)) return new ResultItem(BaseResult.TIME_OF_TOP_SCORE, TimeOfMaxScore, GetResultInfo(BaseResult.TIME_OF_TOP_SCORE));
            return null;
        }

        public new static Dictionary<string, string> GetResultInfo(string key)
        {
            if (key.Equals("LLR_VALUE")) return GetTopScoreInfo();
            else if (key.Equals("VOCAL_COUNT"))    return GetVocalCountInfo();
            else if (key.Equals("VOCAL_VALID"))    return GetVocalValidInfo();
            else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE)) return GetTimeOfTopScoreInfo();
            return null;
        }

        private static Dictionary<string, string> GetTopScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table.Add("UNITS", "LLR");
            table.Add("COMMENT_ABOUT_LLR", "The log likelihood ratio, in this case, is interpreted as follows. " +
                               "Let v1 be the test vocalisation and let v2 be an 'average' vocalisation used to train the Markov Model. "+
                               "Let p1 = p(v1 | MM) and let p2 = p(v2 | MM). Then LLR = log (p1 /p2).  "+
                               "Note 1: In theory LLR takes only negative values because p1 < p2. "+
                               "Note 2: For same reason, LLR's max value = 0.0 (i.e. test str has same prob has training sample. " +
                               "Note 3: In practice, LLR can have positive value when p1 > p2 because p2 is an average.");

            table.Add("THRESHOLD", "-6.64");
            table.Add("COMMENT_ABOUT_THRESHOLD", "The null hypothesis is that the test sequence is a true positive. "+
                      "Accept null hypothesis if LLR >= -6.64.  "+
                      "Reject null hypothesis if LLR <  -6.64.");
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

        public override string WriteResults()
        {
            StringBuilder sb = new StringBuilder("RESULTS OF SCANNING RECORDING FOR CALL <" + this.Template.CallName + ">\n");
            for (int i = 0; i < this.resultItemKeys.Length; i++)
            {
                ResultItem item = GetResultItem(this.resultItemKeys[i]);
                sb.AppendLine(this.resultItemKeys[i] + " = " + item.ToString());
                var info = GetResultInfo(this.resultItemKeys[i]);
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