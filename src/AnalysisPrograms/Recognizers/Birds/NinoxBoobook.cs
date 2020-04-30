// <copyright file="NinoxBoobook.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

/// <summary>
/// A recognizer for the Australian Boobook Owl, /// https://en.wikipedia.org/wiki/Australian_boobook .
/// Eight subspecies of the Australian boobook are recognized,
/// with three further subspecies being reclassified as separate species in 2019 due to their distinctive calls and genetics.
/// THis recognizer has been trained on good quality calls from the Gympie recordings obtained by Yvonne Phillips.
/// The recognizer has also been run across several recordings of Boobook from NZ (recordings obtained from Stuart Parsons.
/// The NZ Boobook calls were of poor quality (distant and echo) and were 200 Hertz higher and performance was not good.
/// </summary>
namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    internal class NinoxBoobook : RecognizerBase
    {
        private static readonly ILog BoobookLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Towsey";

        public override string SpeciesName => "NinoxBoobook";

        public override string Description => "[ALPHA] Detects acoustic events for the Australian Boobook owl.";

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

        /// <summary>
        /// This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording">one minute of audio recording.</param>
        /// <param name="genericConfig">config file that contains parameters used by all profiles.</param>
        /// <param name="segmentStartOffset">when recording starts.</param>
        /// <param name="getSpectralIndexes">not sure what this is.</param>
        /// <param name="outputDirectory">where the recognizer results can be found.</param>
        /// <param name="imageWidth"> assuming ????.</param>
        /// <returns>recognizer results.</returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, Config genericConfig, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            var recognizer = new GenericRecognizer();
            var config = recognizer.ParseConfig(FileInfo file);

            /*
            string[] profileNames = null;
            if (ConfigFile.HasProfiles(genericConfig))
            {
                profileNames = ConfigFile.GetProfileNames(genericConfig);
                int count = profileNames.Length;
                var message = $"Found {count} config profile(s): ";
                foreach (string s in profileNames)
                {
                    message = message + (s + ", ");
                }

                BoobookLog.Debug(message);
            }
            else
            {
                BoobookLog.Warn("No configuration profiles found.");
            }

            var combinedResults = new RecognizerResults();

            foreach (var profileName in profileNames)
            {
                var results = new RecognizerResults();

                if (ConfigFile.TryGetProfile(genericConfig, profileName, out var profile))
                {
                    results = recognizer.(audioRecording, genericConfig, "Territorial", segmentStartOffset);
                    BoobookLog.Debug("Boobook event count = " + results.Events.Count);
                }
                else
                {
                    BoobookLog.Warn($"Could not access {profileName} configuration parameters");
                }

                // combine the results i.e. add wing-beat events to the list of territorial call events.
                //NOTE: The returned territorialResults and wingbeatResults will never be null.
                combinedResults.Events.AddRange(results.Events);
                combinedResults.Plots.AddRange(results.Plots);
            }

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected" in <Towsey.PteropusSpecies.yml> config file.
            //SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);
            */
            return combinedResults;
        }

        /// <summary>
        /// returns a base sonogram type from which spectrogram images are prepared.
        /// </summary>
        internal static BaseSonogram GetSonogram(Config configuration, AudioRecording audioRecording)
        {
            var sonoConfig = new SonogramConfig
            {
                WindowSize = 512,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = configuration.GetDoubleOrNull(AnalysisKeys.NoiseBgThreshold) ?? 0.0,
                WindowOverlap = 0.0,
            };

            // now construct the standard decibel spectrogram WITH noise removal
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(sonoConfig, audioRecording.WavReader);
            return sonogram;
        }

        /// <summary>
        /// THis method can be modified if want to do something non-standard with the output spectrogram.
        /// </summary>
        internal static void SaveDebugSpectrogram(RecognizerResults results, Config genericConfig, DirectoryInfo outputDirectory, string baseName)
        {
            //var image = sonogram.GetImageFullyAnnotated("Test");
            var image = SpectrogramTools.GetSonogramPlusCharts(results.Sonogram, results.Events, results.Plots, null);
            image.Save(Path.Combine(outputDirectory.FullName, baseName + ".profile.png"));
        }
    }
}