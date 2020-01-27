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
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.RecognizerTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using global::Recognizers;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This class calls recognizers for generic syllable types.
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

            var profileResults = new RecognizerResults()
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

                switch (profileConfig)
                {
                    case GenericBlobRecognizer.BlobParameters bp:
                        Log.Debug("Using the blob algorithm... ");
                        throw new NotImplementedException();
                        // var blobConfig = genericConfig.ToDictionary<string, string>();

                        results = GenericBlobRecognizer.BlobRecognizer(audioRecording, genericConfig, name,
                            segmentStartOffset);
                        Log.Debug(name + " event count = " + results.Events.Count);
                        break;
                    case GenericOscillationRecognizer.OscillationParameters op:
                        throw new NotImplementedException();
                        Log.Debug("Using the oscillation algorithm... ");
                        results = GenericOscillationRecognizer.OscillationRecognizer(audioRecording, genericConfig,
                            name, segmentStartOffset);
                        Log.Debug(name + " event count = " + results.Events.Count);
                        break;
                    case GenericWhistleRecognizer.WhistleParameters wp:
                        throw new NotImplementedException();
                        break;
                    case AedParameters ap:
                        Log.Debug("Using the AED algorithm... ");
                        throw new NotImplementedException();
                        break;
                    default:
                        throw new ConfigFileException($"The algorithm parameters in profile {profileName}");
                        break;
                }

                // combine the results i.e. add the events list of call events.
                //NOTE: The returned territorialResults and wingbeatResults will never be null.
                profileResults.Events.AddRange(results.Events);
                profileResults.Plots.AddRange(results.Plots);
                profileResults.Sonogram = results.Sonogram;
                Log.Debug(name + " event count = " + profileResults.Events.Count);
            }

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected"
            //                                          in <Ecosounds.GenericRecognizers.yml> config file.
            //SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);

            return profileResults;
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
