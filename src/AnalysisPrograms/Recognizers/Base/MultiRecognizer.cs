// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MultiRecognizer type.
//
// The action type is EventRecognizer.
//     i.e. the first argument on the command line to call this analysis, should be "EventRecognizer".
// The DEV arguments for this action are in the class RecognizerEntry.cs, in the Dev method.
// In particular, look under the comment: // The MULTI-RECOGNISER
//     string configPath    = path\AudioAnalysis\AnalysisConfigFiles\Ecosounds.MultiRecognizer.yml";
// eg: string configPath    = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Ecosounds.MultiRecognizer.yml";
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.ImageSharp;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.AnalyseLongRecordings;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class MultiRecognizer : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Description => "[BETA] A method to run multiple event/species recognisers, depending on entries in config file.";

        public override string Author => "QUT";

        public override string SpeciesName => "MultiRecognizer";

        public override string CommonName => string.Empty;

        public override Status Status => Status.Unmaintained;

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            var config = ConfigFile.Deserialize<MultiRecognizerConfig>(file);
            return config;
        }

        public override RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            // this is a multi recognizer - it does no actual analysis itself
            MultiRecognizerConfig multiRecognizerConfig = (MultiRecognizerConfig)configuration;

            // make a standard spectrogram in which to render acoustic events and to append score tracks
            // currently using Hamming window. Worth trying Hanning Window
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.1,
                WindowSize = 512,
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            var scoreTracks = new List<Image<Rgb24>>();
            var plots = new List<Plot>();
            var events = new List<AcousticEvent>();
            var newEvents = new List<EventCommon>();

            // Get list of ID names from config file
            // Loop through recognizers and accumulate the output
            foreach (var pair in multiRecognizerConfig.Analyses)
            {
                var output = DoCallRecognition(pair.Recognizer, pair.Configuration, segmentStartOffset, audioRecording, getSpectralIndexes, outputDirectory, imageWidth);

                if (output == null)
                {
                    Log.Warn($"Recognizer for {pair.Recognizer.DisplayName} returned a null output");
                }
                else
                {
                    // concatenate results
                    if (output.ScoreTrack != null)
                    {
                        scoreTracks.Add(output.ScoreTrack);
                    }

                    if (output.Events != null)
                    {
                        events.AddRange(output.Events);
                    }

                    newEvents.AddRange(output.NewEvents);

                    // rescale scale of plots
                    output.Plots.ForEach(p => p.ScaleDataArray(sonogram.FrameCount));

                    plots.AddRange(output.Plots);
                }
            }

            var scoreTrackImage = ImageTools.CombineImagesVertically(scoreTracks);

            return new RecognizerResults()
            {
                Events = events,
                NewEvents = newEvents,
                ScoreTrack = scoreTrackImage,
                Sonogram = sonogram,
                Plots = plots,
                Hits = null,
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
            // no-op
        }

        private static RecognizerResults DoCallRecognition(IEventRecognizer recognizer, RecognizerConfig configuration, TimeSpan segmentStartOffset, AudioRecording recording, Lazy<IndexCalculateResult[]> indices, DirectoryInfo outputDirectory, int? imageWidth)
        {
            // TODO: adapt sample rate to required rate
            int? resampleRate = configuration.ResampleRate;
            if (resampleRate.HasValue && recording.WavReader.SampleRate != resampleRate.Value)
            {
                Log.Warn("Sample rate of provided file does does match");
            }

            // execute it
            Log.Info("MultiRecognizer: Executing single recognizer " + recognizer.DisplayName);
            RecognizerResults result = recognizer.Recognize(
                recording,
                configuration,
                segmentStartOffset,
                indices,
                outputDirectory,
                imageWidth);
            Log.Debug("MultiRecognizer: Completed single recognizer" + recognizer.DisplayName);

            var scoreTracks = result.Plots.Select(p => GenerateScoreTrackImage(recognizer.DisplayName, p?.data, imageWidth ?? result.Sonogram.FrameCount)).ToList();
            if (scoreTracks.Count != 0)
            {
                result.ScoreTrack = ImageTools.CombineImagesVertically(scoreTracks);
            }

            return result;
        }

        private static Image<Rgb24> GenerateScoreTrackImage(string name, double[] scores, int imageWidth)
        {
            Log.Info("MultiRecognizer.GenerateScoreTrackImage(): " + name);
            if (scores == null)
            {
                return null;
            }

            // reduce score array down to imageWidth;
            double[] scoreValues = new double[imageWidth];
            for (int i = 0; i < imageWidth; i++)
            {
                var index = (int)Math.Round(scores.Length / (double)imageWidth * i);
                scoreValues[i] = scores[index];
            }

            //Color[] color = { Color.Blue, Color.LightGreen, Color.Red, Color.Orange, Color.Purple };
            var stringFont = Drawing.Tahoma8;
            var brush = Color.Red;
            int trackHeight = 20;

            var trackImage = new Image<Rgb24>(imageWidth, trackHeight);
            trackImage.Mutate(g2 =>
            {
                g2.Clear(Color.LightGray);
                for (int x = 0; x < imageWidth; x++)
                {
                    int value = (int)Math.Round(scoreValues[x] * trackHeight);
                    for (int y = 1; y < value; y++)
                    {
                        if (y > trackHeight)
                        {
                            break;
                        }

                        trackImage[x, trackHeight - y] = Color.Black;
                    }
                }

                g2.DrawTextSafe(name, stringFont, brush, new PointF(1, 1));
                g2.DrawRectangle(new Pen(Color.Gray, 1), 0, 0, imageWidth - 1, trackHeight - 1);
            });
            return trackImage;
        }

        public class MultiRecognizerConfig : AnalyzerConfig
        {
            public MultiRecognizerConfig()
            {
                void OnLoaded(IConfig eventConfig)
                {
                    MultiRecognizerConfig config = (MultiRecognizerConfig)eventConfig;

                    if (config.AnalysisNames is null or { Length: 0 })
                    {
                        throw new ConfigFileException(
                            $"{ nameof(this.AnalysisNames)} cannot be null or empty. It should be a list with at least one config file in it.",
                            config.ConfigPath);
                    }

                    // load the other config files
                    this.Analyses =
                        config
                        .AnalysisNames
                        .Select(x => x.EndsWith(".yml") ? x : x + ".yml")
                        .Select(lookup =>
                        {
                            Log.Debug("Looking for config files for " + lookup);
                            var configurationFile = ConfigFile.Resolve(lookup, config.ConfigDirectory.ToDirectoryInfo());

                            // find an appropriate event recognizer
                            var recognizer = AnalyseLongRecording.FindAndCheckAnalyzer<IEventRecognizer>(null, configurationFile.Name);

                            // load up the standard config file for this species
                            var configuration = (RecognizerConfig)recognizer.ParseConfig(configurationFile);

                            return (recognizer, configuration);
                        }).ToArray();
                }

                this.Loaded += OnLoaded;
            }

            public string[] AnalysisNames { get; set; }

            /// <summary>
            /// Gets or sets the threshold for which to filter events.
            /// Defaults to 0.0 for the multi recogniser as we want the base recogniser's filters to be used.
            /// </summary>
            public override double EventThreshold { get; set; } = 0.0;

            internal (IEventRecognizer Recognizer, RecognizerConfig Configuration)[] Analyses { get; set; }
        }
    }

}