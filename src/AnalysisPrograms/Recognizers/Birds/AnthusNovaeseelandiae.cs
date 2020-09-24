// <copyright file="AnthusNovaeseelandiae.cs" company="QutEcoacoustics">
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
    /// A recognizer for the Australasian pipit, https://en.wikipedia.org/wiki/Australasian_pipit .
    /// It is a fairly small slender passerine of open country in Australia, New Zealand and New Guinea.
    /// It is 16 to 19 cm long, and weighs about 40 grams.
    /// The plumage is pale brown above with dark streaks. The underparts are pale with streaks on the breast.
    /// There is a pale stripe over the eye and dark malar and moustachial stripes. The long tail has white outer-feathers and is often wagged up and down.
    /// The legs are long and pinkish-brown while the bill is slender and brownish.
    /// It has a sparrow-like chirruping call and a drawn-out tswee call. This recognizer detects the chirrup call.
    /// </summary>
    internal class AnthusNovaeseelandiae : RecognizerBase
    {
        private static readonly ILog PipitLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "AnthusNovaeseelandiae";

        public override string Description => "[ALPHA] Designed to detect chirrup calls of the Australian Pipit.";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(PipitConfig).TypeHandle);
            var config = ConfigFile.Deserialize<PipitConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is ForwardTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is UpwardTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException("AnthusNovaeseelandiae expects one and only one UpwardTrack algorithm.", file);
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
            //class PipitConfig is defined at bottom of this file.
            var genericConfig = (PipitConfig)config;
            var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = recognizer.Recognize(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            // ################### POST-PROCESSING of EVENTS ###################

            if (combinedResults.NewEvents.Count == 0)
            {
                PipitLog.Debug($"Return zero events.");
                return combinedResults;
            }

            // 1: Filter the events for duration in seconds
            var minimumEventDuration = 0.1;
            var maximumEventDuration = 0.4;
            combinedResults.NewEvents = EventFilters.FilterOnDuration(combinedResults.NewEvents, minimumEventDuration, maximumEventDuration);
            PipitLog.Debug($"Event count after filtering on duration = {combinedResults.NewEvents.Count}");

            // 2: Filter the events for bandwidth in Hertz
            double average = 3500;
            double sd = 600;
            double sigmaThreshold = 3.0;
            combinedResults.NewEvents = EventFilters.FilterOnBandwidth(combinedResults.NewEvents, average, sd, sigmaThreshold);
            PipitLog.Debug($"Event count after filtering on bandwidth = {combinedResults.NewEvents.Count}");

            combinedResults.NewEvents = FilterEventsOnFrequencyProfile(combinedResults.NewEvents);

            return combinedResults;
        }

        /// <summary>
        /// This method assumes that the only events of interest are composite events.
        /// </summary>
        /// <param name="events">THe current list of events.</param>
        /// <returns>A list of composite events.</returns>
        public static List<EventCommon> FilterEventsOnFrequencyProfile(List<EventCommon> events)
        {
            if (events.Count == 0)
            {
                return events;
            }

            // select only the composite events.
            //var compositeEvents = events.Select(x => (CompositeEvent)x).ToList();
            var (compositeEvents, others) = events.FilterForEventType<CompositeEvent, EventCommon>();

            if (compositeEvents == null || compositeEvents.Count == 0)
            {
                return events;
            }

            // get the composite track for each composite event.
            var returnEvents = new List<EventCommon>();
            foreach (var ev in compositeEvents)
            {
                var componentEvents = ev.ComponentEvents.Cast<WhipEvent>();
                var points = EventExtentions.GetCompositeTrack(componentEvents).ToArray();

                // Uncomment this line when want to see the composite track profile.
                //WriteFrequencyProfile(points);

                // For Pipit require minimum of four frames duration.
                var length = points.Length;
                if (length < 4)
                {
                    continue;
                }

                // Only select events having strong downward slope in spectrogram.
                var avFirstTwoEvents = (points[0].Hertz.Minimum + points[0].Hertz.Minimum) / 2;
                var avLastTwoEvents = (points[length - 1].Hertz.Minimum + points[length - 2].Hertz.Minimum) / 2;
                if (avFirstTwoEvents - avLastTwoEvents > 500)
                {
                    returnEvents.Add(ev);
                }
            }

            return returnEvents;
        }

        public static void WriteFrequencyProfile(ISpectralPoint[] points)
        {
            if (points != null)
            {
                var str = $"Track({points[0].Seconds.Minimum:F2}):";

                foreach (var point in points)
                {
                    str += $" {point.Hertz.Minimum},";
                }

                Console.WriteLine(str);
            }
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

        public class PipitConfig : GenericRecognizerConfig, INamedProfiles<object>
        {
        }
    }
}
