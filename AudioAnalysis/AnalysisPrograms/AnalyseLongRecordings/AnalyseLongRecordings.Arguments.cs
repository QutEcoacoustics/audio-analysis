// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecordings.Arguments.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System.IO;

    using AnalysisPrograms.Production;

    using PowerArgs;

    public partial class AnalyseLongRecording
    {
        public class Arguments : SourceConfigOutputDirArguments, IArgClassValidator
        {
            public Arguments()
            {
#if DEBUG
                this.WhenExitCopyLog = true;
                this.WhenExitCopyConfig = true;
#endif
            }

            [ArgDescription("A TEMP directory where cut files will be stored. Use this option for efficiency (e.g. write to a RAM Disk).")]
            [Production.ArgExistingDirectory]
            public DirectoryInfo TempDir { get; set; }

            [ArgDescription("The start offset to start analyzing from (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset to stop analyzing (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            public void Validate()
            {
                if (this.StartOffset.HasValue ^ this.EndOffset.HasValue)
                {
                    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
                }

                if (this.StartOffset.HasValue && this.EndOffset.HasValue && this.EndOffset.Value <= this.StartOffset.Value)
                {
                    throw new InvalidStartOrEndException("Start offset must be less than end offset.");
                }
            }

            [ArgDescription("If true, attempts to copy the executable's log file to output directory. If it can't determine an output directory, it copies to the working directory.")]
#if DEBUG
            [DefaultValue(true)]
#else
        [DefaultValue(false)]
#endif
            public bool WhenExitCopyLog { get; set; }

            [ArgDescription("If true, attempts to copy the executable's config file to output directory. If it can't determine an output directory, it copies to the working directory. If it can't find a config file, nothing is copied")]
#if DEBUG
            [DefaultValue(true)]
#else
        [DefaultValue(false)]
#endif
            public bool WhenExitCopyConfig { get; set; }
        }
    }
}