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
		public string ID { get; set; }

        public int? VocalCount { get; set; }			// number of hits i.e. vocalisatsions in the symbol seuqence
        public int? VocalValid { get; set; }            // number of hits/vocalisations whose duration is valid for call
		public double? VocalBestScore { get; set; }	    // the best score in recording, and .....
        public int? VocalBestFrame { get; set; }        // id of frame having best score in recording 
        public double? VocalBestLocation { get; set; }	// its location in seconds from beginning of recording

        public double? MaxScore { get; set; }	        // upper limit for diplay of scores 
        public double? LLRThreshold { get; set; }       // significance threshold for display of LLR scores

		#endregion



        public Result_MMErgodic(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR and put info in Results object 
            AcousticMatrix = Template.AcousticModelConfig.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModelConfig.SyllSymbols;    //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModelConfig.SyllableIDs;    //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;

        }


		#region Comma Separated Summary Methods
		public static string GetSummaryHeader()
		{
			return "ID,Hits,MaxScr,MaxLoc";
		}

		#endregion
	}
}