using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysis
{
    public class Result_1PS : BaseResult
    {
		#region Properties


        public double? MaxScore { get; set; }	        // upper limit for diplay of scores 

		public int? CallPeriodicity_frames { get; set; }
		public int? CallPeriodicity_ms { get; set; }
		public int? NumberOfPeriodicHits { get; set; }
		#endregion



        public Result_1PS(BaseTemplate template)
        {
            Template = template;
            //ACCUMULATE OUTPUT SO FAR
            AcousticMatrix = Template.AcousticModelConfig.AcousticMatrix; //double[,] acousticMatrix
            SyllSymbols = Template.AcousticModelConfig.SyllSymbols;    //string symbolSequence = result.SyllSymbols;
            SyllableIDs = Template.AcousticModelConfig.SyllableIDs;    //int[] integerSequence = result.SyllableIDs;
            //ModelType type = Template.Model.ModelType;

        }


        public string GetOneLineSummary()
        {
            return string.Format("{0},{1},{2:F1},{3:F1}", ID, NumberOfPeriodicHits, TopScore, VocalBestLocation);
        }



    }
}
