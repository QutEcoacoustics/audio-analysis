using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    
        public class CompactCandidates
    {             
        /// <summary>
        /// It indicates the start time of a candidate.
        /// </summary>
        public double StartTime  {get; set;}

        /// <summary>
        /// It indicates the end time of a candidate.
        /// </summary>
        public double EndTime { get; set; }

        /// <summary>
        /// It indidates the audio file where the candidate come from. 
        /// </summary>
        public string SourceFilePath { get; set; }

        //public string SpeciesName { get; set; }
        /// <summary>
        /// A constructor
        /// </summary>
        /// <param name="score"></param>
        /// <param name="startTime"></param>
        /// <param name="maxFreq"></param>
        public CompactCandidates(double startTime, double duration, string sourceFile)
        {          
            this.StartTime = startTime;
            this.EndTime = startTime + duration;          
            this.SourceFilePath = sourceFile;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CompactCandidates()
        {
        }
    }
}
