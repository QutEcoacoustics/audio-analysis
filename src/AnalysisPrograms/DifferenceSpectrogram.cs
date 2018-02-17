// <copyright file="DifferenceSpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using AudioAnalysisTools.LongDurationSpectrograms;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using TowseyLibrary;

    public static class DifferenceSpectrogram
    {
        public const string CommandName = "DrawDifferenceSpectrogram";

        [Command(
            CommandName,
            Description = "[INOPERABLE] Produces a false-colour spectrogram that show only the differences between two spectrograms.")]
        public class Arguments : SubCommandBase
        {
            [Option(Description = "Path to configuration file in YAML format")]
            [ExistingFile]
            [Required]
            [LegalFilePath]
            public string Config { get; set; }

            //[ArgDescription("The directory containing the input files.")]
            //[Production.ArgExistingDirectory]
            //[ArgPosition(1)]
            //[ArgRequired]
            //public string InputDirectory { get; set; }

            //[ArgDescription("The directory to place output files.")]
            //[ArgPosition(2)]
            //[ArgRequired]
            //public string OutputDirectory { get; set; }

            //[ArgDescription("The first input csv file containing acosutic index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(3)]
            //[ArgRequired]
            //public string IndexFile1 { get; set; }

            //[ArgDescription("The third input csv file containing standard deviations of index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(4)]
            //[ArgRequired]
            //public string StdDevFile1 { get; set; }

            //[ArgDescription("The fourth input csv file containing acosutic index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(5)]
            //[ArgRequired]
            //public string IndexFile2 { get; set; }

            //[ArgDescription("The fifth input csv file containing standard deviations of index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(6)]
            //[ArgRequired]
            //public string StdDevFile2 { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                throw new NotImplementedException("The arguments for this class need to be fixed");
                DifferenceSpectrogram.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            // load YAML configuration
            Config configuration = ConfigFile.Deserialize(arguments.Config);

            LDSpectrogramDistance.DrawDistanceSpectrogram(configuration);

            LdSpectrogramDifference.DrawDifferenceSpectrogram(configuration);

            LdSpectrogramTStatistic.DrawTStatisticThresholdedDifferenceSpectrograms(configuration);
        }
    }
}
