// <copyright file="NinoxBoobook.cs" company="QutEcoacoustics">
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
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using static AnalysisPrograms.Recognizers.GenericRecognizer;
    using Path = System.IO.Path;

    /// <summary>
    /// A recognizer for the Australian Boobook Owl, /// https://en.wikipedia.org/wiki/Australian_boobook .
    /// Eight subspecies of the Australian boobook are recognized,
    /// with three further subspecies being reclassified as separate species in 2019 due to their distinctive calls and genetics.
    /// THis recognizer has been trained on good quality calls from the Gympie recordings obtained by Yvonne Phillips.
    /// The recognizer has also been run across several recordings of Boobook from NZ (recordings obtained from Stuart Parsons.
    /// The NZ Boobook calls were of poor quality (distant and echo) and were 200 Hertz higher and performance was not good.
    /// </summary>
    internal class NinoxBoobook : RecognizerBase
    {
        private static readonly ILog BoobookLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "NinoxBoobook";

        public override string Description => "[ALPHA] Detects acoustic events for the Australian Boobook owl.";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(NinoxBoobookConfig).TypeHandle);
            var config = ConfigFile.Deserialize<NinoxBoobookConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is ForwardTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is ForwardTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException("NinoxBoobook expects one and only one ForwardTrack algorithm.", file);
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
            //class NinoxBoobookConfig is defined at bottom of this file.
            var genericConfig = (NinoxBoobookConfig)config;
            var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = recognizer.Recognize(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            // DO POST-PROCESSING of EVENTS

            // Filter out the chirp events for possible combining.
            var (chirpEvents, others) = combinedResults.NewEvents.FilterForEventType<ChirpEvent, EventCommon>();

            // Uncomment the next line when want to obtain the event frequency profiles.
            // WriteFrequencyProfiles(chirpEvents);

            // Calculate frequency profile score for each event
            foreach (var ev in chirpEvents)
            {
                SetFrequencyProfileScore((ChirpEvent)ev);
            }

            // Combine overlapping events. If the dB threshold is set low, may get lots of little events.
            var newEvents = CompositeEvent.CombineOverlappingEvents(chirpEvents.Cast<EventCommon>().ToList());

            if (genericConfig.CombinePossibleSyllableSequence)
            {
                // convert events to spectral events for possible combining.
                var (spectralEvents, _) = combinedResults.NewEvents.FilterForEventType<SpectralEvent, EventCommon>();

                var startDiff = genericConfig.SyllableStartDifference;
                var hertzDiff = genericConfig.SyllableHertzGap;
                newEvents = CompositeEvent.CombineSimilarProximalEvents(spectralEvents, TimeSpan.FromSeconds(startDiff), (int)hertzDiff);
            }

            combinedResults.NewEvents = newEvents;

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected" in <Towsey.PteropusSpecies.yml> config file.
            //GenericRecognizer.SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);
            return combinedResults;
        }

        /// <summary>
        /// The Boobook call syllable is shaped like an inverted "U". Its total duration is close to 0.15 seconds.
        /// The rising portion lasts for 0.06s, followed by a turning portion, 0.03s, followed by the decending portion of 0.06s.
        /// The constants for this method were obtained from the calls in a Gympie recording obtained by Yvonne Phillips.
        /// </summary>
        /// <param name="ev">An event containing at least one forward track i.e. a chirp.</param>
        public static void SetFrequencyProfileScore(ChirpEvent ev)
        {
            const double risingDuration = 0.06;
            const double gapDuration = 0.03;
            const double fallingDuration = 0.06;

            var track = ev.Tracks.First();
            var profile = track.GetTrackFrequencyProfile().ToArray();

            // get the first point
            var firstPoint = track.Points.First();
            var frameDuration = firstPoint.Seconds.Maximum - firstPoint.Seconds.Minimum;
            var risingFrameCount = (int)Math.Floor(risingDuration / frameDuration);
            var gapFrameCount = (int)Math.Floor(gapDuration / frameDuration);
            var fallingFrameCount = (int)Math.Floor(fallingDuration / frameDuration);

            var startSum = 0.0;
            if (profile.Length >= risingFrameCount)
            {
                for (var i = 0; i <= risingFrameCount; i++)
                {
                    startSum += profile[i];
                }
            }

            int startFrame = risingFrameCount + gapFrameCount;
            int endFrame = startFrame + fallingFrameCount;
            var endSum = 0.0;
            if (profile.Length >= endFrame)
            {
                for (var i = startFrame; i <= endFrame; i++)
                {
                    endSum += profile[i];
                }
            }

            // set score to 1.0 if the profile has inverted U shape.
            double score = 0.0;
            if (startSum > 0.0 && endSum < 0.0)
            {
                score = 1.0;
            }

            ev.FrequencyProfileScore = score;
        }

        /// <summary>
        /// WARNING - this method assumes that the rising and falling parts of a Boobook call syllable last for 5 frames.
        /// </summary>
        /// <param name="events">List of spectral events.</param>
        public static void WriteFrequencyProfiles(List<ChirpEvent> events)
        {
            /* Here are the frequency profiles of some events.
             * Note that the first five frames (0.057 seconds) have positive slope and subsequent frames have negative slope.
             * The final frames are likely to be echo and to be avoided.
             * Therefore take the first 0.6s to calculate the positive slope, leave a gap of 0.025 seconds and then get negative slope from the next 0.6 seconds.
42,21,21,42,21, 00, 21,-21,-21,-21, 00,-21,-42
42,42,21,21,42,-21, 21, 00,-21,-21,-21,-21, 00,-21,21,-21
42,42,21,21,42, 00, 00, 00,-21,-21,-21,-21,-21
21,21,00,00,21, 21,-21, 00, 00,-21, 00,-21,-21,21,-21,42
42,42,21,00,42, 00, 00,-21,-21,-21,-21, 00,-21,
21,42,21,21,21, 00,-21,-21,-21, 00,-21,-21
42,21,21,42,21, 21, 00,-21,-21,-21,-21
42,42,21,42,00, 00,-21, 00,-21,-21, 00,-21,-21
*/

            var spectralEvents = events.Select(x => (ChirpEvent)x).ToList();
            foreach (var ev in spectralEvents)
            {
                foreach (var track in ev.Tracks)
                {
                    var profile = track.GetTrackFrequencyProfile().ToArray();
                    var startSum = 0.0;
                    if (profile.Length >= 5)
                    {
                        startSum = profile[0] + profile[1] + profile[2] + profile[3] + profile[4];
                    }

                    var endSum = 0.0;
                    if (profile.Length >= 11)
                    {
                        endSum = profile[6] + profile[7] + profile[8] + profile[9] + profile[10];
                    }

                    LoggedConsole.WriteLine($"{startSum}    {endSum}");
                    LoggedConsole.WriteLine(DataTools.WriteArrayAsCsvLine(profile, "F0"));
                }
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

        /// <inheritdoc cref="NinoxBoobookConfig"/> />
        public class NinoxBoobookConfig : GenericRecognizerConfig, INamedProfiles<object>
        {
            public bool CombinePossibleSyllableSequence { get; set; } = false;

            public double SyllableStartDifference { get; set; } = 0.5;

            public double SyllableHertzGap { get; set; } = 200;
        }
    }
}