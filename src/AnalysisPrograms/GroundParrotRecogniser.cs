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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.AED;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// The ground parrot recognizer.
    /// </summary>
    public class GroundParrotRecogniser : AbstractStrongAnalyser
    {
        public const string CommandName = "GroundParrot";
        private const string EcosoundsGroundParrotIdentifier = "Ecosounds.GroundParrot";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Description => "Uses event pattern recognition for ground-parrots";

        [Command(
            CommandName,
            Description = "[UNMAINTAINED] event pattern recognition - used for ground-parrots (BRAD CLOW version)")]
        public class Arguments : SourceAndConfigArguments
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                GroundParrotRecogniser.Execute(this);
                return this.Ok();
            }
        }

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
        /// Col4: minimum freq (Hz) of the rectangle.
        /// </summary>
        private static readonly double[,] GroundParrotTemplate1 =
            {
                {
                    13.374694, 13.548844, 3832.910156, 3617.578125,
                },
                {
                    13.664943, 13.792653, 3919.042969, 3660.644531,
                },
                {
                    13.920363, 14.117732, 3962.109375, 3703.710938,
                },
                {
                    14.257052, 14.349932, 4005.175781, 3832.910156,
                },
                {
                    14.512472, 14.640181, 4048.242188, 3919.042969,
                },
                {
                    14.814331, 14.895601, 4220.507813, 4048.242188,
                },
                {
                    15.046531, 15.232290, 4349.707031, 4048.242188,
                },
                {
                    15.371610, 15.499320, 4435.839844, 4177.441406,
                },
                {
                    15.615420, 15.812789, 4478.906250, 4220.507813,
                },
                {
                    16.277188, 16.462948, 4608.105469, 4263.574219,
                },
                {
                    16.590658, 16.695147, 4694.238281, 4392.773438,
                },
                {
                    16.834467, 17.020227, 4694.238281, 4392.773438,
                },
                {
                    17.147937, 17.264036, 4737.304688, 4478.906250,
                },
                {
                    17.391746, 17.577506, 4823.437500, 4478.906250,
                },
                {
                    17.705215, 17.821315, 4780.371094, 4521.972656,
                },
            };

        /// <summary>
        /// Detect using EPR.
        /// </summary>
        /// <param name="wavFilePath">
        ///     The wav file path.
        /// </param>
        /// <param name="eprNormalisedMinScore">
        ///     The epr Normalised Min Score.
        /// </param>
        /// <returns>
        /// Tuple containing base Sonogram and list of acoustic events.
        /// </returns>
        public static Tuple<BaseSonogram, List<AcousticEvent>> Detect(FileInfo wavFilePath, Aed.AedConfiguration aedConfiguration, double eprNormalisedMinScore, TimeSpan segmentStartOffset)
        {
            Tuple<EventCommon[], AudioRecording, BaseSonogram> aed = Aed.Detect(wavFilePath, aedConfiguration, segmentStartOffset);

            var events = aed.Item1;
            var newEvents = new List<AcousticEvent>();
            foreach (var be in events)
            {
                newEvents.Add(EventConverters.ConvertSpectralEventToAcousticEvent((SpectralEvent)be));
            }

            var aeEvents = newEvents.Select(ae => Util.fcornersToRect(ae.TimeStart, ae.TimeEnd, ae.HighFrequencyHertz, ae.LowFrequencyHertz)).ToList();

            Log.Debug("EPR start");

            var eprRects = EventPatternRecog.DetectGroundParrots(aeEvents, eprNormalisedMinScore);
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
                    segmentStartOffset,
                    rectScore.Item1.Left,
                    rectScore.Item1.Right - rectScore.Item1.Left,
                    rectScore.Item1.Bottom,
                    rectScore.Item1.Top);
                ae.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                ae.SetTimeAndFreqScales(sonogram.NyquistFrequency, sonogram.Configuration.WindowSize, 0);
                ae.SetScores(rectScore.Item2, 0, 1);
                ae.BorderColour = aedConfiguration.AedEventColor;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.SegmentDurationSeconds = aed.Item2.Duration.TotalSeconds;

                eprEvents.Add(ae);
            }

            return Tuple.Create(sonogram, eprEvents);
        }

        public static void Execute(Arguments arguments)
        {
            MainEntry.WarnIfDeveloperEntryUsed();

            if (arguments == null)
            {
                throw new NoDeveloperMethodException();
            }

            // READ PARAMETER VALUES FROM INI FILE
            var config = ConfigFile.Deserialize(arguments.Config);
            var aedConfig = Aed.GetAedParametersFromConfigFileOrDefaults(config);

            var input = arguments.Source;
            Tuple<BaseSonogram, List<AcousticEvent>> result = Detect(input, aedConfig, Default.eprNormalisedMinScore, TimeSpan.Zero);
            List<AcousticEvent> eprEvents = result.Item2;

            eprEvents.Sort((ae1, ae2) => ae1.TimeStart.CompareTo(ae2.TimeStart));

            LoggedConsole.WriteLine();
            foreach (AcousticEvent ae in eprEvents)
            {
                LoggedConsole.WriteLine(ae.TimeStart + "," + ae.EventDurationSeconds + "," + ae.LowFrequencyHertz + "," + ae.HighFrequencyHertz);
            }

            LoggedConsole.WriteLine();
            string outputFolder = arguments.Config.ToFileInfo().DirectoryName;
            string wavFilePath = input.FullName;
            BaseSonogram sonogram = result.Item1;
            string imagePath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(wavFilePath) + ".png");
            var image = Aed.DrawSonogram(sonogram, eprEvents.ConvertAcousticEventsToSpectralEvents());
            image.Save(imagePath);
        }

        /// <summary>
        /// Get epr parameters from init file.
        /// </summary>
        /// <param name="configuration">The Config configuration object to read.</param>
        internal static double GetEprParametersFromConfigFileOrDefaults(Config configuration)
        {
            return configuration.GetDoubleOrNull(KeyNormalizedMinScore) ?? Default.eprNormalisedMinScore;
        }

        /// <summary>
        /// Takes the template defined by Birgit and converts it to integer bins using the user supplied time &amp; hz scales.
        /// </summary>
        /// <param name="timeScale">seconds per frame.</param>
        /// <param name="hzScale">herz per freq bin.</param>
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
                gpTemplate[r, 2] = (int)Math.Round(GroundParrotTemplate1[r, 2] / hzScale);
                gpTemplate[r, 3] = (int)Math.Round((GroundParrotTemplate1[r, 3] - GroundParrotTemplate1[r, 2]) / hzScale);
            }

            return gpTemplate;
        }

        public static List<AcousticEvent> ReadGroundParrotTemplateAsList(BaseSonogram sonogram)
        {
            var timeScale = sonogram.FrameStep;
            var hzScale = (int)sonogram.FBinWidth;
            int rows = GroundParrotTemplate1.GetLength(0);
            double timeOffset = GroundParrotTemplate1[0, 0];
            var gpTemplate = new List<AcousticEvent>();
            for (int r = 0; r < rows; r++)
            {
                int t1 = (int)Math.Round((GroundParrotTemplate1[r, 0] - timeOffset) / timeScale);
                int t2 = (int)Math.Round((GroundParrotTemplate1[r, 1] - timeOffset) / timeScale);
                int f2 = (int)Math.Round(GroundParrotTemplate1[r, 2] / hzScale);
                int f1 = (int)Math.Round(GroundParrotTemplate1[r, 3] / hzScale);
                Oblong o = new Oblong(t1, f1, t2, f2);
                var ae = AcousticEvent.InitializeAcousticEvent(
                    TimeSpan.Zero,
                    o,
                    sonogram.NyquistFrequency,
                    sonogram.Configuration.FreqBinCount,
                    sonogram.FrameDuration,
                    sonogram.FrameStep,
                    sonogram.FrameCount);

                gpTemplate.Add(ae);
            }

            return gpTemplate;
        }

        public override string DisplayName => "Ground Parrot Recognizer";

        public override string Identifier => EcosoundsGroundParrotIdentifier;

        public override Status Status => Status.Unmaintained;

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            FileInfo audioFile = segmentSettings.SegmentAudioFile;

            var eprNormalizedMinScore = GetEprParametersFromConfigFileOrDefaults(analysisSettings.Configuration);

            var aedConfigFile = ConfigFile.Resolve(
                analysisSettings.Configuration["AedConfig"],
                analysisSettings.ConfigFile.Directory);

            var rawAedConfig = ConfigFile.Deserialize(aedConfigFile);
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
                // noop
            }

            // save image of sonograms
            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                Image image = Aed.DrawSonogram(sonogram, results.Item2.ConvertAcousticEventsToSpectralEvents());
                image.Save(segmentSettings.SegmentImageFile.FullName);
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
            Csv.WriteToCsv(destination, results.Cast<EventIndex>());
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