// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Draw.Zooming
{
    using System;
    using Acoustics.Shared;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.LongDurationSpectrograms.Zooming;
    using log4net;
    using PowerArgs;
    using Production;

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
                throw new NotSupportedException();
                arguments = Dev();
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

            var io = FileSystemProvider.GetInputOutputFileSystems(arguments.SourceDirectory, arguments.Output);

            LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + arguments.SpectrogramZoomingConfig);
            LoggedConsole.WriteLine("# Input Directory             : " + arguments.SourceDirectory);
            LoggedConsole.WriteLine("# Output Directory            : " + arguments.Output);

            var common = new ZoomArguments();

            common.SpectrogramZoomingConfig = Yaml.Deserialise<SpectrogramZoomingConfig>(arguments.SpectrogramZoomingConfig);

            // search for index properties config
            var indexPropertiesPath = IndexProperties.Find(common.SpectrogramZoomingConfig, arguments.SpectrogramZoomingConfig);
            Log.Debug("Using index properties file: " + indexPropertiesPath.FullName);

            // load the index properties
            common.IndexProperties = IndexProperties.GetIndexProperties(indexPropertiesPath);

            // get the indexDistributions and the indexGenerationData AND the common.OriginalBasename
            common.CheckForNeededFiles(arguments.SourceDirectory.ToDirectoryInfo());

            LoggedConsole.WriteLine("# File name of recording      : " + common.OriginalBasename);

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
                        throw new ArgException("FocusMinute is null, cannot proceed");
                    }

                    const int ImageWidth = 1500;
                    ZoomFocusedSpectrograms.DrawStackOfZoomedSpectrograms(
                        arguments.SourceDirectory.ToDirectoryInfo(),
                        arguments.Output.ToDirectoryInfo(),
                        common,
                        focalTime,
                        ImageWidth);
                    break;
                case Arguments.ZoomActionType.Tile:
                    // Create the super tiles for a full set of recordings
                    ZoomTiledSpectrograms.DrawTiles(
                        arguments.SourceDirectory,
                        arguments.Output.ToDirectoryInfo(),
                        common,
                        Acoustic.TowseyAcoustic);

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