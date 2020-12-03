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

namespace AnalysisPrograms.Recognizers.Frogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// This is a frog recognizer based on the "ribit" or "washboard" template
    /// It detects ribit type calls by extracting three features: dominant frequency, pulse rate and pulse train duration.
    ///
    /// This type recognizer was first developed for the Canetoad and has been duplicated with modification for other frogs
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// </summary>
    public class LitoriaRothii : RecognizerBase
    {
        public override string Description => "[ALPHA/EMBRYONIC] Detects acoustic events of Litoria rothii.";

        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaRothii";

        public override string CommonName => "Roth's tree frog";

        public override Status Status =>  Status.InDevelopment;

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
        public override RecognizerResults Recognize(AudioRecording recording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            const int frameSize = 256;
            const double windowOverlap = 0.0;

            double noiseReductionParameter = configuration.GetDoubleOrNull("SeverityOfNoiseRemoval") ?? 2.0;

            int minHz = configuration.GetInt(AnalysisKeys.MinHz);
            int maxHz = configuration.GetInt(AnalysisKeys.MaxHz);

            // ignore oscillations below this threshold freq
            int minOscilFreq = configuration.GetInt(AnalysisKeys.MinOscilFreq);

            // ignore oscillations above this threshold freq
            int maxOscilFreq = configuration.GetInt(AnalysisKeys.MaxOscilFreq);

            // duration of DCT in seconds
            //double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = configuration.GetDouble(AnalysisKeys.DctThreshold);

            // min duration of event in seconds
            double minDuration = configuration.GetDouble(AnalysisKeys.MinDuration);

            // max duration of event in seconds
            double maxDuration = configuration.GetDouble(AnalysisKeys.MaxDuration);

            // min score for an acceptable event
            double decibelThreshold = configuration.GetDouble(AnalysisKeys.DecibelThreshold);

            // min score for an acceptable event
            double eventThreshold = configuration.GetDouble(AnalysisKeys.EventThreshold);

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

            double[] amplitudeArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, rowCount - 1, maxBin);

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
                Oscillations2014.GetOscillationUsingDct(subArray, framesPerSecond, cosines, out var oscilFreq, out var period, out var intensity);
                bool periodWithinBounds = period > minPeriod && period < maxPeriod;

                if (!periodWithinBounds)
                {
                    continue;
                }

                if (intensity < dctThreshold)
                {
                    continue;
                }

                //lay down score for sample length
                for (int j = 0; j < dctLength; j++)
                {
                    if (dctScores[i + j] < intensity && lowPassFilteredSignal[i + j] > decibelThreshold)
                    {
                        dctScores[i + j] = intensity;
                    }
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
                DataTools.Normalise(amplitudeArray, decibelThreshold, out var normalisedScores, out var normalisedThreshold);
                var ampltdPlot = new Plot("amplitude", normalisedScores, normalisedThreshold);
                DataTools.Normalise(highPassFilteredSignal, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var demeanedPlot = new Plot("Hi Pass", normalisedScores, normalisedThreshold);

                DataTools.Normalise(lowPassFilteredSignal, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var lowPassPlot = new Plot("Low Pass", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { ampltdPlot, lowPassPlot, demeanedPlot, plot };
                Image debugImage = SpectrogramTools.GetSonogramPlusCharts(sonogram, acousticEvents, debugPlots, null);
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
    }
}