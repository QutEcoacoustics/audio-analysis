// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.Arguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Draw.Zooming
{
    using System.IO;
    using PowerArgs;
    using Production;

    public static partial class DrawZoomingSpectrograms
    {
        public class Arguments
        {
            public enum ZoomActionType
            {
                Focused,
                Tile,
            }

            [ArgDescription("When doing a `Focused` stack, which minute to center on. Accepts partial minutes.")]
            public float? FocusMinute { get; set; }

            [ArgDescription(FileSystemProvider.DestinationPath)]
            [ArgRequired]
            [ArgPosition(3)]
            public string Output { get; set; }

            [ArgDescription(FileSystemProvider.DestinationFormat)]
            [ArgOneOfThese("", FileSystemProvider.SqlitePattern)]
            public string OutputFormat { get; set; }

            [ArgDescription("The source directory of files output from Towsey.Acoustic (the Index analysis) to operate on")]
            [ArgPosition(2)]
            [ArgRequired]
            public string SourceDirectory { get; set; }

            [ArgDescription("User specified file defining valid spectrogram scales. Also should contain a reference to IndexProperties.yml and optionally a LDSpectrogramConfig object")]
            [Production.ArgExistingFile(Extension = ".yml")]
            [ArgRequired]
            public FileInfo SpectrogramZoomingConfig { get; set; }

            [ArgDescription("Choose which action to execute (Focused, or Tile)")]
            [ArgRequired]
            [ArgPosition(1)]
            public ZoomActionType ZoomAction { get; set; }
        }
    }
}