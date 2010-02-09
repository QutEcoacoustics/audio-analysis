using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TowseyLib;
using MarkovModels;
using QutSensors.Data.Logic;


namespace AudioAnalysisTools
{
	[Serializable]
	public abstract class BaseResult
    {
        protected const int LLR_VALUE         = 0;
        protected const int PERIODIC_HITS     = 1;
        protected const int VOCAL_COUNT       = 2;
        protected const int TIME_OF_TOP_SCORE = 3;
        protected const int VOCAL_VALID       = 4;
        protected const int SCORE             = 5;

        public static string[] resultItemKeys = { "LLR_VALUE", "PERIODIC_HITS", "VOCAL_COUNT", "TIME_OF_TOP_SCORE", "VOCAL_VALID", "SCORE" };



        #region Properties
        public string recordingName { get; set; }       //name of the recording file which was sacnned with template.
        public BaseTemplate Template { get; set; }
        public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
        public string SyllSymbols { get; set; }			// symbol sequence output from the acoustic model 
        public int[] SyllableIDs  { get; set; }			// above symbol sequence represented as array of integers - noise=0
        public double[] Scores { get; set; }		    // array of scores derived from arbitrary source

        //summary scores
        public List<Vocalisation> FullVocalisations { get; set; } // list of vocalisations obtained by recogniser
        public int? VocalCount { get; set; }			// number of vocalisations that include recognised syllable(s)
        public double? RankingScoreValue { get; set; }	// the score used to rank/compare this recording with others
        public double? MaxScore { get; set; }	        // the maximum score obtained with this recording
        public int? FrameWithMaxScore { get; set; }     // id of frame having top score in recording 
        public double? TimeOfMaxScore { get; set; }	    // time of top score from beginning of recording in seconds 

        public double? MaxDisplayScore { get; set; }	// upper limit for diplay of scores 
        public double? MinDisplayScore { get; set; }	// lower limit for diplay of scores 
        public double? DisplayThreshold { get; set; }	// threshold for diplay of scores

        public abstract string RankingScoreName { get; }
        #endregion

        public abstract ResultProperty GetResultItem(string key);

        public abstract List<AcousticEvent> GetAcousticEvents(int samplingRate, int windowSize, int windowOffset, bool doMelScale, int minFreq, int maxFreq);

        public abstract ResultProperty GetEventProperty(string key, AcousticEvent acousticEvent);

        public static Dictionary<string, string> GetResultInfo(string key)
        {
            return null;
        }

        public virtual string WriteResults()
        {
            return "NO RESULTS AVAILABLE FROM BASE CLASS.";
        }



        public static void AddLLRInfo(ResultProperty rp)
        {
            rp.AddInfo("UNITS", "LLR");
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
        }

        public static void AddTimeOfTopScoreInfo(ResultProperty rp)
        {
            rp.AddInfo("UNITS", "seconds");
            rp.AddInfo("COMMENT", "Time from beginning of recording.");
        }

        public static void AddVocalCountInfo(ResultProperty rp)
        {
            rp.AddInfo("COMMENT", "The number of vocalisations that contained at least one recognised syllable.");
        }


    }
}