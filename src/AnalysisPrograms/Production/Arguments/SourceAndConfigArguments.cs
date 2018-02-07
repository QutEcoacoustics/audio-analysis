// <copyright file="SourceAndConfigArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;

    public abstract class SourceAndConfigArguments
        : SourceArguments
    {
        [Option(
            "The path to the config file.If not found it will attempt to use the default config file of the same name.",
            ShortName = "c",
            ValueName = "FILE")]
        [Required]
        [LegalFilePath]
        public FileInfo Config { get; set; }
    }
}