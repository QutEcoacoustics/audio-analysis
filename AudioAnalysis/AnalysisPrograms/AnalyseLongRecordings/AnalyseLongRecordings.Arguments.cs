// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalyseLongRecordings.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AnalyseLongRecording type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.AnalyseLongRecordings
{
    using System.IO;

    using AnalysisBase;

    using Production;

    using PowerArgs;

    public partial class AnalyseLongRecording
    {
        [CustomDetailedDescription]
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

            [ArgDescription("Allow advancing the start of the analysis to the nearest minute. A valid datetime must be available in the file name. Seed additional notes for options.")]
            [DefaultValue(TimeAlignment.None)]
            [ArgExample("help spt", "will print help for the spt action")]
            public TimeAlignment AlignToMinute { get; set; } = TimeAlignment.None;

            [ArgDescription("An array of channels to select. Default is all channels.")]
            public int[] Channels { get; set; } = null;

            [ArgDescription("Mix all selected input channels down into one mono channel. Default is to mixdown.")]
            [DefaultValue(true)]
            public bool MixDownToMono { get; set; } = true;

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

            public static string AdditionalNotes()
            {
                return @"
AlignToMinute Options:
  - None:        does no alignment (default) 
  - TrimBoth:    does alignment and removes fractional sections 
  - TrimNeither: does alignment and keeps fractional sections 
  - TrimStart:   does alignment and keeps last fractional segment 
  - TrimEnd:     does alignment and keeps first fractional segment ";
            }
        }
    }
}