namespace AnalysisPrograms
{
    using AnalysisPrograms.Production;
    using PowerArgs;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class AudioCutter
    {
        public class Arguments : SourceConfigOutputDirArguments, IArgClassValidator
        {
            
            [ArgDescription("The top-level directory to begin looking for audio files.")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo TopDirectory { get; set; }

            [ArgDescription("Whether to recurse into subdirectories.")]
            public bool Recurse { get; set; }

            [ArgDescription("The end offset to stop analysing (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            public void Validate()
            {
                Directory.EnumerateFiles("", "", SearchOption.TopDirectoryOnly);
                /*
                if (this.StartOffset.HasValue ^ this.EndOffset.HasValue)
                {
                    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specifified, then both must be specified");
                }

                if (this.StartOffset.HasValue && this.EndOffset.Value <= this.StartOffset.Value)
                {
                    throw new InvalidStartOrEndException("Start offset must be less than end offset.");
                }
                */
            }
        }
    }
}
