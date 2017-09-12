// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroundParrotRecogniser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLibrary;

    /// <summary>
    /// The ground parrot recognizer.
    /// </summary>
    public class GroundParrotRecogniser : AbstractStrongAnalyser
    {
        public class Arguments : SourceAndConfigArguments
        {
        }

        #region Constants and Fields

        /// <summary>
        /// The Key Normalized Min Score.
        /// </summary>
        public const string KeyNormalizedMinScore = "EprNormalizedMinScore";

        /// <summary>
        /// This is the ground parrot template used by Birgit and hard coded by Brad.
        /// It defines a set of 15 chirps enclosed in a rectangle.
        /// Each row represents one rectangle or chirp.
        /// Col1: start of the rectangle in seconds from beginning of the recording
        /// Col2: end   of the rectangle in seconds from beginning of the recording
        /// Col3: maximum freq (Hz) of the rectangle
        /// Col4: minimum freq (Hz) of the rectangle
        /// </summary>
        private static readonly double[,] GroundParrotTemplate1 =
            {
                { 13.374694, 13.548844, 3832.910156, 3617.578125 },
                { 13.664943, 13.792653, 3919.042969, 3660.644531 },
                { 13.920363, 14.117732, 3962.109375, 3703.710938 },
                { 14.257052, 14.349932, 4005.175781, 3832.910156 },
                { 14.512472, 14.640181, 4048.242188, 3919.042969 },
                { 14.814331, 14.895601, 4220.507813, 4048.242188 },
                { 15.046531, 15.232290, 4349.707031, 4048.242188 },
                { 15.371610, 15.499320, 4435.839844, 4177.441406 },
                { 15.615420, 15.812789, 4478.906250, 4220.507813 },
                { 16.277188, 16.462948, 4608.105469, 4263.574219 },
                { 16.590658, 16.695147, 4694.238281, 4392.773438 },
                { 16.834467, 17.020227, 4694.238281, 4392.773438 },
                { 17.147937, 17.264036, 4737.304688, 4478.906250 },
                { 17.391746, 17.577506, 4823.437500, 4478.906250 },
                { 17.705215, 17.821315, 4780.371094, 4521.972656 },
            };

        #endregion

        #region Public Methods

        /// <summary>
        /// Detect using EPR.
        /// </summary>
        /// <param name="wavFilePath">
        ///     The wav file path.
        /// </param>
        /// <param name="aedConfiguration"></param>
        /// <param name="eprNormalisedMinScore">
        ///     The epr Normalised Min Score.
        /// </param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="intensityThreshold">
        /// The intensity Threshold.
        /// </param>
        /// <param name="bandPassFilterMaximum">
        /// The band Pass Filter Maximum.
        /// </param>
        /// <param name="bandPassFilterMinimum">
        /// The band Pass Filter Minimum.
        /// </param>
        /// <param name="smallAreaThreshold">
        /// The small Area Threshold.
        /// </param>
        /// <returns>
        /// Tuple containing base Sonogram and list of acoustic events.
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(FileInfo wavFilePath, Aed.AedConfiguration aedConfiguration, double eprNormalisedMinScore, TimeSpan segmentStartOffset)
        {
            Tuple<AcousticEvent[], AudioRecording, BaseSonogram> aed = Aed.Detect(wavFilePath, aedConfiguration, segmentStartOffset);

            var events = aed.Item1.Select(ae => Util.fcornersToRect(ae.TimeStart, ae.TimeEnd, ae.MaxFreq, ae.MinFreq)).ToList();

            Log.Debug("EPR start");

            IEnumerable<Tuple<Util.Rectangle<double, double>, double>> eprRects =
                EventPatternRecog.DetectGroundParrots(events, eprNormalisedMinScore);
            Log.Debug("EPR finished");

            var sonogram = aed.Item3;
            SonogramConfig config = sonogram.Configuration;
            double framesPerSec = 1 / config.GetFrameOffset(); // Surely this should go somewhere else
            double freqBinWidth = config.NyquistFreq / (double)config.FreqBinCount;

            // TODO this is common with AED
            var eprEvents = new List<AcousticEvent>();
            foreach (var rectScore in eprRects)
            {
                var ae = new AcousticEvent(
                    rectScore.Item1.Left, rectScore.Item1.Right - rectScore.Item1.Left, rectScore.Item1.Bottom, rectScore.Item1.Top);
                ae.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                ae.SetTimeAndFreqScales(sonogram.NyquistFrequency, sonogram.Configuration.WindowSize, 0 );
                ae.SetScores(rectScore.Item2, 0, 1);
                ae.BorderColour = aedConfiguration.AedEventColor;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = aed.Item2.Duration;

                eprEvents.Add(ae);
            }

            return Tuple.Create(sonogram, eprEvents);
        }

        /// <summary>
        /// The standard dev method.
        /// </summary>
        /// <param name="args">
        /// The args passed into executable.
        /// </param>
        public static void Dev(Arguments arguments)
        {
            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            // "Example: \"trunk\\AudioAnalysis\\Matlab\\EPR\\Ground Parrot\\GParrots_JB2_20090607-173000.wav_minute_3.wav\""

            // READ PARAMETER VALUES FROM INI FILE
            var aedConfig = Aed.GetAedParametersFromConfigFileOrDefaults(arguments.Config);

            Tuple<BaseSonogram, List<AcousticEvent>> result = Detect(arguments.Source, aedConfig, Default.eprNormalisedMinScore, TimeSpan.Zero);
            List<AcousticEvent> eprEvents = result.Item2;

            eprEvents.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));

            LoggedConsole.WriteLine();
            foreach (AcousticEvent ae in eprEvents)
            {
                LoggedConsole.WriteLine(ae.TimeStart + "," + ae.Duration + "," + ae.MinFreq + "," + ae.MaxFreq);
            }

            LoggedConsole.WriteLine();

            string outputFolder = arguments.Config.DirectoryName;
            string wavFilePath = arguments.Source.FullName;
            BaseSonogram sonogram = result.Item1;
            string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavFilePath) + ".png");
            var image = Aed.DrawSonogram(sonogram, eprEvents);
            image.Save(imagePath, ImageFormat.Png);
            //ProcessingTypes.SaveAeCsv(eprEvents, outputFolder, wavFilePath);

            Log.Info("Finished");

        }


        #endregion

        #region helper methods

        /// <summary>
        /// Get epr parameters from init file.
        /// </summary>
        /// <param name="configuration">Th=e dynamic configuration object to read</param>
        internal static double GetEprParametersFromConfigFileOrDefaults(dynamic configuration)
        {
            return (double?)configuration[KeyNormalizedMinScore] ?? Default.eprNormalisedMinScore;
        }


        /// <summary>
        /// Takes the template defined by Birgit and converts it to integer bins using the user supplied time & hz scales
        /// </summary>
        /// <param name="timeScale">seconds per frame.</param>
        /// <param name="hzScale">herz per freq bin</param>
        /// <returns></returns>
        public static int[,] ReadGroundParrotTemplateAsMatrix(double timeScale, int hzScale)
        {
            int rows = GroundParrotTemplate1.GetLength(0);
            int cols = GroundParrotTemplate1.GetLength(1);
            double timeOffset = GroundParrotTemplate1[0, 0];
            var gpTemplate = new int[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                gpTemplate[r, 0] = (int)Math.Round((GroundParrotTemplate1[r, 0] - timeOffset) / timeScale);
                gpTemplate[r, 1] = (int)Math.Round((GroundParrotTemplate1[r, 1] - timeOffset) / timeScale);
                gpTemplate[r, 2] = (int)Math.Round((GroundParrotTemplate1[r, 2] / hzScale));
                gpTemplate[r, 3] = (int)Math.Round((GroundParrotTemplate1[r, 3] - GroundParrotTemplate1[r, 2]) / hzScale);
            }
            return gpTemplate;
        }

        public static List<AcousticEvent> ReadGroundParrotTemplateAsList(BaseSonogram sonogram)
        {
            var timeScale = sonogram.FrameStep;
            var hzScale = (int)sonogram.FBinWidth;
            int rows = GroundParrotTemplate1.GetLength(0);
            int cols = GroundParrotTemplate1.GetLength(1);
            double timeOffset = GroundParrotTemplate1[0, 0];
            var gpTemplate = new List<AcousticEvent>();
            for (int r = 0; r < rows; r++)
            {
                int t1 = (int)Math.Round((GroundParrotTemplate1[r, 0] - timeOffset) / timeScale);
                int t2 = (int)Math.Round((GroundParrotTemplate1[r, 1] - timeOffset) / timeScale);
                int f2 = (int)Math.Round(GroundParrotTemplate1[r, 2] / hzScale);
                int f1 = (int)Math.Round(GroundParrotTemplate1[r, 3] / hzScale);
                Oblong o = new Oblong(t1, f1, t2, f2);
                gpTemplate.Add(
                    new AcousticEvent(
                        o,
                        sonogram.NyquistFrequency,
                        sonogram.Configuration.FreqBinCount,
                        sonogram.FrameDuration,
                        sonogram.FrameStep,
                        sonogram.FrameCount));
            }
            return gpTemplate;
        }

        #endregion

        private const string EcosoundsGroundParrotIdentifier = "Ecosounds.GroundParrot";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string DisplayName
        {
            get
            {
                return "Ground Parrot Recognizer";
            }
        }

        public override string Identifier
        {
            get
            {
                return EcosoundsGroundParrotIdentifier;
            }
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            FileInfo audioFile = segmentSettings.SegmentAudioFile;

            var eprNormalizedMinScore = GetEprParametersFromConfigFileOrDefaults(analysisSettings.Configuration);

            var aedConfigFile = ConfigFile.ResolveConfigFile(
                (string)analysisSettings.Configuration.AedConfig,
                analysisSettings.ConfigFile.Directory);

            var rawAedConfig = Yaml.Deserialise(aedConfigFile);
            var aedConfig = Aed.GetAedParametersFromConfigFileOrDefaults(rawAedConfig);

            Tuple<BaseSonogram, List<AcousticEvent>> results = Detect(audioFile, aedConfig, eprNormalizedMinScore, segmentSettings.SegmentStartOffset);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, results.Item1.Duration)
                                      {
                                          AnalysisIdentifier = this.Identifier,
                                          Events = results.Item2.ToArray(),
                                      };
            BaseSonogram sonogram = results.Item1;

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteEventsFile(segmentSettings.SegmentEventsFile, analysisResults.Events);
                analysisResults.EventsFile = segmentSettings.SegmentEventsFile;
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
            }

            // save image of sonograms
            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                Image image = Aed.DrawSonogram(sonogram, results.Item2);
                image.Save(segmentSettings.SegmentImageFile.FullName, ImageFormat.Png);
                analysisResults.ImageFile = segmentSettings.SegmentImageFile;
            }

            return analysisResults;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            Aed.WriteEventsFileStatic(destination, results);
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            Log.Debug("SummarizeResults noop");
        }
    }
}