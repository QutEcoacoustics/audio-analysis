// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericRecognizers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>


namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This class calls algorithms for generic syllable/component types.
    /// </summary>
    public class GenericRecognizer : RecognizerBase
    {
        public class GenericRecognizerConfig : RecognizerConfig, INamedProfiles<object>
        {
            public Dictionary<string, object> Profiles { get; set; }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Ecosounds";

        public override string SpeciesName => "GenericRecognizer";

        public override string Description => "[ALPHA] Detects generic acoustic events";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<GenericRecognizerConfig>(file);
        }

        /// <inheritdoc/>
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config genericConfig,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            var configuration = (GenericRecognizerConfig)genericConfig;

            if (configuration.Profiles.NotNull() && configuration.Profiles.Count == 0)
            {
                throw new ConfigFileException(
                    "The generic recognizer needs at least one profile set. 0 were found.");
            }

            int count = configuration.Profiles.Count;
            var message = $"Found {count} analysis profile(s): " + configuration.Profiles.Keys.Join(", ");
            Log.Info(message);

            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // Now process each of the profiles
            foreach (var (profileName, profileConfig) in configuration.Profiles)
            {
                Log.Info("Processing profile: " + profileName);

                List<AcousticEvent> acousticEvents;
                var plots = new List<Plot>();
                SpectrogramStandard sonogram;

                // sanity check the algorithm
                string algorithmName;
                switch (profileConfig)
                {
                    case BlobParameters _:
                        algorithmName = "Blob";
                        break;
                    case OscillationParameters _:
                        algorithmName = "Oscillation";
                        break;
                    case WhistleParameters _:
                        throw new NotImplementedException("The whistle algorithm has not been implemented yet");
                        break;
                    case Aed.AedConfiguration _:
                        algorithmName = "AED";
                        break;
                    default:
                        var allowedAlgorithms =
                            $"{nameof(BlobParameters)}, {nameof(OscillationParameters)}, {nameof(WhistleParameters)}, {nameof(Aed.AedConfiguration)}";
                        throw new ConfigFileException($"The algorithm type in profile {profileName} is not recognized. It must be one of {allowedAlgorithms}");
                }

                Log.Debug($"Using the {algorithmName} algorithm... ");
                if (profileConfig is CommonParameters parameters)
                {
                    if (profileConfig is BlobParameters || profileConfig is OscillationParameters)
                    {
                        sonogram = new SpectrogramStandard(
                            this.ParametersToSonogramConfig(parameters),
                            audioRecording.WavReader);

                        var decibelArray = SNR.CalculateFreqBandAvIntensity(
                            sonogram.Data,
                            parameters.MinHertz,
                            parameters.MaxHertz,
                            sonogram.NyquistFrequency);

                        // prepare plots
                        // AT: magic number `3`?
                        double intensityNormalizationMax = 3 * parameters.DecibelThreshold;
                        var eventThreshold = parameters.DecibelThreshold / intensityNormalizationMax;
                        var normalisedIntensityArray =
                            DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalizationMax);
                        var plot = new Plot($"{profileName} ({algorithmName}:Intensity)", normalisedIntensityArray,
                            eventThreshold);
                        plots.Add(plot);

                        if (profileConfig is OscillationParameters op)
                        {
                            Oscillations2012.Execute(
                                sonogram,
                                op.MinHertz,
                                op.MaxHertz,

                                //op.DecibelThreshold,
                                op.DctDuration,
                                op.MinOscillationFrequency,
                                op.MaxOscillationFrequency,
                                op.DctThreshold,
                                op.EventThreshold,
                                op.MinDuration,
                                op.MaxDuration,
                                out var scores,
                                out acousticEvents,
                                out var hits,
                                segmentStartOffset);

                            plots.Add(new Plot(
                                $"{profileName} ({algorithmName}:OscillationScore)",
                                scores,
                                op.EventThreshold));
                        }
                        else if (profileConfig is BlobParameters bp)
                        {
                            // iii: CONVERT decibel SCORES TO ACOUSTIC EVENTS
                            acousticEvents = AcousticEvent.GetEventsAroundMaxima(
                                decibelArray,
                                segmentStartOffset,
                                bp.MinHertz,
                                bp.MaxHertz,
                                bp.DecibelThreshold,
                                bp.MinDuration.Seconds(),
                                bp.MaxDuration.Seconds(),
                                sonogram.FramesPerSecond,
                                sonogram.FBinWidth);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    // AT: disabled - the current method definition does not make sense
                    //acousticEvents = RecognizerTools.FilterEventsForSpectralProfile(acousticEvents, sonogram);

                    //iV add additional info to the acoustic events
                    acousticEvents.ForEach(ae =>
                    {
                        ae.FileName = audioRecording.BaseName;
                        ae.SpeciesName = parameters.SpeciesName;
                        ae.Name = parameters.ComponentName;
                        ae.Profile = profileName;
                        ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                        ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                        ae.SetTimeAndFreqScales(sonogram.FrameStep, sonogram.FrameDuration, sonogram.FBinWidth);
                    });
                }
                else if (profileConfig is Aed.AedConfiguration ac)
                {
                    var config = new SonogramConfig
                    {
                        NoiseReductionType = ac.NoiseReductionType,
                        NoiseReductionParameter = ac.NoiseReductionParameter,
                    };
                    sonogram = new SpectrogramStandard(config, audioRecording.WavReader);

                    acousticEvents = Aed.CallAed(sonogram, ac, segmentStartOffset, audioRecording.Duration).ToList();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                // combine the results i.e. add the events list of call events.
                allResults.Events.AddRange(acousticEvents);
                allResults.Plots.AddRange(plots);

                // effectively keeps only the *last* sonogram produced
                allResults.Sonogram = sonogram;
                Log.Debug($"{profileName} event count = {acousticEvents.Count}");
            }

            return allResults;
        }

        /*
        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }
        */

        private SonogramConfig ParametersToSonogramConfig(CommonParameters common)
        {
            const int DefaultWindow = 512;

            return new SonogramConfig()
            {
                WindowSize = common.FrameSize ?? DefaultWindow,
                WindowOverlap = common.FrameStep / (common.FrameSize ?? DefaultWindow) ?? 0.0,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = common.BgNoiseThreshold ?? 0.0,
            };
        }
    }

    public class CommonParameters
    {
        public string ComponentName { get; set; }

        public string SpeciesName { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window size, i.e. number of signal samples. Must be power of 2. Typically 512.
        /// </summary>
        public int? FrameSize { get; set; }

        /// <summary>
        /// Gets or sets the frame or Window step i.e. before start of next frame.
        /// The overlap can be any number of samples but less than the frame length/size.
        /// </summary>
        public int? FrameStep { get; set; }

        public double? BgNoiseThreshold { get; set; }

        /// <summary>
        /// Gets or sets the bottom bound of the rectangle. Units are Hertz.
        /// </summary>
        public int MinHertz { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of the rectangle. Units are Hertz.
        /// </summary>
        public int MaxHertz { get; set; }

        /// <summary>
        /// Gets or sets the buffer (bandwidth of silence) below the component rectangle. Units are Hertz.
        /// </summary>
        public int BottomHertzBuffer { get; set; }

        /// <summary>
        /// Gets or sets the buffer (bandwidth of silence) above the component rectangle. Units are Hertz.
        /// Quite often this will be set to <value>null</value> which indicates as upper bounds variable,
        /// depending on distance of the source.
        /// </summary>
        public int? TopHertzBuffer { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed duration of the component. Units are seconds.
        /// </summary>
        public double MinDuration { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the maximum allowed duration of the component. Units are seconds.
        /// </summary>
        public double MaxDuration { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the threshold of "loudness" of a component. Units are decibels.
        /// </summary>
        public double DecibelThreshold { get; set; } = 6;
    }

    public class DctParameters : CommonParameters
    {

        /// <summary>
        /// Gets or sets the time duration (in seconds) of a Discrete Cosine Transform.
        /// </summary>
        public double DctDuration { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the minimum acceptable value of a DCT coefficient.
        /// </summary>
        public double DctThreshold { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the minimum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min & above the max threshold.
        /// </summary>
        public int MinOscillationFrequency { get; set; } = 6;

        /// <summary>
        /// Gets or sets the maximum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min & above the max threshold.
        /// </summary>
        public int MaxOscillationFrequency { get; set; } = 4;

        /// <summary>
        /// Gets or sets the Event threshold - use this to determine FP / FN trade-off for events.
        /// </summary>
        public double EventThreshold { get; set; } = 0.3;
    }

    /// <summary>
    /// 
    /// The following parameters worked well on a ten minute recording containing 14-16 calls.
    /// Note: if you lower the dB threshold, you need to increase maxDurationSeconds
    /// </summary>
    public class BlobParameters : CommonParameters
    {
        public BlobParameters()
        {
            this.MinHertz = 800;
            this.MaxHertz = 8000;
        }
    }

    public class OscillationParameters : DctParameters
    {
    }

    public class WhistleParameters : DctParameters
    {
    }
}
