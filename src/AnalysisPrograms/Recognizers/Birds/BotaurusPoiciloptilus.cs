// <copyright file="BotaurusPoiciloptilus.cs" company="QutEcoacoustics">
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
    /// A recognizer for the Australasian Bittern, Botaurus poiciloptilus, https://en.wikipedia.org/wiki/Australasian_bittern.
    /// The Australasian bittern, also known as the brown bittern or matuku hūrepo, is a large bird in the heron family Ardeidae.
    /// A secretive bird with a distinctive, low frequency booming call, it is more often heard than seen.
    /// Australasian bitterns are endangered in both Australia and New Zealand.
    /// </summary>
    internal class BotaurusPoiciloptilus : RecognizerBase
    {
        private static readonly ILog BitternLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "BotaurusPoiciloptilus";

        public override string Description => "[ALPHA] Detects acoustic events for the Australasian Bittern.";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(BotaurusPoiciloptilusConfig).TypeHandle);
            var config = ConfigFile.Deserialize<BotaurusPoiciloptilusConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is OnebinTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is OnebinTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException($"Autralasian Bittern expects one and only one {nameof(OnebinTrackParameters)} algorithm.", file);
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
            //class BotaurusPoiciloptilusConfig is define at bottom of this file.
            var genericConfig = (BotaurusPoiciloptilusConfig)config;
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

            if (combinedResults.NewEvents.Count == 0)
            {
                BitternLog.Debug($"Return zero events.");
                return combinedResults;
            }

            var newEvents = combinedResults.NewEvents;

            // NOTE: If the dB threshold is set low, may get lots of little events.
            if (genericConfig.CombinePossibleSyllableSequence)
            {
                // Convert events to spectral events for combining of possible sequences.
                // Can also use this parameter to combine events that are in the upper or lower neighbourhood.
                // Such combinations will increase bandwidth of the event and this property can be used later to weed out unlikely events..
                var spectralEvents = newEvents.Cast<SpectralEvent>().ToList();
                var startDiff = genericConfig.SyllableStartDifference;
                var hertzDiff = genericConfig.SyllableHertzGap;
                newEvents = CompositeEvent.CombineProximalEvents(spectralEvents, TimeSpan.FromSeconds(startDiff), (int)hertzDiff);
                BitternLog.Debug($"Event count after combining proximals = {combinedResults.NewEvents.Count}");
            }

            // Get the Bittern syllable config.
            const string profileName = "BitternSyllable";
            var configuration = (BotaurusPoiciloptilusConfig)genericConfig;
            var whistleConfig = (OnebinTrackParameters)configuration.Profiles[profileName];

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
            var neighbourhoodDbThreshold = whistleConfig.DecibelThreshold.Value * 0.8333;

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

                BitternLog.Debug($"Event count after filtering on neighbourhood = {combinedResults.NewEvents.Count}");
            }

            if (combinedResults.NewEvents.Count == 0)
            {
                BitternLog.Debug($"Return zero events.");
                return combinedResults;
            }

            // 5: Filter on COMPONENT COUNT in Composite events.
            int maxComponentCount = 6;
            combinedResults.NewEvents = EventExtentions.FilterEventsOnCompositeContent(combinedResults.NewEvents, maxComponentCount);
            BitternLog.Debug($"Event count after filtering on component count = {combinedResults.NewEvents.Count}");

            // 6: Filter the events for duration in seconds
            var minimumEventDuration = 0.5;
            var maximumEventDuration = 2.0;
            if (genericConfig.CombinePossibleSyllableSequence)
            {
                minimumEventDuration = 2.0;
                maximumEventDuration = 10.0;
            }

            combinedResults.NewEvents = EventExtentions.FilterOnDuration(newEvents, minimumEventDuration, maximumEventDuration);
            BitternLog.Debug($"Event count after filtering on duration = {combinedResults.NewEvents.Count}");

            // 7: Filter the events for bandwidth in Hertz
            double average = 100;
            double sd = 15;
            double sigmaThreshold = 3.0;
            combinedResults.NewEvents = EventExtentions.FilterOnBandwidth(combinedResults.NewEvents, average, sd, sigmaThreshold);
            BitternLog.Debug($"Event count after filtering on bandwidth = {combinedResults.NewEvents.Count}");

            // Uncomment the next line when want to obtain the event frequency profiles.
            // WriteFrequencyProfiles(chirpEvents);

            //foreach (var ev in whistleEvents)
            //{
            //    // Calculate frequency profile score for event
            //    SetFrequencyProfileScore((WhistleEvent)ev);
            //}

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

        /// <inheritdoc cref="BotaurusPoiciloptilusConfig"/> />
        public class BotaurusPoiciloptilusConfig : GenericRecognizerConfig, INamedProfiles<object>
        {
            public bool CombinePossibleSyllableSequence { get; set; } = false;

            public double SyllableStartDifference { get; set; } = 0.5;

            public double SyllableHertzGap { get; set; } = 200;
        }
    }
}
