// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AED.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Acoustic Event Detection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.AED;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Production.Arguments;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.FSharp.Core;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    /// <summary>
    ///     Acoustic Event Detection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.", Scope = "Class")]
    public class Aed : AbstractStrongAnalyser
    {
        public const string CommandName = "AED";

        public override string Description => "[BETA] Acoustic event detection, for short files (~ 1 min)";

        /// <summary>
        /// The arguments.
        /// </summary>
        [Command(
            CommandName,
            Description = "[BETA] Acoustic event detection, for short files (~ 1 min)")]
        public class Arguments : SourceConfigOutputDirArguments
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                Aed.Execute(this);
                return this.Ok();
            }
        }

        [YamlTypeTag(typeof(AedConfiguration), "AedParameters")]
        public class AedConfiguration : Config
        {
            public AedConfiguration()
            {
                this.AedEventColor = Color.Red;
                this.AedHitColor = this.AedEventColor.WithAlpha(0.5f);
                this.NoiseReductionType = NoiseReductionType.None;
            }

            public double IntensityThreshold { get; set; }

            public int SmallAreaThreshold { get; set; }

            public int? BandpassMinimum { get; set; }

            public int? BandpassMaximum { get; set; }

            public NoiseReductionType NoiseReductionType { get; set; }

            [YamlDotNet.Serialization.YamlMember(Alias = AnalysisKeys.NoiseBgThreshold)]
            public double NoiseReductionParameter { get; set; }

            public int ResampleRate { get; set; }

            public Color AedEventColor { get; set; }

            public Color AedHitColor { get; set; }

            public bool IncludeHitElementsInOutput { get; set; } = false;
        }

        /// <summary>
        /// The ecosounds aed identifier.
        /// </summary>
        private const string EcosoundsAedIdentifier = "Ecosounds.AED";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Gets the initial (default) settings for the analysis.
        /// </summary>
        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
        };

        /// <summary>
        ///     Gets the name to display for the analysis.
        /// </summary>
        public override string DisplayName => "AED";

        /// <summary>
        ///     Gets Identifier.
        /// </summary>
        public override string Identifier => EcosoundsAedIdentifier;

        public override Status Status => Status.Maintained;

        public static Tuple<EventCommon[], AudioRecording, BaseSonogram> Detect(
            FileInfo audioFile,
            AedConfiguration aedConfiguration,
            TimeSpan segmentStartOffset)
        {
            if (aedConfiguration.NoiseReductionType != NoiseReductionType.None && aedConfiguration.NoiseReductionParameter == null)
            {
                throw new ArgumentException("A noise production parameter should be supplied if not using AED noise removal", "noiseReductionParameter");
            }

            var recording = new AudioRecording(audioFile);
            var segmentDuration = recording.Duration;
            if (recording.SampleRate != aedConfiguration.ResampleRate)
            {
                throw new ArgumentException(
                    "Sample rate of recording ({0}) does not match the desired sample rate ({1})".Format2(
                        recording.SampleRate,
                        aedConfiguration.ResampleRate));
            }

            var config = new SonogramConfig
            {
                NoiseReductionType = aedConfiguration.NoiseReductionType,
                NoiseReductionParameter = aedConfiguration.NoiseReductionParameter,
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, recording.WavReader);

            var events = CallAed(sonogram, aedConfiguration, segmentStartOffset, segmentDuration);
            Log.Debug("AED # events: " + events.Length);
            return Tuple.Create(events, recording, sonogram);
        }

        public static EventCommon[] CallAed(BaseSonogram sonogram, AedConfiguration aedConfiguration, TimeSpan segmentStartOffset, TimeSpan segmentDuration)
        {
            Log.Info("AED start");

            var aedOptions = new AedOptions(sonogram.Configuration.NyquistFreq)
            {
                IntensityThreshold = aedConfiguration.IntensityThreshold,
                SmallAreaThreshold = aedConfiguration.SmallAreaThreshold,
                DoNoiseRemoval = aedConfiguration.NoiseReductionType == NoiseReductionType.None,
            };

            if (aedConfiguration.BandpassMinimum.HasValue && aedConfiguration.BandpassMaximum.HasValue)
            {
                var bandPassFilter =
                    Tuple.Create(
                        (double)aedConfiguration.BandpassMinimum.Value,
                        (double)aedConfiguration.BandpassMaximum.Value);
                aedOptions.BandPassFilter = new FSharpOption<Tuple<double, double>>(bandPassFilter);
            }

            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(aedOptions, sonogram.Data);
            Log.Info("AED finished");

            var unitConverters = new UnitConverters(
                segmentStartOffset.TotalSeconds,
                sonogram.SampleRate,
                sonogram.Configuration.WindowSize,
                sonogram.Configuration.WindowOverlap);

            var events = oblongs.Select(
                o =>
                {
                    var blob = new BlobEvent()
                    {
                        SegmentDurationSeconds = segmentDuration.TotalSeconds,
                        Name = "AED Event",
                    };

                    unitConverters.SetBounds(blob, o);

                    if (aedConfiguration.IncludeHitElementsInOutput)
                    {
                        o.HitElements
                            .Select(p => unitConverters.ConvertPointToSpectralPoint(p, 1.0))
                            .ForEach(sp => blob.Points.Add(sp));
                    }

                    return blob;
                });

            return events.ToArray();
        }

        public static Image DrawSonogram(BaseSonogram sonogram, IEnumerable<EventCommon> events)
        {
            var image = new Image_MultiTrack(sonogram.GetImage(false, true, doMelScale: false));

            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

            ////image.AddTrack(ImageTrack.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

            //############################################################################################ TODO TODO
            //convert blob events to acoustic events for drawing purposes
            var aeEvents = new List<AcousticEvent>();
            foreach (var be in events)
            {
                aeEvents.Add(EventConverters.ConvertSpectralEventToAcousticEvent((SpectralEvent)be));
            }

            image.AddEvents(
                aeEvents,
                sonogram.NyquistFrequency,
                sonogram.Configuration.FreqBinCount,
                sonogram.FramesPerSecond);

            return image.GetImage();
        }

        public static void Execute(Arguments arguments)
        {
            MainEntry.WarnIfDeveloperEntryUsed();
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# Running acoustic event detection.");
            LoggedConsole.WriteLine(date);

            FileInfo recodingFile = arguments.Source;
            var recodingBaseName = recodingFile.BaseName();
            DirectoryInfo outputDir = arguments.Output.Combine(EcosoundsAedIdentifier);
            outputDir.Create();

            Log.Info("# Output folder =" + outputDir);
            Log.Info("# Recording file: " + recodingFile.Name);

            // READ PARAMETER VALUES FROM INI FILE
            AedConfiguration configruation = ConfigFile.Deserialize<AedConfiguration>(arguments.Config);
            var aedConfig = GetAedParametersFromConfigFileOrDefaults(configruation);
            var results = Detect(recodingFile, aedConfig, TimeSpan.Zero);

            // print image
            // save image of sonograms
            var outputImagePath = outputDir.CombineFile(recodingBaseName + ".Sonogram.png");
            Image image = DrawSonogram(results.Item3, results.Item1);
            image.Save(outputImagePath.FullName);
            Log.Info("Image saved to: " + outputImagePath.FullName);

            // output csv
            var outputCsvPath = outputDir.CombineFile(recodingBaseName + ".Events.csv");
            WriteEventsFileStatic(outputCsvPath, results.Item1);
            Log.Info("CSV file saved to: " + outputCsvPath.FullName);

            Log.Success("Finished");
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            FileInfo audioFile = segmentSettings.SegmentAudioFile;

            var aedConfig = GetAedParametersFromConfigFileOrDefaults(analysisSettings.Configuration);

            var results = Detect(audioFile, aedConfig, segmentSettings.SegmentStartOffset);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, results.Item2.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.Events = results.Item1;
            BaseSonogram sonogram = results.Item3;

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
                Image image = DrawSonogram(sonogram, results.Item1);
                image.Save(segmentSettings.SegmentImageFile.FullName);
                analysisResults.ImageFile = segmentSettings.SegmentImageFile;
            }

            return analysisResults;
        }

        public static AedConfiguration GetAedParametersFromConfigFileOrDefaults(Config configuration)
        {
            if (!configuration.TryGetEnum(AnalysisKeys.NoiseDoReduction, out NoiseReductionType noiseReduction))
            {
                noiseReduction = NoiseReductionType.None;
                Log.Warn("Noise reduction disabled, default AED noise removal used - this indicates a bad config file");
            }

            return new AedConfiguration()
            {
                IntensityThreshold = configuration.GetDouble(nameof(AedConfiguration.IntensityThreshold)),
                SmallAreaThreshold = configuration.GetInt(nameof(AedConfiguration.SmallAreaThreshold)),
                BandpassMinimum = configuration.GetIntOrNull(nameof(AedConfiguration.BandpassMinimum)),
                BandpassMaximum = configuration.GetIntOrNull(nameof(AedConfiguration.BandpassMaximum)),
                IncludeHitElementsInOutput = configuration.GetBoolOrNull(nameof(AedConfiguration.IncludeHitElementsInOutput)) ?? false,
                AedEventColor = configuration[nameof(AedConfiguration.AedEventColor)].ParseAsColor(),
                AedHitColor = configuration[nameof(AedConfiguration.AedHitColor)].ParseAsColor(),
                ResampleRate = configuration.GetInt(nameof(AedConfiguration.ResampleRate)),
                NoiseReductionType = noiseReduction,
                NoiseReductionParameter = configuration.GetDouble(AnalysisKeys.NoiseBgThreshold),
            };
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // noop
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            WriteEventsFileStatic(destination, results);
        }

        public static void WriteEventsFileStatic(FileInfo destination, IEnumerable<EventBase> results)
        {
            Csv.WriteToCsv(destination, results);
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<EventIndex>());
        }
    }
}