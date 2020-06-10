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
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is UpwardTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException("CisticolaExilis expects one and only one UpwardTrack algorithm.", file);
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

            if (combinedResults.NewEvents.Count == 0)
            {
                CisticolaLog.Debug($"Return zero events.");
                return combinedResults;
            }

            // 1: Filter the events for duration in seconds
            var minimumEventDuration = 0.1;
            var maximumEventDuration = 0.25;
            combinedResults.NewEvents = EventExtentions.FilterOnDuration(combinedResults.NewEvents, minimumEventDuration, maximumEventDuration);
            CisticolaLog.Debug($"Event count after filtering on duration = {combinedResults.NewEvents.Count}");

            // 2: Filter the events for bandwidth in Hertz
            double average = 600;
            double sd = 150;
            double sigmaThreshold = 3.0;
            combinedResults.NewEvents = EventExtentions.FilterOnBandwidth(combinedResults.NewEvents, average, sd, sigmaThreshold);
            CisticolaLog.Debug($"Event count after filtering on bandwidth = {combinedResults.NewEvents.Count}");

            // 3: Filter on COMPONENT COUNT in Composite events.
            //int maxComponentCount = 2;
            //combinedResults.NewEvents = EventExtentions.FilterEventsOnCompositeContent(combinedResults.NewEvents, maxComponentCount);
            //CisticolaLog.Debug($"Event count after filtering on component count = {combinedResults.NewEvents.Count}");

            //combinedResults.NewEvents = FilterEventsOnFrequencyProfile(combinedResults.NewEvents);

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
        }
    }
}
