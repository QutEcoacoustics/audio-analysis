namespace Dong.Felt.Representations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MFCC
    {
        /// <summary>
        /// The unit of Width is millison seconds.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// The unit of Height is millison seconds.
        /// </summary>
        public double EndTime { get; set; }

        public string audioFile { get; set; }

        public List<double> MFCCoefficients { get; set; }
        public MFCC()
        {
            MFCCoefficients = new List<double>();
        }

        public static List<MFCC> CombineMFCCfeatures(List<CompactCandidates> comCandidates, List<MFCC> mfccs)
        {
            var result = new List<MFCC>();
            // assume both lists have the same length
            for (int i = 0; i < comCandidates.Count(); i++)
            {
                if (mfccs[i].MFCCoefficients.Count != 0)
                {
                    mfccs[i].StartTime = comCandidates[i].StartTime;
                    mfccs[i].EndTime = comCandidates[i].EndTime;
                    mfccs[i].audioFile = comCandidates[i].SourceFilePath;
                    result.Add(mfccs[i]);
                }
            }
            return result;
        }
    }
}
