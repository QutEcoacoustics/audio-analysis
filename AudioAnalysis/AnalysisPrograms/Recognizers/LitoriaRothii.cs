// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaRothii.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This is a frog recognizer based on the "trill", "ribit" or "washboard" template
//   It detects trill type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
//   This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
//   To call this recognizer, the first command line argument must be "EventRecognizer".
//   Alternatively, this recognizer can be called via the MultiRecognizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    //using System.Linq;
    using System.Reflection;
    //using System.Text;

    using Acoustics.Shared;
    //using Acoustics.Tools.Wav;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    ///
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    class LitoriaRothii : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaRothii";

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
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            const int frameSize = 256;
            const double windowOverlap = 0.0;

            double noiseReductionParameter = (double?)configuration["SeverityOfNoiseRemoval"] ?? 2.0;

            int minHz = (int)configuration[AnalysisKeys.MinHz];
            int maxHz = (int)configuration[AnalysisKeys.MaxHz];

            // ignore oscillations below this threshold freq
            int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];
            // ignore oscillations above this threshold freq
            int maxOscilFreq = (int)configuration[AnalysisKeys.MaxOscilFreq];

            // duration of DCT in seconds
            //double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            // min duration of event in seconds
            double minDuration = (double)configuration[AnalysisKeys.MinDuration];

            // max duration of event in seconds
            double maxDuration = (double)configuration[AnalysisKeys.MaxDuration];

            // min score for an acceptable event
            double decibelThreshold = (double)configuration[AnalysisKeys.DecibelThreshold];

            // min score for an acceptable event
            double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = noiseReductionParameter,
            };

            var recordingDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(maxHz / freqBinWidth) + 1;

            // duration of DCT in seconds - want it to be about 3X or 4X the expected maximum period
            double framesPerSecond = freqBinWidth;
            double minPeriod = 1 / (double)maxOscilFreq;
            double maxPeriod = 1 / (double)minOscilFreq;
            double dctDuration = 5 * maxPeriod;
            // duration of DCT in frames
            int dctLength = (int)Math.Round(framesPerSecond * dctDuration);
            // set up the cosine coefficients
            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength);

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            //int colCount = sonogram.Data.GetLength(1);

            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxBin);
            bool[] peakArray = new bool[rowCount];

            // remove baseline from amplitude array
            var highPassFilteredSignal = DspFilters.SubtractBaseline(amplitudeArray, 7);
            // remove hi freq content from amplitude array
            var lowPassFilteredSignal = DataTools.filterMovingAverageOdd(amplitudeArray, 11);

            var dctScores = new double[highPassFilteredSignal.Length];
            const int step = 2;
            for (int i = dctLength; i < highPassFilteredSignal.Length - dctLength; i += step)
            {
                if (highPassFilteredSignal[i] < decibelThreshold)
                {
                    continue;
                }
                double[] subArray = DataTools.Subarray(highPassFilteredSignal, i, dctLength);
                // Look for oscillations in the highPassFilteredSignal
                double oscilFreq;
                double period;
                double intensity;
                Oscillations2014.GetOscillation(subArray, framesPerSecond, cosines, out oscilFreq, out period, out intensity);
                bool periodWithinBounds = (period > minPeriod) && (period < maxPeriod);

                if (!periodWithinBounds) continue;
                if (intensity < dctThreshold) continue;

                for (int j = 0; j < dctLength; j++) //lay down score for sample length
                {
                    if ((dctScores[i + j] < intensity) && (lowPassFilteredSignal[i + j] > decibelThreshold))
                        dctScores[i + j] = intensity;
                }
            }

            //iii: CONVERT decibel sum-diff SCORES TO ACOUSTIC EVENTS
            var acousticEvents = AcousticEvent.ConvertScoreArray2Events(
                dctScores,
                minHz,
                maxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                eventThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);

            // ######################################################################
            acousticEvents.ForEach(ae =>
            {
                ae.SpeciesName = speciesName;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.Name = abbreviatedSpeciesName;
            });

            var plot = new Plot(this.DisplayName, dctScores, eventThreshold);
            var plots = new List<Plot> { plot };

            // DEBUG IMAGE this recognizer only. MUST set false for deployment.
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var ampltdPlot = new Plot("amplitude", normalisedScores, normalisedThreshold);
                DataTools.Normalise(highPassFilteredSignal, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var demeanedPlot = new Plot("Hi Pass", normalisedScores, normalisedThreshold);

                DataTools.Normalise(lowPassFilteredSignal, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var lowPassPlot = new Plot("Low Pass", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { ampltdPlot, lowPassPlot, demeanedPlot, plot };
                Image debugImage = DisplayDebugImage(sonogram, acousticEvents, debugPlots, null);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recording.BaseName), this.Identifier, "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = null,
                Plots = plots,
                Events = acousticEvents,
            };

        }

        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1kHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);

            if (events.Count > 0)
            {
                foreach (AcousticEvent ev in events) // set colour for the events
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }
                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }
            return image.GetImage();
        }

    }
}
