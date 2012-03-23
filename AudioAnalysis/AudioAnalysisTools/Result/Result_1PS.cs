using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using MarkovModels;

namespace AudioAnalysisTools
{
    using Acoustics.Shared;

    public class Result_1PS : BaseResult
    {
		#region Properties
		public int? CallPeriodicity_frames { get; set; }
		public static int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }


        //public override string[] ResultItemKeys
        //{
        //    get
        //    {
        //        return resultItemKeys;
        //    }
        //}

        public override string RankingScoreName
        {
            get
            {
                return BaseResult.resultItemKeys[BaseResult.PERIODIC_HITS];
            }

        }

		#endregion



        public Result_1PS(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR
            AcousticMatrix = Template.AcousticModel.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModel.SyllSymbols;       //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModel.SyllableIDs;       //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;

        }

        public override ResultProperty GetEventProperty(string key, AcousticEvent acousticEvent)
        {
            if (key.Equals("LLR_VALUE"))
            {
                double? score = GetMaxScoreInEvent(acousticEvent);
                var rp = new ResultProperty("LLR_VALUE", score);
                BaseResult.AddLLRInfo(rp); 
                return rp;
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




        public override ResultProperty GetResultItem(string key)
        {
            if (key.Equals(resultItemKeys[0]))
            {
                var rp = new ResultProperty(resultItemKeys[0], RankingScoreValue);
                BaseResult.AddLLRInfo(rp);
                return rp;
            }
            else if (key.Equals(resultItemKeys[1]))
            {
                var rp = new ResultProperty(resultItemKeys[1], VocalCount);
                BaseResult.AddVocalCountInfo(rp); 
                return rp;
            }
            else if (key.Equals(resultItemKeys[2]))
            { 
                var rp = new ResultProperty(resultItemKeys[2], TimeOfMaxScore);
                BaseResult.AddTimeOfTopScoreInfo(rp); 
                return rp;
            }
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


        //public new static Dictionary<string, string> GetResultInfo(string key)
        //{
        //    if (key.Equals("PERIODIC_HITS")) return GetRankingScoreInfo();
        //    else if (key.Equals("VOCAL_COUNT"))      return GetVocalCountInfo();
        //    else if (key.Equals(BaseResult.TIME_OF_TOP_SCORE)) return GetTimeOfTopScoreInfo();
        //    return null;
        //}

        private static Dictionary<string, string> GetRankingScoreInfo()
        {
            var table = new Dictionary<string, string>();
            table["UNITS"] = "integer";
            table["COMMENT"] = "The count of the number of recognised syllables that have the correct time interval from " +
                               "the previous recognised syllable. ";
            table["PERIODICITY"] = "The required periodicity in ms for syllables of this call type = " + CallPeriodicity_ms + ".";
            return table;
        }

        public string GetOneLineSummary()
        {
            return string.Format("{0},{1},{2:F1},{3:F1}", recordingName, NumberOfPeriodicHits, RankingScoreValue, TimeOfMaxScore);
        }



    }
}
