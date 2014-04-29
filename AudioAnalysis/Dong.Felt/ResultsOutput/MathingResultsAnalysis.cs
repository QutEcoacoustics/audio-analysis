using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.ResultsOutput
{
    public class MathingResultsAnalysis
    {
        /// <summary>
        /// To get or set the ranking position of the hit in the ranking list. 
        /// </summary>
        public int HitPosition { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public bool Hit { get; set; }

        /// <summary>
        /// To get or set the query path.  
        /// </summary>
        public string MatchedAudioName { get; set; }        




    }
}
