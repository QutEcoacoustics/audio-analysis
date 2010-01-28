using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using MarkovModels;

namespace AudioAnalysisTools
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

        public override ResultItem GetEventProperty(string key, AcousticEvent acousticEvent)
        {
            if (key.Equals("LLR_VALUE"))
            {
                double? score = GetMaxScoreInEvent(acousticEvent);
                var table = new Dictionary<string, string>();
                table.Add("UNITS", "LLR");
                table.Add("COMMENT_ABOUT_EVENT_LLR", "The returned score is the maximum LLR in the event. " +
                                   "An LLR score is given to each frame in the event.");
                table.Add("COMMENT_ABOUT_LLR", "The log likelihood ratio, in this case, is interpreted as follows. " +
                                   "Let v1 be the test vocalisation and let v2 be an 'average' vocalisation used to train the Markov Model. " +
                                   "Let p1 = p(v1 | MM) and let p2 = p(v2 | MM). Then LLR = log (p1 /p2).  " +
                                   "Note 1: In theory LLR takes only negative values because p1 < p2. " +
                                   "Note 2: For same reason, LLR's max value = 0.0 (i.e. test str has same prob has training sample. " +
                                   "Note 3: In practice, LLR can have positive value when p1 > p2 because p2 is an average.");

                table.Add("THRESHOLD", "-6.64");
                table.Add("COMMENT_ABOUT_THRESHOLD", "The null hypothesis is that the test sequence is a true positive. " +
                          "Accept null hypothesis if LLR >= -6.64.  " +
                          "Reject null hypothesis if LLR <  -6.64.");
                return new ResultItem("LLR_VALUE", score, table);
            }
            return null;

        }
        double? GetMaxScoreInEvent(AcousticEvent acousticEvent)
        {
            int start = acousticEvent.oblong.r1;
            int end = acousticEvent.oblong.r2;
            double max = -Double.MaxValue;
            for (int i = start; i <= end; i++)
            {
                if (this.Scores[i] > max) max = this.Scores[i];
            }
            return max;
        }




        public override ResultItem GetResultItem(string key)
        {
            if (key.Equals(resultItemKeys[0])) return new ResultItem(resultItemKeys[0], RankingScoreValue, GetResultInfo(resultItemKeys[0]));
            else if (key.Equals(resultItemKeys[1])) return new ResultItem(resultItemKeys[1], VocalCount, GetResultInfo(resultItemKeys[1]));
            else if (key.Equals(resultItemKeys[2])) return new ResultItem(resultItemKeys[2], TimeOfMaxScore, GetResultInfo(resultItemKeys[2]));
            return null;
        }


        //returns a list of acoustic events derived from the list of vocalisations detected by the recogniser.
        public override List<AcousticEvent> GetAcousticEvents(int samplingRate, int windowSize, int windowOffset, 
                                                              bool doMelScale, int minFreq, int maxFreq)
        {
            double frameDuration, frameOffset, framesPerSecond;
            AcousticEvent.CalculateTimeScale(samplingRate, windowSize, windowOffset,
                                         out frameDuration, out frameOffset, out framesPerSecond);

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
