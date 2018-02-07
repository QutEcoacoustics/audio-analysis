// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Draw.Zooming
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;

    public static partial class DrawZoomingSpectrograms
    {
        public const string CommandName = "DrawZoomingSpectrograms";

        [Command(
            CommandName,
            Description = "Produces long-duration false-colour spectrograms on different time scales.",
            ExtendedHelpText = "The `Tile` option will produce a full pyramid of tiles whereas the `Focused` option produces just one image")]
        public class Arguments : SubCommandBase
        {
            public enum ZoomActionType
            {
                Focused,
                Tile,
            }

            [Option("When doing a `Focused` stack, which minute to center on. Accepts partial minutes.")]
            [InRange(min: 0)]
            public double? FocusMinute { get; set; }

            [Option(FileSystemProvider.DestinationPath)]
            [Required]
            [LegalFilePath]
            public string Output { get; set; }

            [Option(FileSystemProvider.DestinationFormat)]
            [OneOfThese("", FileSystemProvider.SqlitePattern)]
            public string OutputFormat { get; set; } = string.Empty;

            [Option("The source directory of files output from Towsey.Acoustic (the Index analysis) to operate on")]
            [Required]
            [DirectoryExists]
            public string SourceDirectory { get; set; }

            [Option("User specified file defining valid spectrogram scales. Also should contain a reference to IndexProperties.yml and optionally a LDSpectrogramConfig object")]
            [ExistingFile(Extension = ".yml")]
            [Required]
            public FileInfo SpectrogramZoomingConfig { get; set; }

            [Option("Choose which action to execute (Focused, or Tile)")]
            [Required]
            public ZoomActionType ZoomAction { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                DrawZoomingSpectrograms.Execute(this);
                return this.Ok();
            }
        }
    }
}