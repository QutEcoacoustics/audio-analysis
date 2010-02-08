using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors;
using TowseyLib;
using MarkovModels;
using System.IO;
using QutSensors.Data.Logic;

namespace AudioAnalysisTools
{
	public class Result_MMErgodic : BaseResult
	{
		#region Properties
        public double? LLRThreshold { get; set; } // significance threshold for display of LLR scores

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





        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="template"></param>
        public Result_MMErgodic(BaseTemplate template)
        {
            Template = template;
            AcousticMatrix = Template.AcousticModel.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModel.SyllSymbols;       //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModel.SyllableIDs;       //int[] integerSequence = result.SyllableIDs;
        }


        //returns a list of acoustic events derived from the list of vocalisations detected by the recogniser.
        public override List<AcousticEvent> GetAcousticEvents(int samplingRate, int windowSize, int windowOffset,
                                                              bool doMelScale, int minFreq, int maxFreq)
        {
            double frameDuration, frameOffset, framesPerSecond;
            AcousticEvent.CalculateTimeScale(samplingRate, windowSize, windowOffset,
                                             out frameDuration, out frameOffset, out framesPerSecond);

            this.FullVocalisations = GetFullVocalisations();

            var list = new List<AcousticEvent>();

            foreach (Vocalisation vocalEvent in this.FullVocalisations)
            {
                int startFrame = vocalEvent.Start;
                int endFrame   = vocalEvent.End;
                double startTime = startFrame * frameOffset;
                double duration = (endFrame - startFrame) * frameOffset; 
                var acouticEvent = new AcousticEvent(startTime, duration, minFreq, maxFreq);
                acouticEvent.SetTimeAndFreqScales(samplingRate, windowSize, windowOffset);
                list.Add(acouticEvent);
            }
            return list;
        } //end method GetAcousticEvents()


        private List<Vocalisation> GetFullVocalisations()
        {
            var list = new List<Vocalisation>();
            int start = 0;

            for (int i = 1; i < this.Scores.Length; i++)
            {
                if ((Scores[i - 1] < this.LLRThreshold) && (Scores[i] >= this.LLRThreshold))
                {
                    start = i;
                }
                //at end of vocalisatioin
                if ((Scores[i - 1] >= this.LLRThreshold) && (Scores[i] < this.LLRThreshold))
                {
                    int end = i;
                    int length = end - start + 1;
                    string sequence = this.SyllSymbols.Substring(start, length);
                    var vocalisation = new Vocalisation(start, end, sequence);
                    list.Add(vocalisation);
                }

            }//end for loop
            return list;
        }


        public override ResultProperty GetEventProperty(string key, AcousticEvent acousticEvent)
        {
            if (key.Equals("LLR_VALUE"))
            {
                double? score = GetMaxScoreInEvent(acousticEvent);
                var rp = new ResultProperty("LLR_VALUE", score, typeof(double));
                rp.AddInfo("UNITS", "LLR");
                rp.AddInfo("COMMENT_ABOUT_EVENT_LLR", "The returned score is the maximum LLR in the event. " +
                                   "An LLR score is given to each frame in the event." );
                rp.AddInfo("COMMENT_ABOUT_LLR", "The log likelihood ratio, in this case, is interpreted as follows. " +
                                   "Let v1 be the test vocalisation and let v2 be an 'average' vocalisation used to train the Markov Model. " +
                                   "Let p1 = p(v1 | MM) and let p2 = p(v2 | MM). Then LLR = log (p1 /p2).  " +
                                   "Note 1: In theory LLR takes only negative values because p1 < p2. " +
                                   "Note 2: For same reason, LLR's max value = 0.0 (i.e. test str has same prob has training sample. " +
                                   "Note 3: In practice, LLR can have positive value when p1 > p2 because p2 is an average.");

                rp.AddInfo("THRESHOLD", "-6.64");
                rp.AddInfo("COMMENT_ABOUT_THRESHOLD", "The null hypothesis is that the test sequence is a true positive. " +
                           "Accept null hypothesis if LLR >= -6.64.  " +
                           "Reject null hypothesis if LLR <  -6.64.");
                return rp;
            }
            return null;
        }


        public override ResultProperty GetResultItem(string key)
        {
            if (key.Equals("LLR_VALUE"))
            {
                var rp  = new ResultProperty("LLR_VALUE", RankingScoreValue, typeof(double));
                BaseResult.AddLLRInfo(rp);
                return rp;
            }
            else if (key.Equals("VOCAL_COUNT"))
            {
                var rp = new ResultProperty("VOCAL_COUNT", VocalCount, typeof(int));
                AddVocalCountInfo(rp);
                return rp;
            }
            else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE))
            {
                var rp = new ResultProperty(BaseResult.TIME_OF_TOP_SCORE, TimeOfMaxScore, typeof(double));
                AddTimeOfTopScoreInfo(rp);
                return rp;
            }
            return null;
        }

        //public new static Dictionary<string, string> GetResultItem(string key, ResultProperty rp)
        //{
        //    if (key.Equals("LLR_VALUE"))                       AddTopScoreInfo(rp);
        //    else if (key.Equals("VOCAL_COUNT"))                AddVocalCountInfo(rp);
        //    else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE)) AddTimeOfTopScoreInfo(rp);
        //    return null;
        //}

        public override string WriteResults()
        {
            StringBuilder sb = new StringBuilder("RESULTS OF SCANNING RECORDING FOR CALL <" + this.Template.CallName + ">\n");
            for (int i = 0; i < this.resultItemKeys.Length; i++)
            {
                ResultProperty item = GetResultItem(this.resultItemKeys[i]);
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

        double? GetMaxScoreInEvent(AcousticEvent acousticEvent)
        {
            int start = acousticEvent.oblong.r1;
            int end   = acousticEvent.oblong.r2;
            double max = -Double.MaxValue;
            for (int i = start; i <= end; i++)
            {
                if (this.Scores[i] > max) max = this.Scores[i];
            }
            return max - this.MaxDisplayScore;
        }


		#region Comma Separated Summary Methods
		public static string GetSummaryHeader()
		{
			return "ID,Hits,MaxScr,MaxLoc";
		}

		#endregion
	}
}