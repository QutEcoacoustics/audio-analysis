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
        public string ID { get; set; } //name of the recording file which was sacnned with template.
        public BaseTemplate Template { get; set; }
        public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
        public string SyllSymbols { get; set; }			// symbol sequence output from the acoustic model 
        public int[] SyllableIDs  { get; set; }			// above symbol sequence represented as array of integers - noise=0
        public double[] Scores { get; set; }		    // array of scores derived from arbitrary source
        public int? VocalCount { get; set; }			// number of hits i.e. vocalisatsions in the symbol seuqence
        public double? TopScore { get; set; }	        // the highest score in recording, and .....
        public int? FrameHavingTopScore { get; set; }        // id of frame having best score in recording 
        public double? VocalBestLocation { get; set; }	// its location in seconds from beginning of recording

        #endregion




    }
}