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
    using log4net;
    using PowerArgs;

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
            LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + arguments.SpectrogramTilingConfig);
            LoggedConsole.WriteLine("# Input Directory             : " + arguments.SourceDirectory);
            LoggedConsole.WriteLine("# Output Directory            : " + arguments.Output);

            var common = new ZoomArguments();

            common.SpectrogramZoomingConfig = Yaml.Deserialise<SpectrogramZoomingConfig>(arguments.SpectrogramTilingConfig);

            // search for index properties config
            var indexPropertiesPath = IndexProperties.Find(common.SpectrogramZoomingConfig, arguments.SpectrogramTilingConfig);
            Log.Debug("Using index properties file: " + indexPropertiesPath.FullName);

            // load the index properties
            common.IndexProperties = IndexProperties.GetIndexProperties(indexPropertiesPath);

            // get the indexDistributions and the indexGenerationData AND the common.OriginalBasename
            common.CheckForNeededFiles(arguments.SourceDirectory);

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
                        arguments.SourceDirectory,
                        arguments.Output,
                        common,
                        focalTime,
                        ImageWidth);
                    break;
                case Arguments.ZoomActionType.Tile:
                    // Create the super tiles for a full set of recordings
                    ZoomTiledSpectrograms.DrawTiles(
                        arguments.SourceDirectory,
                        arguments.Output,
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