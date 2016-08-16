// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AED.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Acoustic Event Detection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Shared.Extensions;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using Microsoft.FSharp.Core;

    using QutSensors.AudioAnalysis.AED;

    using TowseyLibrary;

    using YamlDotNet.Dynamic;

    /// <summary>
    ///     Acoustic Event Detection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.", Scope = "Class")]
    public class Aed : AbstractStrongAnalyser
    {
        /// <summary>
        /// The arguments.
        /// </summary>
        public class Arguments : SourceConfigOutputDirArguments
        {
        }

        public class AedConfiguration
        {
            public AedConfiguration()
            {
                this.AedEventColor = Color.Red;
                this.AedHitColor = Color.FromArgb(128, this.AedEventColor);
                this.NoiseReductionType = NoiseReductionType.NONE;
            }

            public double IntensityThreshold { get; set; } 

            public int SmallAreaThreshold { get; set; }

            public int? BandpassMinimum { get; set; }

            public int? BandpassMaximum { get; set; }

            public NoiseReductionType NoiseReductionType { get; set; } 

            public double NoiseReductionParameter { get; set; } 

            public int ResampleRate { get; set; }

            public Color AedEventColor { get; set; }

            public Color AedHitColor { get; set; }

            public bool IncludeHitElementsInOutput { get; set; }
        }

        #region Constants

        /// <summary>
        /// The ecosounds aed identifier.
        /// </summary>
        private const string EcosoundsAedIdentifier = "Ecosounds.AED";

        #endregion

        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the initial (default) settings for the analysis.
        /// </summary>
        public override AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                           {
                               SegmentMaxDuration = TimeSpan.FromMinutes(1), 
                               SegmentMinDuration = TimeSpan.FromSeconds(20), 
                               SegmentMediaType = MediaTypes.MediaTypeWav, 
                               SegmentOverlapDuration = TimeSpan.Zero, 
                           };
            }
        }

        /// <summary>
        ///     Gets the name to display for the analysis.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "AED";
            }
        }

        /// <summary>
        ///     Gets Identifier.
        /// </summary>
        public override string Identifier
        {
            get
            {
                return EcosoundsAedIdentifier;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static Tuple<AcousticEvent[], AudioRecording, BaseSonogram> Detect(
            FileInfo audioFile,
            AedConfiguration aedConfiguration,
            TimeSpan segmentStartOffset)
        {
            if (aedConfiguration.NoiseReductionType != NoiseReductionType.NONE && aedConfiguration.NoiseReductionParameter == null)
            {
                throw new ArgumentException("A noise production parameter should be supplied if not using AED noise removal", "noiseReductionParameter");
            }

            var recording = new AudioRecording(audioFile);
            var segmentDuration = recording.Duration();
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
                                  NoiseReductionParameter = aedConfiguration.NoiseReductionParameter
                              };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, recording.WavReader);

            AcousticEvent[] events = CallAed(sonogram, aedConfiguration, segmentStartOffset, segmentDuration);
            TowseyLibrary.Log.WriteIfVerbose("AED # events: " + events.Length);
            return Tuple.Create(events, recording, sonogram);
        }


        public static AcousticEvent[] CallAed(BaseSonogram sonogram, AedConfiguration aedConfiguration, TimeSpan segmentStartOffset, TimeSpan segmentDuration)
        {
            Log.Info("AED start");
            var aedOptions = new AedOptions(sonogram.Configuration.NyquistFreq)
                                 {
                                     IntensityThreshold = aedConfiguration.IntensityThreshold,
                                     SmallAreaThreshold = aedConfiguration.SmallAreaThreshold,
                                     DoNoiseRemoval = aedConfiguration.NoiseReductionType == NoiseReductionType.NONE
                                 };

            if (aedConfiguration.BandpassMinimum.HasValue && aedConfiguration.BandpassMaximum.HasValue)
            {
                var bandPassFilter =
                    Tuple.Create(
                        (double)aedConfiguration.BandpassMinimum.Value,
                        (double)aedConfiguration.BandpassMaximum.Value);
                aedOptions.BandPassFilter = bandPassFilter.ToOption();
            }
               
            IEnumerable<Oblong> oblongs = AcousticEventDetection.detectEvents(aedOptions,   sonogram.Data);
            Log.Info("AED finished");

            var events = oblongs.Select(
                o =>
                {
                    if (!aedConfiguration.IncludeHitElementsInOutput)
                    {
                        o.HitElements = null;
                    }

                    return new AcousticEvent(
                        o,
                        sonogram.NyquistFrequency,
                        sonogram.Configuration.FreqBinCount,
                        sonogram.FrameDuration,
                        sonogram.FrameStep,
                        sonogram.FrameCount)
                    {
                        SegmentStartOffset = segmentStartOffset,
                        BorderColour = aedConfiguration.AedEventColor,
                        HitColour = aedConfiguration.AedHitColor,
                        SegmentDuration = segmentDuration
                    };
                }).ToArray();
            return events;
        }



        public static Arguments Dev(object obj)
        {
            throw new NotImplementedException();
        }

        public static Image DrawSonogram(BaseSonogram sonogram, IEnumerable<AcousticEvent> events)
        {
            var image = new Image_MultiTrack(sonogram.GetImage(false, true));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

            ////image.AddTrack(Image_Track.GetWavEnvelopeTrack(sonogram, image.sonogramImage.Width));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            image.AddEvents(
                events, 
                sonogram.NyquistFrequency, 
                sonogram.Configuration.FreqBinCount, 
                sonogram.FramesPerSecond);

            return image.GetImage();
        }


        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev(arguments);
            }

            TowseyLibrary.Log.Verbosity = 1;
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine("# Running acoustic event detection.");
            LoggedConsole.WriteLine(date);

            FileInfo recodingFile = arguments.Source;
            var recodingBaseName = Path.GetFileNameWithoutExtension(arguments.Source.Name);
            DirectoryInfo outputDir = arguments.Output.Combine(EcosoundsAedIdentifier);
            outputDir.Create();


            Log.Info("# Output folder =" + outputDir);
            Log.Info("# Recording file: " + recodingFile.Name);

            // READ PARAMETER VALUES FROM INI FILE
            DynamicYaml configruation = Yaml.Deserialise(arguments.Config);
            var aedConfig = GetAedParametersFromConfigFileOrDefaults(configruation);
            var results = Detect(recodingFile, aedConfig, TimeSpan.Zero);

            // print image
            // save image of sonograms
            var outputImagePath = outputDir.CombineFile(recodingBaseName + ".Sonogram.png");
            Image image = DrawSonogram(results.Item3, results.Item1);
            image.Save(outputImagePath.FullName, ImageFormat.Png);
            Log.Info("Image saved to: " + outputImagePath.FullName);


            // output csv
            var outputCsvPath = outputDir.CombineFile(recodingBaseName + ".Events.csv");
            WriteEventsFileStatic(outputCsvPath, results.Item1);
            Log.Info("CSV file saved to: " + outputCsvPath.FullName);

            TowseyLibrary.Log.WriteLine("Finished");
        }


        public override AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            FileInfo audioFile = analysisSettings.AudioFile;

            var aedConfig = GetAedParametersFromConfigFileOrDefaults(analysisSettings.Configuration);

            var results = Detect(audioFile, aedConfig, analysisSettings.SegmentStartOffset.Value);

            var analysisResults = new AnalysisResult2(analysisSettings, results.Item2.Duration());
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.Events = results.Item1;
            BaseSonogram sonogram = results.Item3;

            if (analysisSettings.EventsFile != null)
            {
                this.WriteEventsFile(analysisSettings.EventsFile, analysisResults.Events);
                analysisResults.EventsFile = analysisSettings.EventsFile;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                var unitTime = TimeSpan.FromMinutes(1.0);
                analysisResults.SummaryIndices = this.ConvertEventsToSummaryIndices(analysisResults.Events, unitTime, analysisResults.SegmentAudioDuration, 0);

                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
            }


            // save image of sonograms
            if (analysisSettings.ImageFile != null)
            {
                Image image = DrawSonogram(sonogram, results.Item1);
                image.Save(analysisSettings.ImageFile.FullName, ImageFormat.Png);
                analysisResults.ImageFile = analysisSettings.ImageFile;
            }

            return analysisResults;
        }

        public static AedConfiguration GetAedParametersFromConfigFileOrDefaults(dynamic configuration)
        {
            NoiseReductionType noiseReduction = NoiseReductionType.NONE;
            if ((bool)configuration[AnalysisKeys.NoiseDoReduction])
            {
                noiseReduction = (NoiseReductionType?)configuration[AnalysisKeys.NoiseReductionType] ?? noiseReduction;
            }
            else
            {
                Log.Warn("Noise reduction disabled, default AED noise removal used - this indicates a bad config file");
            }

            return new AedConfiguration()
                       {
                           IntensityThreshold = configuration.IntensityThreshold,
                           SmallAreaThreshold = configuration.SmallAreaThreshold,
                           BandpassMinimum = configuration.BandpassMinimum,
                           BandpassMaximum = configuration.BandpassMaximum,
                           IncludeHitElementsInOutput = (bool?)configuration.IncludeHitElementsInOutput ?? false,
                           AedEventColor = ((string)configuration.AedEventColor).ParseAsColor(),
                           AedHitColor = ((string)configuration.AedHitColor).ParseAsColor(),
                           ResampleRate = configuration[AnalysisKeys.ResampleRate],
                           NoiseReductionType = noiseReduction,
                           NoiseReductionParameter = configuration[AnalysisKeys.NoiseBgThreshold]
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
            Csv.WriteToCsv(destination, results);
        }

        #endregion
    }
}