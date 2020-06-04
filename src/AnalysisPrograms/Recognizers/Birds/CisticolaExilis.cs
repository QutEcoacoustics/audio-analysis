// <copyright file="CisticolaExilis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using static AnalysisPrograms.Recognizers.GenericRecognizer;
    using Path = System.IO.Path;

    /// <summary>
    /// A recognizer for calls of the golden-headed cisticola (Cisticola exilis), https://en.wikipedia.org/wiki/Golden-headed_cisticola .
    /// It is a species of warbler in the family Cisticolidae, found in Australia and 13 Asian countries.
    /// Grows to 9–11.5 centimetres (3.5–4.5 in) long, it is usually brown and cream in colour.
    /// It produces a variety of calls distinct from other birds, which, according to the Sunshine Coast Council, range from a "teewip" to a "wheezz, whit-whit".
    /// It has a very large range and population, which is thought to be increasing. It is not a threatened species.
    /// </summary>
    internal class CisticolaExilis : RecognizerBase
    {
        private static readonly ILog CisticolaLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "CisticolaExilis";

        public override string Description => "[ALPHA] Detects acoustic events for the golden-headed cisticola.";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(CisticolaExilisConfig).TypeHandle);
            var config = ConfigFile.Deserialize<CisticolaExilisConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is ForwardTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is ForwardTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException("CisticolaExilis expects one and only one ForwardTrack algorithm.", file);
        }

        /// <summary>
        /// This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording">one minute of audio recording.</param>
        /// <param name="config">config file that contains parameters used by all profiles.</param>
        /// <param name="segmentStartOffset">when recording starts.</param>
        /// <param name="getSpectralIndexes">not sure what this is.</param>
        /// <param name="outputDirectory">where the recognizer results can be found.</param>
        /// <param name="imageWidth"> assuming ????.</param>
        /// <returns>recognizer results.</returns>
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config config,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            //class CisticolaExilisConfig is defined at bottom of this file.
            var genericConfig = (CisticolaExilisConfig)config;
            var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = recognizer.Recognize(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            // ################### POST-PROCESSING of EVENTS ###################
            // Following two commented lines are different ways of casting lists.
            //var newEvents = spectralEvents.Cast<EventCommon>().ToList();
            //var spectralEvents = events.Select(x => (SpectralEvent)x).ToList();

            // 1: Pull out the chirp events and calculate their frequency profiles.
            var (chirpEvents, others) = combinedResults.NewEvents.FilterForEventType<ChirpEvent, EventCommon>();

            if (combinedResults.NewEvents.Count == 0)
            {
                CisticolaLog.Debug($"Return zero events.");
                return combinedResults;
            }

            // 2: Combine overlapping events. If the dB threshold is set low, may get lots of little events.
            combinedResults.NewEvents = CompositeEvent.CombineOverlappingEvents(chirpEvents.Cast<EventCommon>().ToList());
            CisticolaLog.Debug($"Event count after combining overlaps = {combinedResults.NewEvents.Count}");

            // 3: Combine proximal events. If the dB threshold is set low, may get lots of little events.
            if (genericConfig.CombinePossibleSyllableSequence)
            {
                // Convert events to spectral events for combining of possible sequences.
                // Can also use this parameter to combine events that are in the upper or lower neighbourhood.
                // Such combinations will increase bandwidth of the event and this property can be used later to weed out unlikely events.
                var spectralEvents1 = combinedResults.NewEvents.Cast<SpectralEvent>().ToList();
                var startDiff = genericConfig.SyllableStartDifference;
                var hertzDiff = genericConfig.SyllableHertzGap;
                combinedResults.NewEvents = CompositeEvent.CombineProximalEvents(spectralEvents1, TimeSpan.FromSeconds(startDiff), (int)hertzDiff);
                CisticolaLog.Debug($"Event count after combining proximals = {combinedResults.NewEvents.Count}");
            }

            // Get the CisticolaSyllable config.
            const string profileName = "CisticolaSyllable";
            var configuration = (CisticolaExilisConfig)genericConfig;
            var chirpConfig = (ForwardTrackParameters)configuration.Profiles[profileName];

            // 4: Filter events on the amount of acoustic activity in their upper and lower neighbourhoods - their buffer zone.
            //    The idea is that an unambiguous event should have some acoustic space above and below.
            //    The filter requires that the average acoustic activity in each frame and bin of the upper and lower buffer zones should not exceed the user specified decibel threshold.
            //    The bandwidth of these two neighbourhoods is determined by the following parameters.
            //    ########## These parameters could be specified by user in config.yml file.
            var upperHertzBuffer = 400;
            var lowerHertzBuffer = 150;

            // The decibel threshold is currently set 5/6ths of the user specified threshold.
            // THIS IS TO BE WATCHED. IT MAY PROVE TO BE INAPPROPRIATE TO HARD-CODE.
            // Want the activity in buffer zones to be "somewhat" less than the user-defined threshold.
            var neighbourhoodDbThreshold = chirpConfig.DecibelThreshold.Value * 0.8333;

            if (upperHertzBuffer > 0 || lowerHertzBuffer > 0)
            {
                var spectralEvents2 = combinedResults.NewEvents.Cast<SpectralEvent>().ToList();
                combinedResults.NewEvents = EventExtentions.FilterEventsOnNeighbourhood(
                    spectralEvents2,
                    combinedResults.Sonogram,
                    lowerHertzBuffer,
                    upperHertzBuffer,
                    segmentStartOffset,
                    neighbourhoodDbThreshold);

                CisticolaLog.Debug($"Event count after filtering on neighbourhood = {combinedResults.NewEvents.Count}");
            }

            if (combinedResults.NewEvents.Count == 0)
            {
                CisticolaLog.Debug($"Return zero events.");
                return combinedResults;
            }

            // 5: Filter on COMPONENT COUNT in Composite events.
            int maxComponentCount = 2;
            combinedResults.NewEvents = EventExtentions.FilterEventsOnCompositeContent(combinedResults.NewEvents, maxComponentCount);
            CisticolaLog.Debug($"Event count after filtering on component count = {combinedResults.NewEvents.Count}");

            // 6: Filter the events for duration in seconds
            var minimumEventDuration = chirpConfig.MinDuration;
            var maximumEventDuration = chirpConfig.MaxDuration;
            if (genericConfig.CombinePossibleSyllableSequence)
            {
                minimumEventDuration *= 2.0;
                maximumEventDuration *= 1.5;
            }

            combinedResults.NewEvents = EventExtentions.FilterOnDuration(combinedResults.NewEvents, minimumEventDuration.Value, maximumEventDuration.Value);
            CisticolaLog.Debug($"Event count after filtering on duration = {combinedResults.NewEvents.Count}");

            // 7: Filter the events for bandwidth in Hertz
            double average = 280;
            double sd = 40;
            double sigmaThreshold = 3.0;
            combinedResults.NewEvents = EventExtentions.FilterOnBandwidth(combinedResults.NewEvents, average, sd, sigmaThreshold);
            CisticolaLog.Debug($"Event count after filtering on bandwidth = {combinedResults.NewEvents.Count}");

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected" in UserName.SpeciesName.yml config file.
            //GenericRecognizer.SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);
            return combinedResults;
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

        /// <inheritdoc cref="CisticolaExilisConfig"/> />
        public class CisticolaExilisConfig : GenericRecognizerConfig, INamedProfiles<object>
        {
            public bool CombinePossibleSyllableSequence { get; set; } = false;

            public double SyllableStartDifference { get; set; } = 0.5;

            public double SyllableHertzGap { get; set; } = 200;
        }
    }
}
