// <copyright file="SourceArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;

    public abstract class SourceArguments
        : SubCommandBase
    {
        [Option(
            "The source audio file to operate on",
            ShortName = "s",
            ValueName = "FILE")]
        [Required]
        [FileExists]
        public FileInfo Source { get; set; }
    }
}