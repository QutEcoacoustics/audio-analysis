// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PteropusSpecies.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a recognizer for the Australian Flying Fox.
//   Since there are several species, this project is started using only the generic name for Flying Foxes.

// As of August 2019, the Flying Fox TERRITORIAL CALL recogniser employs the following steps:
// 1. Break long recordings into one-minute segments.
// 2. Convert each segment to a spectrogram (use frame of 512).
// 3. Do standard noise subtraction on the spectrogram. This removes bands of strong insect chorusing.
// 4. Set up equivalent of a band-pass filter between min Hz and max Hz (Hz bounds are set in Territorial profile of the config file).
// 5. Calculate average dB amplitude in each frame of the band pass. THis yields an array of dB values.
// 6. Find the maxima in the dB array where value exceeds a threshold value (set in config file).
// 7. I tried smoothing the array but this turns out not to be a good idea!
// 8. Each maximum becomes the locus of a possible acoustic event. Step both forwards and backwards along the dB array until the array value drops below the dB threshold.
// 9. Test 1: Does the event duration lie within the min and max duration bounds set in the config file.
//10: Take the average of the spectrum that lies within the temporal & frequency bounds defined above.
// NOTE: Averaging of dB values in steps 5 and 10 is not done via antilogs & logs - but good enough for these purposes and faster!
//11: Test 2: The spectral maximum must lie below 4 kHz.
//12: Test 3: The average dB value in the 0-1kHz band must be less than average over the entire 1-8kHz band. i.e. there should be little energy in the 0-1kHz band.
//13: Test 4: The average dB value in the 4-5kHz band must be less than average over the entire 1-8kHz band. i.e. there should be little energy in the 4-5kHz band.
//            Test 4 is problematic because it may be that noise removal has hit the insect band. This may need further examination but it appears as if the bat call has less energy in the 4-5kHz band.
//
// As of August 2019, the Flying Fox WING-BEAT recogniser employs the following steps:
// 1 - 3. Same as above.
// 4. Set up equivalent of a band-pass filter between min Hz and max Hz (Hz bounds are set in the Wingbeat profile of the config file).
// 5. Calculate average dB amplitude in each frame of the band pass. THis yields an array of dB values.
// 6. Find the maxima in the dB array where value exceeds a threshold value (set in config file).
// 7. At each maximum perform oscillation detection using the parameters shown in the Wingbeat profile of the config file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// This is a recognizer for species of Flying Fox, Pteropus species.
    /// </summary>
    internal class PteropusSpecies : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "PteropusSpecies";

        public override string Description => "[ALPHA] Detects acoustic events for species of Flying Fox, Pteropus species";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        /// <param name="outputDirectory">where the recogniser results can be found.</param>
        /// <param name="imageWidth"> assuming ????.</param>
        /// <returns>recogniser results.</returns>
        public override RecognizerResults Recognize(AudioRecording audioRecording, Config genericConfig, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            if (ConfigFile.HasProfiles(genericConfig))
            {
                string[] profileNames = ConfigFile.GetProfileNames(genericConfig);
                int count = profileNames.Length;
                var message = $"Found {count} config profile(s): ";
                foreach (string s in profileNames)
                {
                    message = message + (s + ", ");
                }

                log.Debug(message);
            }
            else
            {
                log.Warn("No configuration profiles found. Two profiles expected for the Flying Fox recogniser.");
            }

            var territorialResults = new RecognizerResults();

            if (ConfigFile.TryGetProfile(genericConfig, "Territorial", out var profile1))
            {
                territorialResults = TerritorialCall(audioRecording, genericConfig, "Territorial", segmentStartOffset);
                log.Debug("Territory event count = " + territorialResults.Events.Count);
            }
            else
            {
                log.Warn("Could not access Territorial configuration parameters");
            }

            var wingbeatResults = new RecognizerResults();
            if (ConfigFile.TryGetProfile(genericConfig, "Wingbeats", out var profile2))
            {
                wingbeatResults = WingBeats(audioRecording, genericConfig, "Wingbeats", segmentStartOffset);
                log.Debug("Wingbeat event count = " + wingbeatResults.Events.Count);
            }
            else
            {
                log.Warn("Could not access Wingbeats configuration parameters");
            }

            // combine the results i.e. add wing-beat events to the list of territorial call events.
            //NOTE: The returned territorialResults and wingbeatResults will never be null.
            territorialResults.Events.AddRange(wingbeatResults.Events);
            territorialResults.Plots.AddRange(wingbeatResults.Plots);

            //UNCOMMENT following line if you want special debug spectrogram, i.e. with special plots.
            //  NOTE: Standard spectrograms are produced by setting SaveSonogramImages: "True" or "WhenEventsDetected" in <Towsey.PteropusSpecies.yml> config file.
            //SaveDebugSpectrogram(territorialResults, genericConfig, outputDirectory, audioRecording.BaseName);

            return territorialResults;
        }

        /// <summary>
        /// THis method does the work.
        /// </summary>
        /// <param name="audioRecording">the recording.</param>
        /// <param name="configuration">the config file.</param>
        /// <param name="profileName">name of the call/event type.</param>
        /// <param name="segmentStartOffset">where one segment is located in the total recording.</param>
        /// <returns>a list of events.</returns>
        private static RecognizerResults TerritorialCall(AudioRecording audioRecording, Config configuration, string profileName, TimeSpan segmentStartOffset)
        {
            ConfigFile.TryGetProfile(configuration, profileName, out var profile);

            // get the common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "Pteropus species";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "Pteropus";

            // The following parameters worked well on a ten minute recording containing 14-16 calls.
            // Note: if you lower the dB threshold, you need to increase maxDurationSeconds
            int minHz = profile.GetIntOrNull(AnalysisKeys.MinHz) ?? 800;
            int maxHz = profile.GetIntOrNull(AnalysisKeys.MaxHz) ?? 8000;
            double minDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MinDuration) ?? 0.15;
            double maxDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MaxDuration) ?? 0.5;
            double decibelThreshold = profile.GetDoubleOrNull(AnalysisKeys.DecibelThreshold) ?? 9.0;
            var minTimeSpan = TimeSpan.FromSeconds(minDurationSeconds);
            var maxTimeSpan = TimeSpan.FromSeconds(maxDurationSeconds);

            //######################
            //2. Don't use samples in this recogniser.
            //var samples = audioRecording.WavReader.Samples;
            //Instead, convert each segment to a spectrogram.
            var sonogram = GetSonogram(configuration, audioRecording);
            /*
            // make a spectrogram
            var sonoConfig = new SonogramConfig
            {
                WindowSize = 512,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = configuration.GetDoubleOrNull(AnalysisKeys.NoiseBgThreshold) ?? 0.0,
            };
            sonoConfig.WindowOverlap = 0.0;

            // now construct the standard decibel spectrogram WITH noise removal, and look for LimConvex
            // get frame parameters for the analysis
            var sonogram = (BaseSonogram)new SpectrogramStandard(sonoConfig, audioRecording.WavReader);
            */
            var decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, sonogram.NyquistFrequency);

            // prepare plots
            double intensityNormalisationMax = 3 * decibelThreshold;
            var eventThreshold = decibelThreshold / intensityNormalisationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalisationMax);
            var plot = new Plot(speciesName + " Territory", normalisedIntensityArray, eventThreshold);
            var plots = new List<Plot> { plot };

            //iii: CONVERT decibel SCORES TO ACOUSTIC EVENTS
            var acousticEvents = AcousticEvent.GetEventsAroundMaxima(
                decibelArray,
                segmentStartOffset,
                minHz,
                maxHz,
                decibelThreshold,
                minTimeSpan,
                maxTimeSpan,
                sonogram.FramesPerSecond,
                sonogram.FBinWidth);

            //iV add additional info to the acoustic events
            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.Name = abbreviatedSpeciesName + profileName;
                ae.Profile = profileName;
                ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
            });

            acousticEvents = FilterEventsForSpectralProfile(acousticEvents, sonogram);

            return new RecognizerResults()
            {
                Events = acousticEvents,
                Hits = null,
                ScoreTrack = null,
                Plots = plots,
                Sonogram = sonogram,
            };
        }

        /// <summary>
        /// Remove events whose acoustic profile does not match that of a flying fox.
        /// </summary>
        /// <param name="events">unfiltered acoustic events.</param>
        /// <param name="sonogram">includes matrix of spectrogram values.</param>
        /// <returns>filtered acoustic events.</returns>
        private static List<AcousticEvent> FilterEventsForSpectralProfile(List<AcousticEvent> events, BaseSonogram sonogram)
        {
            double[,] spectrogramData = sonogram.Data;
            int colCount = spectrogramData.GetLength(1);

            // The following freq bins are used to demarcate freq bands for spectral tests below.
            // The hertz values are hard coded but could be included in the config.yml file.
            int maxBin = (int)Math.Round(8000 / sonogram.FBinWidth);
            int fourKiloHzBin = (int)Math.Round(4000 / sonogram.FBinWidth);
            int oneKiloHzBin = (int)Math.Round(1000 / sonogram.FBinWidth);

            var filteredEvents = new List<AcousticEvent>();
            foreach (AcousticEvent ae in events)
            {
                int startFrame = ae.Oblong.RowTop;
                int endFrame = ae.Oblong.RowBottom;

                // get all the frames of the acoustic event
                //var subMatrix = DataTools.Submatrix(spectrogramData, startFrame, 0, endFrame, colCount - 1);

                // get only the frames from centre of the acoustic event
                var subMatrix = DataTools.Submatrix(spectrogramData, startFrame + 1, 0, startFrame + 4, maxBin);
                var spectrum = MatrixTools.GetColumnAverages(subMatrix);
                var normalisedSpectrum = DataTools.normalise(spectrum);
                normalisedSpectrum = DataTools.filterMovingAverageOdd(normalisedSpectrum, 11);
                var maxId = DataTools.GetMaxIndex(normalisedSpectrum);
                var hzMax = (int)Math.Ceiling(maxId * sonogram.FBinWidth);

                // Do TESTS to determine if event has spectrum matching a Flying fox.

                // Test 1: Spectral maximum should be below 4 kHz.
                bool passTest1 = maxId < fourKiloHzBin;

                // Test 2: There should be little energy in 0-1 kHz band.
                var subband1Khz = DataTools.Subarray(normalisedSpectrum, 0, oneKiloHzBin);
                double bandArea1 = subband1Khz.Sum();
                double energyRatio1 = bandArea1 / normalisedSpectrum.Sum();

                // 0.125  = 1/8.  i.e. test requires that energy in 0-1kHz band is less than average in all 8 kHz bands
                // 0.0938 = 3/32. i.e. test requires that energy in 0-1kHz band is less than 3/4 average in all 8 kHz bands
                // 0.0625 = 1/16. i.e. test requires that energy in 0-1kHz band is less than half average in all 8 kHz bands
                bool passTest2 = !(energyRatio1 > 0.1);

                // Test 3: There should be little energy in 4-5 kHz band.
                var subband4Khz = DataTools.Subarray(normalisedSpectrum, fourKiloHzBin, oneKiloHzBin);
                double bandArea2 = subband4Khz.Sum();
                double energyRatio2 = bandArea2 / normalisedSpectrum.Sum();
                bool passTest3 = !(energyRatio2 > 0.125);

                // TODO write method to determine similarity of spectrum to a true flying fox spectrum.
                // Problem: it is not certain how variable the FF spectra are.
                // In ten minutes of recording used so far, which include 14-15 obvious calls, there appear to be two spectral types.
                // One type has three peaks at around 1.5 kHz, 3 kHz and 6 kHz.
                // The other type have two peaks around 2.5 and 5.5 kHz.

                //if (passTest1)
                //if (true)
                if (passTest1 && passTest2 && passTest3)
                {
                    filteredEvents.Add(ae);

                    //DEBUG SPECTRAL PROFILES: UNCOMMENT following lines to get spectral profiles of the events.
                    /*
                    double startSecond = ae.EventStartSeconds - ae.SegmentStartSeconds;
                    string name = "CallSpectrum " + (ae.SegmentStartSeconds / 60) + "m" + (int)Math.Floor(startSecond) + "s hzMax" + hzMax;
                    var bmp2 = GraphsAndCharts.DrawGraph(name, normalisedSpectrum, 100);
                    bmp2.Save(Path.Combine(@"PATH\Towsey.PteropusSpecies", name + ".png"));
                    */
                }
            }

            return filteredEvents;
        }

        /// <summary>
        /// THis method does the work.
        /// </summary>
        /// <param name="audioRecording">the recording.</param>
        /// <param name="configuration">the config file.</param>
        /// <param name="profileName">name of call/event type to be found.</param>
        /// <param name="segmentStartOffset">where one segment is located in the total recording.</param>
        /// <returns>a list of events.</returns>
        private static RecognizerResults WingBeats(AudioRecording audioRecording, Config configuration, string profileName, TimeSpan segmentStartOffset)
        {
            ConfigFile.TryGetProfile(configuration, profileName, out var profile);
            // get the common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "Pteropus species";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "Pteropus";

            // The following parameters worked well on a ten minute recording containing 14-16 calls.
            // Note: if you lower the dB threshold, you need to increase maxDurationSeconds
            int minHz = profile.GetIntOrNull(AnalysisKeys.MinHz) ?? 100;
            int maxHz = profile.GetIntOrNull(AnalysisKeys.MaxHz) ?? 3000;
            double minDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MinDuration) ?? 1.0;
            double maxDurationSeconds = profile.GetDoubleOrNull(AnalysisKeys.MaxDuration) ?? 10.0;
            double dctDuration = profile.GetDoubleOrNull("DctDuration") ?? 1.0;
            double dctThreshold = profile.GetDoubleOrNull("DctThreshold") ?? 0.5;
            double minOscilFreq = profile.GetDoubleOrNull("MinOscilFreq") ?? 4.0;
            double maxOscilFreq = profile.GetDoubleOrNull("MaxOscilFreq") ?? 6.0;
            double eventThreshold = profile.GetDoubleOrNull("EventThreshold") ?? 0.3;

            //######################

            //2. Don't use samples in this recogniser.
            //var samples = audioRecording.WavReader.Samples;
            //Instead, convert each segment to a spectrogram.
            var sonogram = GetSonogram(configuration, audioRecording);
            var decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, sonogram.NyquistFrequency);

            // Look for wing beats using oscillation detector
            Oscillations2012.Execute(
                (SpectrogramStandard)sonogram,
                minHz,
                maxHz,
                dctDuration,
                (int)Math.Floor(minOscilFreq),
                (int)Math.Floor(maxOscilFreq),
                dctThreshold,
                eventThreshold,
                minDurationSeconds,
                maxDurationSeconds,
                out var scores,
                out var acousticEvents,
                out var hits,
                segmentStartOffset);

            /*
             * //NOTE: The following was an experiment which was discontinued!
            // Look for wing beats using pulse train detector
            double pulsesPerSecond = 5.1;
            var scores = PulseTrain.GetPulseTrainScore(decibelArray, pulsesPerSecond, sonogram.FramesPerSecond, 1.0);

            //iii: CONVERT Pulse Train SCORES TO ACOUSTIC EVENTS
            double pulseTrainThreshold = 0.5;
            var minTimeSpan = TimeSpan.FromSeconds(minDurationSeconds);
            var maxTimeSpan = TimeSpan.FromSeconds(maxDurationSeconds);
            var acousticEvents = AcousticEvent.GetEventsAroundMaxima(
                scores,
                segmentStartOffset,
                minHz,
                maxHz,
                pulseTrainThreshold,
                minTimeSpan,
                maxTimeSpan,
                sonogram.FramesPerSecond,
                sonogram.FBinWidth
            );

            double scoreThreshold = 0.5;
            var normalisedScoreArray = DataTools.NormaliseInZeroOne(scores, 0, 1.0);
            var plot2 = new Plot(speciesName + " Wingbeat Pulse-train Score", normalisedScoreArray, scoreThreshold);
            */

            // prepare plots
            double decibelThreshold = 12.0;
            double intensityNormalisationMax = 3 * decibelThreshold;
            var normThreshold = decibelThreshold / intensityNormalisationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalisationMax);
            var plot1 = new Plot(speciesName + " Wingbeat band", normalisedIntensityArray, normThreshold);
            var plot2 = new Plot(speciesName + " Wingbeat Osc Score", scores, eventThreshold);
            var plots = new List<Plot> { plot1, plot2 };

            // ######################################################################
            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.Name = abbreviatedSpeciesName + profileName;
                ae.Profile = profileName;
                ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;

                //UNCOMMENT following lines to get spectral profiles of the Wingbeat events.
                /*    double[,] spectrogramData = sonogram.Data;
                    int maxBin = (int)Math.Round(8000 / sonogram.FBinWidth);
                    double startSecond = ae.EventStartSeconds - ae.SegmentStartSeconds;
                    int startFrame = (int)Math.Round(startSecond / sonogram.FrameStep);
                    int frameLength = (int)Math.Round(ae.EventDurationSeconds / sonogram.FrameStep);
                    int endFrame = startFrame + frameLength;

                    // get only the frames from centre of the acoustic event
                    var subMatrix = DataTools.Submatrix(spectrogramData, startFrame + 10, 0, endFrame - 10, maxBin);
                    var spectrum = MatrixTools.GetColumnAverages(subMatrix);
                    var normalisedSpectrum = DataTools.normalise(spectrum);
                    normalisedSpectrum = DataTools.filterMovingAverageOdd(normalisedSpectrum, 11);
                    var maxId = DataTools.GetMaxIndex(normalisedSpectrum);
                    var hzMax = (int)Math.Ceiling(maxId * sonogram.FBinWidth);
                    string name = "BeatSpectrum " + (ae.SegmentStartSeconds / 60) + "m" + (int)Math.Floor(startSecond) + "s hzMax" + hzMax;
                    var bmp2 = GraphsAndCharts.DrawGraph(name, normalisedSpectrum, 100);

                    //Set required path
                    bmp2.Save(Path.Combine(@"C:\PATH", name + ".png"));
                    */
            });

            return new RecognizerResults()
            {
                Events = acousticEvents,
                Hits = null,
                ScoreTrack = null,
                Plots = plots,
                Sonogram = sonogram,
            };
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
            };
            sonoConfig.WindowOverlap = 0.0;

            // now construct the standard decibel spectrogram WITH noise removal, and look for LimConvex
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
