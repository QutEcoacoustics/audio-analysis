// <copyright file="NinoxStrenua.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using static AnalysisPrograms.Recognizers.GenericRecognizer;
    using Path = System.IO.Path;

    /// <summary>
    /// A recognizer for the Australian Powerful Owl, https://en.wikipedia.org/wiki/Powerful_owl.
    /// The owl is so named because it is the largest of the Australian owls and it preys on large marsupials such as possums.
    /// Its range is confined to the East and SE coast of Australia.
    /// Its conservation status is "threatened".
    /// This recognizer has been trained on good quality calls provided by NSW DPI by Brad Law and Kristen Thompson.
    /// </summary>
    internal class NinoxStrenua : RecognizerBase
    {
        private static readonly ILog PowerfulOwlLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "NinoxStrenua";

        public override string Description => "[ALPHA] Detects acoustic events for the Australian Powerful Owl.";

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(NinoxStrenuaConfig).TypeHandle);
            var config = ConfigFile.Deserialize<NinoxStrenuaConfig>(file);

            // validation of configs can be done here
            GenericRecognizer.ValidateProfileTagsMatchAlgorithms(config.Profiles, file);

            // This call sets a restriction so that only one generic algorithm is used.
            // CHANGE this to accept multiple generic algorithms as required.
            //if (result.Profiles.SingleOrDefault() is ForwardTrackParameters)
            if (config.Profiles?.Count == 1 && config.Profiles.First().Value is ForwardTrackParameters)
            {
                return config;
            }

            throw new ConfigFileException("NinoxStrenua expects one and only one ForwardTrack algorithm.", file);
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
            //class NinoxStrenuaConfig is defined at bottom of this file.
            var genericConfig = (NinoxStrenuaConfig)config;
            var recognizer = new GenericRecognizer();

            RecognizerResults combinedResults = recognizer.Recognize(
                audioRecording,
                genericConfig,
                segmentStartOffset,
                getSpectralIndexes,
                outputDirectory,
                imageWidth);

            var count = combinedResults.NewEvents.Count;
            PowerfulOwlLog.Debug($"Event count before post-processing = {count}.");
            if (combinedResults.NewEvents.Count == 0)
            {
                return combinedResults;
            }

            // ################### POST-PROCESSING of EVENTS ###################
            // Following two commented lines are different ways of casting lists.
            //var newEvents = spectralEvents.Cast<EventCommon>().ToList();
            //var spectralEvents = events.Select(x => (SpectralEvent)x).ToList();

            //NOTE:
            // The generic recognizer does some post-processing of events prior to returning the list of combined events.
            // Its post-processing steps are determined by config settings.
            // Generic post processing step 1: Combine overlapping events.
            // Generic post processing step 2: Combine possible syllable sequences and filter on excess syllable count.
            // Generic post processing step 3: Remove events whose bandwidth is too small or large.
            // Generic post processing step 4: Remove events that have excessive noise in their side-bands.

            // Post-processing steps are put here:

            if (combinedResults.NewEvents.Count == 0)
            {
                PowerfulOwlLog.Debug($"Zero events after post-processing.");
            }

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

        public class NinoxStrenuaConfig : GenericRecognizerConfig, INamedProfiles<object>
        {
        }
    }
}
