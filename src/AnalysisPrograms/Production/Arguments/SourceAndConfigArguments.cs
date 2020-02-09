// <copyright file="SourceAndConfigArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.ComponentModel.DataAnnotations;
    using McMaster.Extensions.CommandLineUtils;

    public abstract class SourceAndConfigArguments
        : SourceArguments
    {
        [Argument(
            1,
            Description = "The path to the config file. If not found it will attempt to use the default config file of the same name.")]
        [Required]
        [LegalFilePath]
        public string Config { get; set; }
    }
}