// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;

    using log4net;

    using PowerArgs;

    /// <summary>
    /// First argument on command line to call this action is "ZoomingSpectrograms"
    /// Activity Codes for other tasks to do with spectrograms and audio files:
    /// 
    /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
    /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
    /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
    /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
    /// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
    /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
    ///
    /// audiofilecheck - Writes information about audio files to a csv file.
    /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
    /// audiocutter - Cuts audio into segments of desired length and format
    /// createfoursonograms 
    /// </summary>
    public static class DrawZoomingSpectrograms
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "zoomingSpectrograms"
        /// </summary>
        /// <returns>
        /// The <see cref="Arguments"/>.
        /// </returns>
        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES
            // 2010 Oct 13th
            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            // string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct13_SpectralIndices";

            // 2010 Oct 14th
            // string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct14_SpectralIndices";

            // 2010 Oct 15th
            // string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct15_SpectralIndices";

            // 2010 Oct 16th
            // string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct16_SpectralIndices";

            // 2010 Oct 17th
            // string ipFileName = "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000";
            // string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct17_SpectralIndices";

            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.OneSecondIndices";
            // string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.200msIndicesKIWI-TEST";

            // KOALA RECORDING AT ST BEES
            //string ipdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016\SpectrogramFocalZoom";
            //string opdir = @"C:\SensorNetworks\Output\KoalaMale\StBeesIndices2016";


            // BAC
            string ipdir = @"C:\SensorNetworks\Output\BAC\HiRes\Towsey.Acoustic";
            string opdir = @"C:\SensorNetworks\Output\BAC\HiRes";


            // ECLIPSE FARMSTAY
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Eclipse\EclipseFarmstay.200ms\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";

            //BRISTLE BIRD
            //string ipdir = @"C:\SensorNetworks\Output\BristleBird\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImageBristleBird";

            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramFocalZoom\FocalZoomImage";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramTileZoom\TiledImages";

            // ################ TEST a colour scheme for the high resolution frame spectrograms.
            //var cch = TowseyLibrary.CubeHelix.GetCubeHelix();
            //cch.TestImage(Path.Combine(opdir, "testImageColorHelixScale.png"));
            //var rsp = new TowseyLibrary.CubeHelix("redscale");
            //rsp.TestImage(Path.Combine(opdir, "testImageRedScale1.png"));
            //var csp = new TowseyLibrary.CubeHelix("cyanscale");
            //csp.TestImage(Path.Combine(opdir, "testImageCyanScale1.png"));
            // ################ TEST a colour scheme for the high resolution frame spectrograms.

            var opDir = new DirectoryInfo(opdir);

            //string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramScalingConfig.json";
            string config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";


            return new Arguments
                       {
                           // use the default set of index properties in the AnalysisConfig directory.
                           SourceDirectory = ipdir.ToDirectoryInfo(),
                           Output = opdir.ToDirectoryInfo(),
                           SpectrogramTilingConfig = config.ToFileInfo(),

                           // draw a focused multi-resolution pyramid of images
                           //ZoomAction = Arguments.ZoomActionType.Tile,
                           ZoomAction = Arguments.ZoomActionType.Focused,
                           FocusMinute = 1,
                           //FocusMinute = 61,
                       };
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            string date = "# DATE AND TIME: " + DateTime.Now;
            string description;
            switch (arguments.ZoomAction)
            {
                case Arguments.ZoomActionType.Focused:
                    description =
                        "# DRAW STACK OF FOCUSED MULTI-SCALE LONG DURATION SPECTROGRAMS DERIVED FROM SPECTRAL INDICES.";
                    break;
                case Arguments.ZoomActionType.Tile:
                    description =
                        "# DRAW ZOOMING SPECTROGRAMS DERIVED FROM CSV FILES OF SPECTRAL INDICES OBTAINED FROM AN AUDIO RECORDING";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LoggedConsole.WriteLine(description);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + arguments.SpectrogramTilingConfig);
            LoggedConsole.WriteLine("# Input Directory             : " + arguments.SourceDirectory);
            LoggedConsole.WriteLine("# Output Directory            : " + arguments.Output);

            var common = new ZoomCommonArguments();

            common.SuperTilingConfig = Yaml.Deserialise<SuperTilingConfig>(arguments.SpectrogramTilingConfig);
            var indexPropertiesPath = IndexProperties.Find(common.SuperTilingConfig, arguments.SpectrogramTilingConfig);

            //#################################
            // CHECK THIS CHANGE WITH ANTHONY
            indexPropertiesPath = arguments.SpectrogramTilingConfig;

            Log.Debug("Using index properties file: " + indexPropertiesPath.FullName);
            common.IndexProperties = IndexProperties.GetIndexProperties(indexPropertiesPath);

            // get the indexDistributions and the indexGenerationData AND the //common.OriginalBasename
            common.CheckForNeededFiles(arguments.SourceDirectory);
            // Create directory if not exists
            if (!arguments.Output.Exists)
            {
                arguments.Output.Create();
            }

            LoggedConsole.WriteLine("# File name of recording      : " + common.OriginalBasename);
            LoggedConsole.WriteLine();


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
                        // ReSharper disable once NotResolvedInText
                        throw new ArgumentNullException("FocusMinute", "Focus minute is null, cannot proceed");
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
                        common);
            
                    break;
                default:
                    Log.Warn("Other ZoomAction results in standard LD Spectrogram to be drawn");
                    // draw standard false colour spectrograms - useful to check what spectrograms of the indiviual indices are like. 

                    throw new NotImplementedException();
                    /*LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                    arguments.SourceDirectory,
                    arguments.Output,
                    arguments.SpectrogramConfigPath,
                    arguments.IndexPropertiesConfig);*/
                    break;
            }
        }

        #endregion

        public class Arguments
        {
            public enum ZoomActionType
            {
                Focused,
                Tile
            }

            #region Public Properties

            public int? FocusMinute { get; set; }

            [ArgDescription("A directory to write output to")]
            [Production.ArgExistingDirectory(createIfNotExists: true)]
            [ArgRequired]
            [ArgPosition(3)]
            public DirectoryInfo Output { get; set; }

            [ArgDescription(
                "The source directory of files output from Towsey.Acoustic (the Index analysis) to operate on")]
            [Production.ArgExistingDirectory]
            [ArgPosition(2)]
            [ArgRequired]
            public DirectoryInfo SourceDirectory { get; set; }

            [ArgDescription("User specified file defining valid spectrogram scales. Also should contain a reference to IndexProperties.yml and optionally a LDSpectrogramConfig object")]
            [Production.ArgExistingFile(Extension = ".yml")]
            [ArgRequired]
            public FileInfo SpectrogramTilingConfig { get; set; }

            [ArgDescription("Choose which action to execute (Focused, or Tile)")]
            [ArgRequired]
            [ArgPosition(1)]
            public ZoomActionType ZoomAction { get; set; }

            #endregion
        }
    }
}