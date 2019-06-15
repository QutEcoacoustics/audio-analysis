// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PteropusSpecies.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a template recognizer for the Australian Flying Fox.
//   Since there are several species, this project is started using only the generic name for Flying Foxes.

// Proposed algorithm has 8 steps
// 1. Break long recordings into one-minute segments.
// 2. Convert each segment to a spectrogram.
// 3. Obtain a noise profile for each segment. This is to be used later to remove insect chorusing.
// 4. Scan the one-minute waveform and select "spike maxima" whose amplitude exceeds a decibel threshold, D.
// 5. Extract a single frame (say 512 samples) centred on each spike and convert to a spike spectrum.
// 6. Subtract the noise profile from the spike spectrum.
// 7. Smooth the remaining spectrum.
// 8. Look for evenly spaced harmonics in the smoothed spectrum.
// Typically the lowest harmonic will lie between 1200 Hz and 3000 Hz and the higher ones evenly spaced.
// This is the tricky bit due to variability but may work to use auto-correlation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Tools.Wav;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This is a template recognizer for species of Flying Fox, Pteropus species
    /// </summary>
    internal class PteropusSpecies : RecognizerBase
    {
        public override string Author => "Truskinger";

        public override string SpeciesName => "PteropusSpecies";

        public override string Description => "[STATUS DESCRIPTION] Detects acoustic events for species of Flying Fox, Pteropus species";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording">one minute of audio recording.</param>
        /// <param name="configuration">config file.</param>
        /// <param name="segmentStartOffset">when recording starts.</param>
        /// <param name="getSpectralIndexes">not sure what this is.</param>
        /// <param name="outputDirectory">where the recogniser results can be found.</param>
        /// <param name="imageWidth"> assuming ????.</param>
        /// <returns>recogniser results.</returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            // Get a value from the config file - with a backup default
            int minHz = configuration.GetIntOrNull(AnalysisKeys.MinHz) ?? 600;

            // Get a value from the config file - with no default, throw an exception if value is not present
            //int maxHz = ((int?)configuration[AnalysisKeys.MaxHz]).Value;

            // Get a value from the config file - without a string accessor, as a double
            double someExampleSettingA = configuration.GetDoubleOrNull("SomeExampleSettingA") ?? 0.0;

            // common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            /*
             * Examples of using profiles
             */

            // Examples of the APIs available. You don't need all of these commands! Pick and choose.
            bool hasProfiles = ConfigFile.HasProfiles(configuration);

            //Config profile = ConfigFile.GetProfile<Config, Aed.AedConfiguration>(configuration, "Groote");
            Config profile2;

            //bool success = ConfigFile.TryGetProfile(configuration, "FemaleRelease", out profile2);
            //string[] profileNames = ConfigFile.GetProfileNames<Config>(configuration);
            //            IEnumerable<(string Name, object Profile)> allProfiles = ConfigFile.GetAllProfiles<IProfile<object>>(configuration);
            //            foreach (var profile in allProfiles)
            //            {
            //                object currentProfile = profile.Profile;
            //                Log.Info(profile.Name + ": " + ((int)currentProfile.MinHz).ToString());
            //            }

            //######################
            //2.Convert each segment to a spectrogram.

            // Profile example: running the same algorithm on every profile with different settings (regional variation)
            /*
            List<AcousticEvent> allAcousticEvents = new List<AcousticEvent>();
            Dictionary<string, Config> allProfiles = ConfigFile.GetAllProfiles(configuration);
            foreach (var kvp in allProfiles)
            {
                string profileName = kvp.Key;
                Log.Info($"Analyzing profile: {profileName}");
                Config currentProfile = kvp.Value;

                // extract parameters
                int minHz = (int)configuration[AnalysisKeys.MinHz];

                // ...

                // run the algorithm
                List<AcousticEvent> acousticEvents;
                Oscillations2012.Execute( All the correct parameters, minHz);

                // augment the returned events
                acousticEvents.ForEach(ae =>
                {
                    ae.SpeciesName = speciesName;
                    ae.Profile = profileName;
                    ae.AnalysisIdealSegmentDuration = recordingDuration;
                    ae.Name = abbreviatedSpeciesName;
                });

                // add events found in this profile to the total list
                allAcousticEvents.AddRange(acousticEvents);
            }
            */

            // Profile example: running a different algorithm on different profiles
            /*
            bool hasProfiles = ConfigFile.HasProfiles(configuration);
            if (hasProfiles)
            {
                // add resulting events from each algorithm into the combined event list
                allAcousticEvents.AddRange(RunFemaleProfile(...all the arguments));
                allAcousticEvents.AddRange(RunMaleProfile(...all the arguments));
            }

            // example method
            private static List<AcousticEvent> RunFemaleProfile(configuration, rest of arguments)
            {
                const string femaleProfile = "Female";
                Config currentProfile = ConfigFile.GetProfile(configuration, femaleProfile);
                Log.Info($"Analyzing profile: {femaleProfile}");

                // extract parameters
                int minHz = (int)configuration[AnalysisKeys.MinHz];

                // ...

                // run the algorithm
                List<AcousticEvent> acousticEvents;
                Oscillations2012.Execute(All the correct parameters, minHz);

                // augment the returned events
                acousticEvents.ForEach(ae =>
                {
                    ae.SpeciesName = speciesName;
                    ae.Profile = femaleProfile;
                    ae.AnalysisIdealSegmentDuration = recordingDuration;
                    ae.Name = abbreviatedSpeciesName;
                });

                return acousticEvents;
            }
            */

            // get samples
            var samples = audioRecording.WavReader.Samples;

            // make a spectrogram
            var config = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = configuration.GetDoubleOrNull(AnalysisKeys.NoiseBgThreshold) ?? 0.0,
            };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, audioRecording.WavReader);

            // get high resolution indices

            // when the value is accessed, the indices are calculated
            var indices = getSpectralIndexes.Value;

            // check if the indices have been calculated - you shouldn't actually need this
            if (getSpectralIndexes.IsValueCreated)
            {
                // then indices have been calculated before
            }

            var foundEvents = new List<AcousticEvent>();

            // some kind of loop where you scan through the audio

            // 'find' a Flying Fox event - if you find an event, store the data in the AcousticEvent class
            var anEvent = new AcousticEvent(
                segmentStartOffset,
                new Oblong(50, 50, 100, 100),
                sonogram.NyquistFrequency,
                sonogram.Configuration.FreqBinCount,
                sonogram.FrameDuration,
                sonogram.FrameStep,
                sonogram.FrameCount)
            {
                Name = "Flying Fox",
            };

            foundEvents.Add(anEvent);

            return new RecognizerResults()
            {
                Events = foundEvents,
                Hits = null,
                ScoreTrack = null,

                //Plots = null,
                Sonogram = sonogram,
            };
        }
    }
}
