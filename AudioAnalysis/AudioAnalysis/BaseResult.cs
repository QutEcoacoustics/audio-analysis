using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
	[Serializable]
	public abstract class BaseResult
    {

        #region Properties
        public string recordingName { get; set; }       //name of the recording file which was sacnned with template.
        public BaseTemplate Template { get; set; }
        public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
        public string SyllSymbols { get; set; }			// symbol sequence output from the acoustic model 
        public int[] SyllableIDs  { get; set; }			// above symbol sequence represented as array of integers - noise=0
        public double[] Scores { get; set; }		    // array of scores derived from arbitrary source

        //summary scores
        public int? VocalCount { get; set; }			// number of vocalisatsions involving a recognised syllable
        public double? RankingScore { get; set; }	    // the score used to rank/compare this recording with others
        public int? FrameWithTopScore { get; set; }     // id of frame having top score in recording 
        public double? TimeOfTopScore { get; set; }	    // time of top score from beginning of recording in seconds 

        #endregion

        public abstract ResultItem GetResultItem(string key);

        public static Dictionary<string, string> GetResultInfo(string key)
        {
            return null;
        }



    }
}