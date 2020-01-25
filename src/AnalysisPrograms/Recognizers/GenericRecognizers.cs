// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericRecognizers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a recognizer for common generic acoustsic syllables.

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using global::Recognizers;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This class calls recognizers for generic syllable types.
    /// </summary>
    internal class GenericRecognizers : RecognizerBase
    {
        private static readonly ILog GrLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string Author => "Ecosounds";

        public override string SpeciesName => "GenericRecognizers";

        public override string Description => "[ALPHA] Detects generic acoustic events";

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
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config genericConfig,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            string[] profileNames = null;

            if (ConfigFile.HasProfiles(genericConfig))
            {
                profileNames = ConfigFile.GetProfileNames(genericConfig);
                int count = profileNames.Length;
                var message = $"Found {count} analysis profile(s): ";
                foreach (string s in profileNames)
                {
                    message = message + (s + ", ");
                }

                GrLog.Debug(message);
                Console.WriteLine(message);
            }
            else
            {
                GrLog.Warn("No configured profiles found.");
            }

            var profileResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // Now process each of the profiles
            foreach (string name in profileNames)
            {
                GrLog.Info("Processing profile: " + name);

                if (ConfigFile.TryGetProfile(genericConfig, name, out var profile))
                {
                    RecognizerResults results = new RecognizerResults();

                    var algorithmName = profile.GetString("Algorithm");
                    //var parameters = profile.GetString("Parameters");

                    switch (algorithmName)
                    {
                        case "BlobRecognizer":
                            GrLog.Info("    Use algorithm One: " + algorithmName);
                            results = GenericBlobRecognizer.BlobRecognizer(audioRecording, genericConfig, name, segmentStartOffset);
                            GrLog.Debug(name + " event count = " + results.Events.Count);
                            break;
                        case "OscillationRecognizer":
                            GrLog.Info("    Use algorithm Two: " + algorithmName);
                            results = GenericOscillationRecognizer.OscillationRecognizer(audioRecording, genericConfig, name, segmentStartOffset);
                            GrLog.Debug(name + " event count = " + results.Events.Count);
                            break;
                        default:
                            GrLog.Info("    WARNING: Algorithm Name not recognised: " + algorithmName);
                            break;
                    }

                    // combine the results i.e. add the events list of call events.
                    //NOTE: The returned territorialResults and wingbeatResults will never be null.
                    profileResults.Events.AddRange(results.Events);
                    profileResults.Plots.AddRange(results.Plots);
                    profileResults.Sonogram = results.Sonogram;
                    GrLog.Debug(name + " event count = " + profileResults.Events.Count);
                }
                else
                {
                    GrLog.Warn("Could not access " + name + " configuration parameters");
                }
            } // end of profiles

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected"
            //                                          in <Ecosounds.GenericRecognizers.yml> config file.
            //SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);

            return profileResults;
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
    }
}
