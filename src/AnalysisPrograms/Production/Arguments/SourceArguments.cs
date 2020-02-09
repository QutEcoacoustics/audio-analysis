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

        [Argument(
            0,
            Description = "The source audio file to operate on")]
        [Required]
        [FileExists]
        public virtual FileInfo Source { get; set; }
    }
}