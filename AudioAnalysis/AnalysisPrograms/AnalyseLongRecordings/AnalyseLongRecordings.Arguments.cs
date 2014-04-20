using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AnalysisPrograms.Production;
using PowerArgs;

namespace AnalysisPrograms.AnalyseLongRecordings
{
    public partial class AnalyseLongRecording
    {
        public class Arguments : SourceConfigOutputDirArguments, IArgClassValidator
        {

            [ArgDescription("A TEMP directory where cut files will be stored. Use this option for effciency (e.g. write to a RAM Disk).")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo TempDir { get; set; }

            [ArgDescription("The start offset to start analysing from (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset to stop analysing (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            public void Validate()
            {
                if (this.StartOffset.HasValue ^ this.EndOffset.HasValue)
                {
                    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specifified, then both must be specified");
                }

                if (this.StartOffset.HasValue && this.EndOffset.Value <= this.StartOffset.Value)
                {
                    throw new InvalidStartOrEndException("Start offset must be less than end offset.");
                }
            }
        }
    }
}
