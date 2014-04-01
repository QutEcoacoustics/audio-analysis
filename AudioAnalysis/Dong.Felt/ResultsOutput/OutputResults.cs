using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools;
using System.IO;
using TowseyLib;
using AudioBase;
using AnalysisRunner;
using Acoustics.Tools;
using Acoustics.Shared;

namespace Dong.Felt.ResultsOutput
{
    public class OutputResults
    {
        /// <summary>
        /// To use this method, you have to make sure there is no file with the same name of fiOutputSegment in the specific fold. 
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="fiOutputSegment"></param>
        public static void AudioSegmentBasedCandidates(Candidates candidate, FileInfo fiOutputSegment)
        {
            // to cut out 2 sec either side, here buffer means the time offset.
            var buffer = new TimeSpan(0, 0, 1);
            var startTime = TimeSpan.FromMilliseconds(candidate.StartTime - buffer.TotalMilliseconds);
            var endTime = TimeSpan.FromMilliseconds(candidate.EndTime + buffer.TotalMilliseconds);           
            var sampleRate = 22050;
            if (startTime.TotalMilliseconds < 0)
            {
                startTime = TimeSpan.FromMilliseconds(candidate.StartTime);
                endTime = TimeSpan.FromMilliseconds(candidate.EndTime + 2 * buffer.TotalMilliseconds); 
            }
            var request = new AudioUtilityRequest
                    {
                        TargetSampleRate = sampleRate,
                        OffsetStart = startTime,
                        OffsetEnd = endTime
                    };
            
            var result = AudioFilePreparer.PrepareFile(candidate.SourceFilePath.ToFileInfo(), fiOutputSegment, request, TempFileHelper.TempDir());       
        }


    }
}
