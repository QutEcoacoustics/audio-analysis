using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    public class Candidates
    {
        /// <summary>
        /// It could be distance or similarity score.
        /// </summary>
        public double Score {get; set;}
        
        /// <summary>
        /// It indicates the max frequency of a candidate.
        /// </summary>
        public double MaxFrequency {get; set;}

        /// <summary>
        /// It indicates the start time of a candidate.
        /// </summary>
        public double StartTime  {get; set;}

        public string SourceFilePath { get; set; }
        
        /// <summary>
        /// A constructor
        /// </summary>
        /// <param name="score"></param>
        /// <param name="startTime"></param>
        /// <param name="maxFreq"></param>
        public Candidates(double score, double startTime, double maxFreq, string sourceFile)
        {
            this.Score = score;
            this.StartTime = startTime;
            this.MaxFrequency = maxFreq;
            this.SourceFilePath = sourceFile;
        }

    }
}
