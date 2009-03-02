using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QutSensors;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
	public class Results : BaseResult
	{
        public Results(Template_CC template)
        {
            Template = template;
        }

		#region Properties
		public string ID { get; set; }
		public Template_CC Template { get; private set; }

		public double[,] AcousticMatrix { get; set; }	// matrix of fv x time frames
		public string SyllSymbols { get; set; }			// symbol sequence output from the acoustic model 
		public int[] SyllableIDs { get; set; }			// above symbol sequence represented as array of integers - noise=0
        public int? VocalCount { get; set; }			// number of hits i.e. vocalisatsions in the symbol seuqence
        public int? VocalValid { get; set; }            // number of hits/vocalisations whose duration is valid for call
        public double[] VocalScores { get; set; }		// array of scores derived from user defined call templates
		public double? VocalBestScore { get; set; }	    // the best score in recording, and .....
        public int? VocalBestFrame { get; set; }        // id of frame having best score in recording 
        public double? VocalBestLocation { get; set; }	// its location in seconds from beginning of recording

        public double? MaxScore { get; set; }	        // upper limit for diplay of scores 
        public double? LLRThreshold { get; set; }       // significance threshold for display of LLR scores

		public int? CallPeriodicity_frames { get; set; }
		public int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }
		#endregion


		#region Comma Separated Summary Methods
		public static string GetSummaryHeader()
		{
			return "ID,Hits,MaxScr,MaxLoc";
		}

		public string GetOneLineSummary()
		{
			return string.Format("{0},{1},{2:F1},{3:F1}", ID, NumberOfPeriodicHits, VocalBestScore, VocalBestLocation);
		}
		#endregion
	}
}