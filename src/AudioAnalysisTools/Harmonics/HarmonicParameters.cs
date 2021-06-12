// <copyright file="HarmonicParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect the stacked harmonic components of a soundscape.
    /// This can also be used for recognizing the harmonics of non-biological sounds such as from turbines, motor-bikes, compressors, hi-revving motors, etc.
    /// </summary>
    [YamlTypeTag(typeof(HarmonicParameters))]
    public class HarmonicParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets the dctThreshold.
        /// </summary>
        public double? DctThreshold { get; set; }

        /// <summary>
        /// Gets or sets the bottom bound of the gap between formants. Units are Hertz.
        /// </summary>
        public int? MinFormantGap { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of gap between formants. Units are Hertz.
        /// </summary>
        public int? MaxFormantGap { get; set; }

        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            HarmonicParameters hp,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            double[] decibelMaxArray;
            double[] harmonicIntensityScores;
            (spectralEvents, decibelMaxArray, harmonicIntensityScores) = GetHarmonicEvents(
                                spectrogram,
                                hp.MinHertz.Value,
                                hp.MaxHertz.Value,
                                decibelThreshold.Value,
                                hp.DctThreshold.Value,
                                hp.MinDuration.Value,
                                hp.MaxDuration.Value,
                                hp.MinFormantGap.Value,
                                hp.MaxFormantGap.Value,
                                segmentStartOffset);

            // prepare plot of resultant Harmonics decibel array.
            var plot = Plot.PreparePlot(decibelMaxArray, $"{profileName} (Harmonics:{decibelThreshold:F0}db)", decibelThreshold.Value);
            plots.Add(plot);

            return (spectralEvents, plots);
        }

        public static (List<EventCommon> SpectralEvents, double[] AmplitudeArray, double[] HarmonicIntensityScores) GetHarmonicEvents(
            SpectrogramStandard spectrogram,
            int minHz,
            int maxHz,
            double decibelThreshold,
            double dctThreshold,
            double minDuration,
            double maxDuration,
            int minFormantGap,
            int maxFormantGap,
            TimeSpan segmentStartOffset)
        {
            int nyquist = spectrogram.NyquistFrequency;
            var sonogramData = spectrogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            // get the min and max bin of the freq-band of interest.
            double freqBinWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / freqBinWidth);
            int maxBin = (int)Math.Round(maxHz / freqBinWidth);
            int bandBinCount = maxBin - minBin + 1;

            // extract the sub-band of interest
            double[,] subMatrix = MatrixTools.Submatrix(spectrogram.Data, 0, minBin, frameCount - 1, maxBin);

            //ii: DETECT HARMONICS
            // now look for harmonics in search band using the Xcorrelation-DCT method.
            var results = CrossCorrelation.DetectHarmonicsInSpectrogramData(subMatrix, decibelThreshold);

            // set up score arrays
            double[] dBArray = results.Item1; // this is not used currently.
            double[] harmonicIntensityScores = results.Item2; //an array of formant intesnity
            int[] maxIndexArray = results.Item3;

            for (int r = 0; r < frameCount; r++)
            {
                if (harmonicIntensityScores[r] < dctThreshold)
                {
                    //ignore frames where DCT coefficient (proxy for formant intensity) is below threshold
                    continue;
                }

                //ignore frames with incorrect formant gap
                // first get id of the maximum coefficient.
                int maxId = maxIndexArray[r];
                double freqBinGap = 2 * bandBinCount / (double)maxId;
                double formantGap = freqBinGap * freqBinWidth;

                // remove values where formantGap lies outside the expected range.
                if (formantGap < minFormantGap || formantGap > maxFormantGap)
                {
                    harmonicIntensityScores[r] = 0.0;
                }
            }

            // fill in brief gaps of one or two frames.
            var harmonicIntensityScores2 = new double[harmonicIntensityScores.Length];
            for (int r = 1; r < frameCount - 2; r++)
            {
                harmonicIntensityScores2[r] = harmonicIntensityScores[r];
                if (harmonicIntensityScores[r - 1] > dctThreshold && harmonicIntensityScores[r] < dctThreshold)
                {
                    // we have arrived at a possible gap. Fill the gap.
                    harmonicIntensityScores2[r] = harmonicIntensityScores[r - 1];
                }

                //now check if the gap is two frames wide
                if (harmonicIntensityScores[r + 1] < dctThreshold && harmonicIntensityScores[r + 2] > dctThreshold)
                {
                    harmonicIntensityScores2[r + 1] = harmonicIntensityScores[r + 2];
                    r += 1;
                }
            }

            //extract the events based on length and threshhold.
            // Note: This method does NOT do prior smoothing of the score array.
            var harmonicEvents = ConvertScoreArray2Events(
                    spectrogram,
                    harmonicIntensityScores2,
                    dBArray,
                    maxIndexArray,
                    minDuration,
                    maxDuration,
                    minHz,
                    maxHz,
                    bandBinCount,
                    dctThreshold,
                    segmentStartOffset);

            return (harmonicEvents, dBArray, harmonicIntensityScores2);
        }

        /// <summary>
        /// Finds harmonic events in an array harmonic scores.
        /// NOTE: The score array is assumed to be temporal i.e. each element of the array is derived from a time frame.
        /// The method uses the passed scoreThreshold in order to calculate a normalised score.
        /// Max possible score := threshold * 5.
        /// normalised score := score / maxPossibleScore.
        /// </summary>
        /// <param name="scores">the array of harmonic scores.</param>
        /// <param name="maxIndexArray">the array of max index values derived from the DCT. Used to calculate the harmonic interval.</param>
        /// <param name="minDuration">duration of event must exceed this to be a valid event.</param>
        /// <param name="maxDuration">duration of event must be less than this to be a valid event.</param>
        /// <param name="minHz">lower freq bound of the event.</param>
        /// <param name="maxHz">upper freq bound of the event.</param>
        /// <param name="scoreThreshold">threshold.</param>
        /// <param name="segmentStartOffset">the time offset from segment start to the recording start.</param>
        /// <returns>a list of acoustic events.</returns>
        public static List<EventCommon> ConvertScoreArray2Events(
            SpectrogramStandard spectrogram,
            double[] scores,
            double[] dBArray,
            int[] maxIndexArray,
            double minDuration,
            double maxDuration,
            int minHz,
            int maxHz,
            int bandBinCount,
            double scoreThreshold,
            TimeSpan segmentStartOffset)
        {
            double framesPerSec = spectrogram.FramesPerSecond;
            double freqBinWidth = spectrogram.FBinWidth;

            // create a unit converter
            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            // used this to calculate a normalised score between 0 - 1.0
            double maxPossibleScore = 5 * scoreThreshold;
            var scoreRange = new Interval<double>(0, maxPossibleScore);

            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            int startFrame = 0;
            int frameCount = scores.Length;

            var events = new List<EventCommon>();

            // pass over all time frames
            for (int i = 0; i < frameCount; i++)
            {
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startFrame = i + 2;
                }
                else // check for the end of an event
                if (isHit && scores[i] <= scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    double duration = (i - 1 - startFrame) * frameOffset;

                    if (duration < minDuration || duration > maxDuration)
                    {
                        //skip events with duration shorter than threshold
                        continue;
                    }

                    // obtain an average score and harmonic interval for the duration of the potential event.
                    double avScore = 0.0;
                    double avIndex = 0;
                    for (int n = startFrame; n <= i; n++)
                    {
                        avScore += scores[n];
                        avIndex += maxIndexArray[n];
                    }

                    // calculate average event score
                    int eventLength = i - startFrame;
                    avScore /= eventLength;
                    avIndex /= eventLength;
                    double freqBinGap = 2 * bandBinCount / avIndex;
                    double harmonicInterval = freqBinGap * freqBinWidth;

                    // calculate start and end time of this event relative to start of segment.
                    var eventStartWrtSegment = startFrame * frameOffset;
                    var eventEndWrtSegment = (i - 1) * frameOffset;

                    // Initialize the event.
                    var ev = new HarmonicEvent()
                    {
                        SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                        SegmentDurationSeconds = frameCount * converter.SecondsPerFrameStep,
                        Name = "Stacked harmonics",
                        ResultStartSeconds = segmentStartOffset.TotalSeconds + eventStartWrtSegment,
                        EventStartSeconds = segmentStartOffset.TotalSeconds + eventStartWrtSegment,
                        EventEndSeconds = segmentStartOffset.TotalSeconds + eventEndWrtSegment,
                        LowFrequencyHertz = minHz,
                        HighFrequencyHertz = maxHz,
                        Score = avScore,
                        ScoreRange = scoreRange,
                        HarmonicInterval = harmonicInterval,
                        //DecibelDetectionThreshold,
                    };

                    events.Add(ev);
                }
            }

            return events;
        }
    }
}