// <copyright file="HarmonicParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect the stacked harmonic components of a soundscape.
    /// This can also be used for recognizing the harmonics of non-biological sounds such as from turbines, motor-bikes, compressors, hi-revving motors, etc.
    /// </summary>
    [YamlTypeTag(typeof(HarmonicParameters))]
    public class HarmonicParameters : CommonParameters
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets or sets the dctThreshold.
        /// </summary>
        public double? DctThreshold { get; set; }

        /// <summary>
        /// Gets or sets the bottom bound of the gap between formants. Units are Hertz.
        /// </summary>
        public int? MinFormantGap { get; set; }

        /// <summary>
        /// Gets or sets the top bound of gap between formants. Units are Hertz.
        /// </summary>
        public int? MaxFormantGap { get; set; }

        /// <summary>
        /// Gets or sets a smoothing window.
        /// This is used to run a moving average filter along each of the frequency bins.
        /// It can help to smooth over discontinuous formants.
        /// If applied sensible values are 3, 5, or 7.
        /// </summary>
        public int SmoothingWindow { get; set; } = 0;

        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            HarmonicParameters hp,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var dctThreshold = hp.DctThreshold.Value;
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            double[] decibelMaxArray;
            double[] harmonicIntensityScores;

            (spectralEvents, decibelMaxArray, harmonicIntensityScores) = GetHarmonicEvents(
                                spectrogram,
                                hp.MinHertz.Value,
                                hp.MaxHertz.Value,
                                hp.SmoothingWindow,
                                decibelThreshold.Value,
                                dctThreshold,
                                hp.MinDuration.Value,
                                hp.MaxDuration.Value,
                                hp.MinFormantGap.Value,
                                hp.MaxFormantGap.Value,
                                segmentStartOffset);

            // prepare plot of resultant Harmonics decibel array.
            var plot1 = Plot.PreparePlot(decibelMaxArray, $"{profileName} (Harmonics:{decibelThreshold:F0}db)", decibelThreshold.Value);
            plots.Add(plot1);
            var plot2 = Plot.PreparePlot(harmonicIntensityScores, $"{profileName} (HarmonicScores:{dctThreshold:F0})", dctThreshold);
            plots.Add(plot2);

            return (spectralEvents, plots);
        }

        public static (List<EventCommon> SpectralEvents, double[] AmplitudeArray, double[] HarmonicIntensityScores) GetHarmonicEvents(
            SpectrogramStandard spectrogram,
            int minHz,
            int maxHz,
            int smoothingWindow,
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

            // create a unit converter
            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            // extract the sub-band of interest
            double[,] subMatrix = MatrixTools.Submatrix(spectrogram.Data, 0, minBin, frameCount - 1, maxBin);

            // DETECT HARMONICS in search band using the Xcorrelation-DCT method.
            var results = DetectHarmonicsInSpectrogramData(subMatrix, decibelThreshold, smoothingWindow);

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
            var harmonicEvents = ConvertScoreArray2HarmonicEvents(
                    spectrogram,
                    harmonicIntensityScores2,
                    dBArray,
                    converter,
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
        /// A METHOD TO DETECT a set of stacked HARMONICS/FORMANTS in the sub-band of a spectrogram.
        /// Developed for GenericRecognizer of harmonics.
        /// NOTE 1: This method assumes the matrix is derived from a spectrogram rotated so that the matrix rows are spectral columns of the spectrogram.
        /// NOTE 2: As of March 2020, this method averages the values in five adjacent frames. This is to reduce noise.
        ///         But it requires that the frequency of any potential formants is not changing rapidly.
        ///         A side-effect is that the edges of harmonic events become blurred.
        ///         This may not be suitable for detecting human speech. However can reduce the frame step.
        /// NOTE 3: This method assumes that the minimum  number of formants in a stack = 3.
        ///         This means that the first 4 values in the returned array of DCT coefficients are set = 0 (see below).
        /// </summary>
        /// <param name="m">data matrix derived from the subband of a spectrogram.</param>
        /// <param name="xThreshold">Minimum acceptable value to be considered part of a harmonic.</param>
        /// <returns>three arrays: dBArray, intensity, maxIndexArray.</returns>
        public static Tuple<double[], double[], int[]> DetectHarmonicsInSpectrogramData(double[,] m, double xThreshold, int smoothingWindow)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            var binCount = m.GetLength(1);

            //set up the cosine coefficients
            double[,] cosines = DctMethods.Cosines(binCount, binCount);

            // set up time-frame arrays to store decibels, formant intensity and max index.
            var dBArray = new double[rowCount];
            var intensity = new double[rowCount];
            var maxIndexArray = new int[rowCount];

            // Run a moving average filter along each frequency bin.
            // This may help to fill noise gaps in formants. Ignore values <3.
            if (smoothingWindow > 2)
            {
                m = MatrixTools.SmoothColumns(m, smoothingWindow);
            }

            // for all time-frames or spectra
            for (int t = 0; t < rowCount; t++)
            {
                var avFrame = MatrixTools.GetRow(m, t);

                // ignore frame if its maximum decibel value is below the threshold.
                double maxValue = avFrame.Max();
                dBArray[t] = maxValue;
                if (maxValue < xThreshold)
                {
                    continue;
                }

                // do autocross-correlation prior to doing the DCT.
                double[] xr = AutoAndCrossCorrelation.AutoCrossCorr(avFrame);

                // xr has twice length of frame and is symmetrical. Require only first half.
                double[] normXr = new double[colCount];
                for (int i = 0; i < colCount; i++)
                {
                    // Typically normalise the xcorr values for overlap count.
                    // i.e. normXr[i] = xr[i] / (colCount - i);
                    // But for harmonics, this introduces too much noise - need to give less weight to the less overlapped values.
                    // Therefore just normalise by dividing values by the first, so first value = 1.
                    normXr[i] = xr[i] / xr[0];
                }

                // fit the x-correlation array to a line to remove first order trend.
                // This will help in detecting the correct maximum DCT coefficient.
                var xValues = new double[normXr.Length];
                for (int j = 0; j < xValues.Length; j++)
                {
                    xValues[j] = j;
                }

                // do linear detrend of the vector of coefficients.
                // get the line of best fit and subtract to get deviation from the line.
                Tuple<double, double> values = MathNet.Numerics.Fit.Line(xValues, normXr);
                var intercept = values.Item1;
                var slope = values.Item2;
                for (int j = 0; j < xValues.Length; j++)
                {
                    var lineValue = (slope * j) + intercept;
                    normXr[j] -= lineValue;
                }

                // now do DCT across the detrended auto-cross-correlation
                // set the first four values in the returned DCT coefficients to 0.
                // We require a minimum of three formants, that is, two harmonic intervals.
                int lowerDctBound = 4;
                var dctCoefficients = DctMethods.DoDct(normXr, cosines, lowerDctBound);
                int indexOfMaxValue = DataTools.GetMaxIndex(dctCoefficients);
                intensity[t] = dctCoefficients[indexOfMaxValue];
                maxIndexArray[t] = indexOfMaxValue;
            }

            return Tuple.Create(dBArray, intensity, maxIndexArray);
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
        public static List<EventCommon> ConvertScoreArray2HarmonicEvents(
            SpectrogramStandard spectrogram,
            double[] scores,
            double[] dBArray,
            UnitConverters converter,
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
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            int startFrame = 0;
            int frameCount = scores.Length;

            // use this to calculate a normalised score between 0 - 1.0
            double maxPossibleScore = 5 * scoreThreshold;
            var scoreRange = new Interval<double>(0, maxPossibleScore);

            var events = new List<EventCommon>();

            int rejectionCount = 0;
            double rejectedDuration = 0.0;

            // pass over all time frames
            for (int i = 0; i < frameCount; i++)
            {
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startFrame = i;
                }
                else // check for the end of an event
                if (isHit && scores[i] <= scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    int eventFrameLength = i - startFrame;
                    double duration = eventFrameLength * frameOffset;

                    if (duration < minDuration || duration > maxDuration)
                    {
                        //skip events with invalid duration
                        //var message = $"Harmonic event rejected - {duration} duration not in {minDuration}..{maxDuration}";
                        //Log.Info(message);

                        rejectionCount++;
                        rejectedDuration += duration;
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
                    avScore /= eventFrameLength;
                    avIndex /= eventFrameLength;
                    double freqBinGap = 2 * bandBinCount / avIndex;
                    double harmonicInterval = freqBinGap * freqBinWidth;

                    // calculate start and end time of this event relative to start of segment.
                    var eventStartWrtSegment = startFrame * frameOffset;
                    var eventEndWrtSegment = eventStartWrtSegment + duration;

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

            var message = $"Harmonic events REJECTED = {rejectionCount}; having av duration = {(rejectedDuration / rejectionCount):F3} seconds. Accepted durations in {minDuration}..{maxDuration}";
            Log.Info(message);

            return events;
        }
    }
}