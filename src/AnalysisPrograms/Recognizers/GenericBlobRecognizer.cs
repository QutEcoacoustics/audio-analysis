// <copyright file="GenericBlobRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Recognizers
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    public class GenericBlobRecognizer
    {
        public class BlobParameters
        {
            /// <summary>
            /// Gets or sets the frame or Window size, i.e. number of signal samples. Must be power of 2. Typically 512.
            /// </summary>
            public int FrameSize { get; set; }

            /// <summary>
            /// Gets or sets the frame or Window step i.e. before start of next frame.
            /// The overlap can be any number of samples but less than the frame length/size.
            /// </summary>
            public int FrameStep { get; set; }

            /// <summary>
            /// Gets or sets the bottom bound of the rectangle. Units are Hertz.
            /// </summary>
            public int MinHz { get; set; }

            /// <summary>
            /// Gets or sets the the top bound of the rectangle. Units are Hertz.
            /// </summary>
            public int MaxHz { get; set; }

            /// <summary>
            /// Gets or sets the buffer (bandwidth of silence) below the blob rectangle. Units are Hertz.
            /// </summary>
            public int BottomHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets the buffer (bandwidth of silence) above the blob rectangle. Units are Hertz.
            /// Quite often this will be set to zero Hertz because upper bounds variable, depending on distance of the source.
            /// </summary>
            public int TopHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets the minimum allowed duration of the acoustic event. Units are seconds.
            /// </summary>
            public double MinDuration { get; set; }

            /// <summary>
            /// Gets or sets the maximum allowed duration of the acoustic event. Units are seconds.
            /// </summary>
            public double MaxDuration { get; set; }

            /// <summary>
            /// Gets or sets the threshold of "loudness" of an acoustic event. Units are decibels.
            /// </summary>
            public double DecibelThreshold { get; set; }
        }

        /// <summary>
        /// Detects a concentration of acoustic energy that falls within the bounds of a rectangle.
        /// </summary>
        /// <param name="audioRecording">the recording.</param>
        /// <param name="configuration">the config file.</param>
        /// <param name="profileName">name of the call/event type.</param>
        /// <param name="segmentStartOffset">where one segment is located in the total recording.</param>
        /// <returns>a list of events.</returns>
        public static RecognizerResults BlobRecognizer(AudioRecording audioRecording, Config configuration, string profileName, TimeSpan segmentStartOffset)
        {
            ConfigFile.TryGetProfile(configuration, profileName, out var profile);

            // get the common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? profileName;
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? profileName;

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
            //2. Convert each segment to a spectrogram.
            using (AudioAnalysisTools.StandardSpectrograms.BaseSonogram sonogram = RecognizerTools.GetSonogram(configuration, audioRecording: audioRecording))
            {
                var decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, sonogram.NyquistFrequency);

                // prepare plots
                double intensityNormalisationMax = 3 * decibelThreshold;
                var eventThreshold = decibelThreshold / intensityNormalisationMax;
                var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalisationMax);
                var plot = new Plot(speciesName + "Blob", normalisedIntensityArray, eventThreshold);
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
                    ae.FileName = audioRecording.BaseName;
                    ae.SpeciesName = speciesName;
                    ae.Name = abbreviatedSpeciesName + profileName;
                    ae.Profile = profileName;
                    ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                    ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                });

                acousticEvents = RecognizerTools.FilterEventsForSpectralProfile(acousticEvents, sonogram);

                return new RecognizerResults()
                {
                    Events = acousticEvents,
                    Hits = null,
                    ScoreTrack = null,
                    Plots = plots,
                    Sonogram = sonogram,
                };
            }
        }
    }
}
