// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Draw.Zooming
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.LongDurationSpectrograms.Zooming;
    using log4net;
    using Production;

    using Zio;

    /// <summary>
    /// Renders index data as false color images at various scales, with various styles.
    /// </summary>
    public static partial class DrawZoomingSpectrograms
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(DrawZoomingSpectrograms));

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            string description;
            switch (arguments.ZoomAction)
            {
                case Arguments.ZoomActionType.Focused:
                    description =
                        "# DRAW STACK OF FOCUSED MULTI-SCALE LONG DURATION SPECTROGRAMS DERIVED FROM SPECTRAL INDICES.";
                    break;
                case Arguments.ZoomActionType.Tile:
                    description =
                        "# DRAW ZOOMING SPECTROGRAMS DERIVED FROM SPECTRAL INDICES OBTAINED FROM AN AUDIO RECORDING";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LoggedConsole.WriteLine(description);

            LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + arguments.SpectrogramZoomingConfig);
            LoggedConsole.WriteLine("# Input Directory             : " + arguments.SourceDirectory);
            LoggedConsole.WriteLine("# Output Directory            : " + arguments.Output);

            var common = new ZoomParameters(
                arguments.SourceDirectory.ToDirectoryEntry(),
                arguments.SpectrogramZoomingConfig.ToFileEntry(),
                !string.IsNullOrEmpty(arguments.OutputFormat));

            LoggedConsole.WriteLine("# File name of recording      : " + common.OriginalBasename);

            // create file systems for reading input and writing output
            var io = FileSystemProvider.GetInputOutputFileSystems(
                arguments.SourceDirectory,
                FileSystemProvider.MakePath(arguments.Output, common.OriginalBasename, arguments.OutputFormat, "Tiles"))
                .EnsureInputIsDirectory();

            switch (arguments.ZoomAction)
            {
                case Arguments.ZoomActionType.Focused:
                    // draw a focused multi-resolution pyramid of images
                    TimeSpan focalTime;
                    if (arguments.FocusMinute.HasValue)
                    {
                        focalTime = TimeSpan.FromMinutes(arguments.FocusMinute.Value);
                    }
                    else
                    {
                        throw new ArgumentException("FocusMinute is null, cannot proceed");
                    }

                    const int ImageWidth = 1500;
                    ZoomFocusedSpectrograms.DrawStackOfZoomedSpectrograms(
                        arguments.SourceDirectory.ToDirectoryInfo(),
                        arguments.Output.ToDirectoryInfo(),
                        common,
                        focalTime,
                        ImageWidth,
                        AcousticIndices.TowseyAcoustic);
                    break;
                case Arguments.ZoomActionType.Tile:
                    // Create the super tiles for a full set of recordings
                    ZoomTiledSpectrograms.DrawTiles(
                        io,
                        common,
                        AcousticIndices.TowseyAcoustic);

                    break;
                default:
                    Log.Warn("Other ZoomAction results in standard LD Spectrogram to be drawn");

                    // draw standard false color spectrograms - useful to check what spectrograms of the individual
                    // indices are like.

                    throw new NotImplementedException();
                    /*LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                    arguments.SourceDirectory,
                    arguments.Output,
                    arguments.SpectrogramConfigPath,
                    arguments.IndexPropertiesConfig);*/
                    break;
            }
        }
    }
}