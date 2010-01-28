using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TowseyLib;
using MarkovModels;

namespace AudioAnalysisTools
{
	[Serializable]
	public abstract class BaseResult
    {
        protected const string TIME_OF_TOP_SCORE = "TIME_OF_TOP_SCORE";

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

        public abstract ResultItem GetResultItem(string key);

        public abstract string[] ResultItemKeys
        {
            get;
        }

        public abstract List<AcousticEvent> GetAcousticEvents(int samplingRate, int windowSize, int windowOffset,
                                                              bool doMelScale, int minFreq, int maxFreq);

        public abstract ResultItem GetEventProperty(string key, AcousticEvent acousticEvent);

        public static Dictionary<string, string> GetResultInfo(string key)
        {
            return null;
        }

        public virtual string WriteResults()
        {
            return "NO RESULTS AVAILABLE FROM BASE CLASS.";
        }

    }
}