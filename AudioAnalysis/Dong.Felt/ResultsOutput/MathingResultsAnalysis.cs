using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt.ResultsOutput
{
    public class MathingResultsAnalysis
    {
        /// <summary>
        /// To get or set the species name of query. 
        /// </summary>
        public string QuerySpeciesName { get; set; }

        /// <summary>
        /// To get or set the species name of query. 
        /// </summary>
        public string HitSpeciesName { get; set; }

        /// <summary>
        /// To get or set the ranking position of the hit in the ranking list. 
        /// </summary>
        public int FirstHitPosition { get; set; }

        /// <summary>
        /// To get or set the ranking position of the hit in the ranking list. 
        /// </summary>
        public int SecondHitPosition { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public int FirstHit { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public int SecondHit { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public int ThirdHit { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public int FourthHit { get; set; }

        /// <summary>
        /// To get or set whether hit is correct. 
        /// </summary>
        public int FifthHit { get; set; }

        /// <summary>
        /// To get or set the query path.  
        /// </summary>
        public string QueryAudioName { get; set; }
        
        /// <summary>
        /// To get or set the matched audio path.
        /// </summary>
        public string MatchedAudioName { get; set; }        




    }
}
