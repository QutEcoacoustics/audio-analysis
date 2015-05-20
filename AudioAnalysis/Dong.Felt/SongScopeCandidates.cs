using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dong.Felt
{
    public class SongScopeCandidates:Candidates
    {
        /// <summary>
        /// It could be distance or similarity score.
        /// </summary>
        public double Quality { get; set; }

        /// <summary>
        /// It indicates the max frequency of a candidate.
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// It indicates the max frequency of a candidate.
        /// </summary>
        public string Recognizer { get; set; }

        /// <summary>
        /// A constructor
        /// </summary>
        /// <param name="score"></param>
        /// <param name="startTime"></param>
        /// <param name="maxFreq"></param>
        public SongScopeCandidates(double score, double startTime, double duration, double maxFreq, double minFreq, string sourceFile, double quality,
            double probability, string recognizer)
        {
            this.Score = score;
            this.StartTime = startTime;
            this.EndTime = startTime + duration;
            this.MaxFrequency = maxFreq;
            this.MinFrequency = minFreq;
            this.SourceFilePath = sourceFile;
            this.Quality = quality;
            this.Probability = probability;
            this.Recognizer = recognizer;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SongScopeCandidates()
        {
        }
    }
}
