// <copyright file="OscillationParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    /// <summary>
    /// There are currently two algorithms implemented in AnalysisPrograms to detect temporal oscillations in a spectrogram, "Standard" and "Hits".
    /// At the heart of both is a Discrete Cosine Transform (DCT) which identifies an oscillation and determines its oscillation rate or the inverse, its periodicity.
    /// Note that other algorithms could also be used to identify an oscillation, in particular a Discrete Fourier Transform, but currently this is not implemented.
    /// The Standard DCT algorithm is implemented in the class Oscillations2019. The Hits algorithm is implemented in the class Oscillations2012.
    /// Given a spectrogram, a search band and other constraints, these algorithms identify acoustic events containing temporal oscillations.
    /// Eight of the ten parameters required for these algorithms are the same - just two differences.
    /// The identical parameters are as follows:
    /// (1) MinDuration, (2) MaxDuration, (3) MinHertz, (4) MaxHertz. These constrain the size of the event within the spectrogram.
    /// MinHertz and MaxHertz idenfiy the search band. All discovered events will occupy this band.
    /// (5) MinOscillationFrequency and (6) MaxOscillationFrequency set the minimum and maximum acceptable oscillation rate.
    /// Although these rates are defined as "oscillations per second" the calculations are done using periodicity. Periodicity = 1/OscillationRate.
    /// (7) DctDuration and (8) DctThreshold. These parameters determine how the DCT is implemented.
    /// DctDuration sets the time span (in seconds) of the DCT. Typically forreliable detection, you would want several oscillations to occur within the DCT duration.
    /// DctThreshold (a value in [0,1]) sets the minimum required amplitude for the oscillation to be accepted.
    /// The final two parameters are used differently by each algorithm: (9) EventThreshold and (10) DecibelThresholds. ############### SMOOTHING WINDOW
    /// The steps for each algorithm are outlined below. The first three steps and last two steps are identical for each.
    ///
    /// ### THE STANDARD algorithm for detecting oscillations - This is implemented in the class Oscillations2019.
    /// STEP 1: smooth the spectrum in each timeframe. This is intended to make oscillations more regular. Currently a smoothing window of 3 is used by default.
    /// STEP 2: extract an array of decibel values, frame averaged over the search band.
    ///         decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, spectrogram.NyquistFrequency);
    /// STEP 3: prepare a set of cosine basis functions.
    /// STEP 4: DETECT OSCILLATIONS in the extracted array of average decibel values.
    ///         DetectOscillations(decibelArray, framesPerSecond, decibelThreshold, dctDuration, minOscFreq, maxOscFreq, dctThreshold, out var dctScores, out var oscFreq);
    /// STEP 5: Scan the decibel array for peak values and do a DCT starting at each peak whose amplitude exceeds the current DecibelThreshold.
    /// STEP 6: Ignore first four coefficients. Assign value of largest coefficient to the corresponding positions in the dctScores array only if its value is greater than the DctThreshold and greater than that in the dctScores array.
    ///         This becomes the array of oscillation scores.
    /// STEP 7: Apply a smoothing window to the array of oscillation scores - window=11 has been the DEFAULT. Now letting user set this.
    ///         dctScores = DataTools.filterMovingAverage(dctScores, SmoothingWindow);
    /// STEP 8: Search the array of DCT scores to find events that satisfy the constraints set by parameters (1) to (6). And ScoreThreshold.
    ///         events = OscillationEvent.ConvertOscillationScores2Events(spectrogram, minDuration, maxDuration, minHz, maxHz, minOscilFrequency,  maxOscilFrequency, oscScores, eventThreshold, segmentStartOffset);
    /// ###
    /// ### THE HITS algorithm for detecting oscillations - This is implemented in the class Oscillations2012.
    /// STEP 1: smooth the spectrum in each timeframe. This is intended to make oscillations more regular. Currently a smoothing window of 3 is used by default.
    /// STEP 2: extract an array of decibel values, frame averaged over the search band.
    ///         decibelArray = SNR.CalculateFreqBandAvIntensity(sonogram.Data, minHz, maxHz, spectrogram.NyquistFrequency);
    /// STEP 3: prepare a set of cosine basis functions.
    /// STEP 4: DETECT OSCILLATIONS in each frequency bin separately. A 'hit' occurs where the DCT coefficient is greater than the DctThreshold and falls within the acceptable oscillation rate.
    ///         hits = DetectOscillations(sonogram, minHz, maxHz, dctDuration, minOscilFrequency.Value, maxOscilFrequency.Value, dctThreshold);
    /// STEP 5: Remove isolated oscillations
    ///         hits = RemoveIsolatedOscillations(hits);
    /// STEP 6: Calculate an array of oscillation scores as the fraction of frequency bins in the search frequecny band that contain a dct coefficient value greater than zero.
    ///         oscScores = GetOscillationScores(hits, minHz, maxHz, sonogram.FBinWidth);
    /// STEP 7: Apply a smoothing window to the array of oscillation scores. Parameter: SmoothingWindow
    ///         STEP 6 is implemented by the same method as STEP 7 in the STANDARD algorithm.
    ///         oscScores = DataTools.filterMovingAverage(oscScores, smoothingWindow);
    /// STEP 8: Search the array of DCT scores to find events that satisfy the constraints set by parameters (1) to (6). And ScoreThreshold.
    ///         STEP 7 is implemented by the same method as STEP 7 in the STANDARD algorithm.
    ///         events = OscillationEvent.ConvertOscillationScores2Events(spectrogram, minDuration, maxDuration, minHz, maxHz, minOscilFrequency,  maxOscilFrequency, oscScores, eventThreshold, segmentStartOffset);
    /// ###.
    /// </summary>
    public enum OscillationAlgorithm
    {
        Standard,
        Hits,
    }

    /// <summary>
    /// Parameters needed from a config file to detect oscillation components.
    /// </summary>
    [YamlTypeTag(typeof(OscillationParameters))]
    public class OscillationParameters : DctParameters
    {
        /// <summary>
        /// Gets or sets he algorithm to be used to find oscillation events.
        /// </summary>
        public OscillationAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Gets or sets the minimum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates below the min threshold.
        /// </summary>
        /// <value>The value in oscillations per second.</value>
        public double? MinOscillationFrequency { get; set; }

        /// <summary>
        /// Gets or sets the maximum OSCILLATIONS PER SECOND
        /// Ignore oscillation rates above the max threshold.
        /// </summary>
        /// <value>The value in oscillations per second.</value>
        public double? MaxOscillationFrequency { get; set; }

        /// <summary>
        /// Return oscillation events as determined by the user set parameters.
        /// </summary>
        public static (List<EventCommon> OscillEvents, List<Plot> Plots) GetOscillationEvents(
            SpectrogramStandard spectrogram,
            OscillationParameters op,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var algorithm = op.Algorithm;

            List<EventCommon> events;
            List<Plot> plots;
            double[,] hits = null;

            if (algorithm == OscillationAlgorithm.Hits)
            {
                (events, plots, hits) = Oscillations2012.GetComponentsWithOscillations(
                    spectrogram,
                    op,
                    decibelThreshold,
                    segmentStartOffset,
                    profileName);
            }
            else
            {
                // the standard algorithm is the default.
                (events, plots) = Oscillations2019.GetComponentsWithOscillations(
                    spectrogram,
                    op,
                    decibelThreshold,
                    segmentStartOffset,
                    profileName);
            }

            // save a debug image of the spectrogram which includes the HITS overlay.
            var image3 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, events, plots, hits, profileName + " Oscillations");
            var path = "C:\\temp\\oscillationsImage.png";
            image3.Save(path);

            return (events, plots);
        }
    }
}