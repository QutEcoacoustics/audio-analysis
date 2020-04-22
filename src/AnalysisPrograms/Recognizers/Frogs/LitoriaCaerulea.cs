// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaCaerulea.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   AKA: The Common Green Tree Frog
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Frogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
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
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    /// <summary>
    /// LitoriaCaerulea AKA: The Common Green Tree Frog
    /// This is a frog recognizer based on the "croak" or "honk" template
    /// It detects croak type calls by extracting three features: croak bandwidth, dominant frequency, croak duration.
    /// It may also look for trains of repeated croaks and set a minimum pulse train duration.
    ///
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    public class LitoriaCaerulea : RecognizerBase
    {
        public override string Description => "[ALPHA/EMBRYONIC] Detects acoustic events of Litoria caerulea";

        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaCaerulea";

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
        public override RecognizerResults Recognize(AudioRecording recording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            var recognizerConfig = new LitoriaCaeruleaConfig();
            recognizerConfig.ReadConfigFile(configuration);

            // common properties
            string speciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            string abbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // BETTER TO SET THESE. IGNORE USER!
            // This framesize is large because the oscillation we wish to detect is due to repeated croaks
            // having an interval of about 0.6 seconds. The overlap is also required to give smooth oscillation.
            const int frameSize = 2048;
            const double windowOverlap = 0.5;

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,

                // use the default HAMMING window
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),

                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.None
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
            };

            TimeSpan recordingDuration = recording.WavReader.Time;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = sr / (sonoConfig.WindowSize * (1 - windowOverlap));

            //int dominantFreqBin = (int)Math.Round(recognizerConfig.DominantFreq / freqBinWidth) + 1;
            int minBin = (int)Math.Round(recognizerConfig.MinHz / freqBinWidth) + 1;
            int maxBin = (int)Math.Round(recognizerConfig.MaxHz / freqBinWidth) + 1;
            var decibelThreshold = 9.0;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            int rowCount = sonogram.Data.GetLength(0);

            // get the freq band as set by min and max Herz
            var frogBand = MatrixTools.Submatrix(sonogram.Data, 0, minBin, rowCount - 1, maxBin);

            // Now look for spectral maxima. For L.caerulea, the max should lie around 1100Hz +/-150 Hz.
            // Skip over spectra where maximum is not in correct location.
            int buffer = 150;
            var croakScoreArray = new double[rowCount];
            var hzAtTopOfTopBand = recognizerConfig.DominantFreq + buffer;
            var hzAtBotOfTopBand = recognizerConfig.DominantFreq - buffer;
            var binAtTopOfTopBand = (int)Math.Round((hzAtTopOfTopBand - recognizerConfig.MinHz) / freqBinWidth);
            var binAtBotOfTopBand = (int)Math.Round((hzAtBotOfTopBand - recognizerConfig.MinHz) / freqBinWidth);

            // scan the frog band and get the decibel value of those spectra which have their maximum within the correct subband.
            for (int x = 0; x < rowCount; x++)
            {
                //extract spectrum
                var spectrum = MatrixTools.GetRow(frogBand, x);
                int maxIndex = DataTools.GetMaxIndex(spectrum);
                if (spectrum[maxIndex] < decibelThreshold)
                {
                    continue;
                }

                if (maxIndex < binAtTopOfTopBand && maxIndex > binAtBotOfTopBand)
                {
                    croakScoreArray[x] = spectrum[maxIndex];
                }
            }

            // Perpare a normalised plot for later display with spectrogram
            DataTools.Normalise(croakScoreArray, decibelThreshold, out var normalisedScores, out var normalisedThreshold);
            var text1 = string.Format($"Croak scores (threshold={decibelThreshold})");
            var croakPlot1 = new Plot(text1, normalisedScores, normalisedThreshold);

            // extract potential croak events from the array of croak candidate
            var croakEvents = AcousticEvent.ConvertScoreArray2Events(
                croakScoreArray,
                recognizerConfig.MinHz,
                recognizerConfig.MaxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                recognizerConfig.EventThreshold,
                recognizerConfig.MinCroakDuration,
                recognizerConfig.MaxCroakDuration,
                segmentStartOffset);

            // add necesary info into the candidate events
            var prunedEvents = new List<AcousticEvent>();
            foreach (var ae in croakEvents)
            {
                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                prunedEvents.Add(ae);
            }

            // With those events that survive the above Array2Events process, we now extract a new array croak scores
            croakScoreArray = AcousticEvent.ExtractScoreArrayFromEvents(prunedEvents, rowCount, recognizerConfig.AbbreviatedSpeciesName);
            DataTools.Normalise(croakScoreArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
            var text2 = string.Format($"Croak events (threshold={decibelThreshold})");
            var croakPlot2 = new Plot(text2, normalisedScores, normalisedThreshold);

            // Look for oscillations in the difference array
            // duration of DCT in seconds
            //croakScoreArray = DataTools.filterMovingAverageOdd(croakScoreArray, 5);
            double dctDuration = recognizerConfig.DctDuration;

            // minimum acceptable value of a DCT coefficient
            double dctThreshold = recognizerConfig.DctThreshold;
            double minOscRate = 1 / recognizerConfig.MaxPeriod;
            double maxOscRate = 1 / recognizerConfig.MinPeriod;
            Oscillations2019.DetectOscillations(croakScoreArray, framesPerSecond, decibelThreshold, dctDuration, minOscRate, maxOscRate, dctThreshold, out double[] dctScores, out double[] oscFreq);

            // ######################################################################
            // ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var events = AcousticEvent.ConvertScoreArray2Events(
                dctScores,
                recognizerConfig.MinHz,
                recognizerConfig.MaxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                recognizerConfig.EventThreshold,
                recognizerConfig.MinDuration,
                recognizerConfig.MaxDuration,
                segmentStartOffset);

            double[,] hits = null;
            prunedEvents = new List<AcousticEvent>();
            foreach (var ae in events)
            {
                // add additional info
                ae.SpeciesName = speciesName;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                prunedEvents.Add(ae);
            }

            // do a recognizer test.
            if (MainEntry.InDEBUG)
            {
                //TestTools.RecognizerScoresTest(scores, new FileInfo(recording.FilePath));
                //AcousticEvent.TestToCompareEvents(prunedEvents, new FileInfo(recording.FilePath));
            }

            var scoresPlot = new Plot(this.DisplayName, dctScores, recognizerConfig.EventThreshold);

            if (true)
            {
                // display a variety of debug score arrays
                // calculate amplitude at location
                double[] amplitudeArray = MatrixTools.SumRows(frogBand);
                DataTools.Normalise(amplitudeArray, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var amplPlot = new Plot("Band amplitude", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scoresPlot, croakPlot2, croakPlot1, amplPlot };

                // NOTE: This DrawDebugImage() method can be over-written in this class.
                var debugImage = DrawDebugImage(sonogram, prunedEvents, debugPlots, hits);
                var debugPath = FilenameHelpers.AnalysisResultPath(outputDirectory, recording.BaseName, this.SpeciesName, "png", "DebugSpectrogram");
                debugImage.Save(debugPath);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = scoresPlot.AsList(),
                Events = prunedEvents,

                //Events = events
            };
        }
    }

    internal class LitoriaCaeruleaConfig
    {
        public string AnalysisName { get; set; }

        public string SpeciesName { get; set; }

        public string AbbreviatedSpeciesName { get; set; }

        public int DominantFreq { get; set; }

        public int MinHz { get; set; }

        public int MaxHz { get; set; }

        public double DctDuration { get; set; }

        public double DctThreshold { get; set; }

        public double MinCroakDuration { get; set; }

        public double MaxCroakDuration { get; set; }

        public double MinPeriod { get; set; }

        public double MaxPeriod { get; set; }

        public double MinDuration { get; set; }

        public double MaxDuration { get; set; }

        public double EventThreshold { get; set; }

        internal void ReadConfigFile(Config configuration)
        {
            // common properties
            this.AnalysisName = configuration[AnalysisKeys.AnalysisName] ?? "<no name>";
            this.SpeciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            this.AbbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // frequency band of the call
            this.MinHz = configuration.GetInt(AnalysisKeys.MinHz);
            this.MaxHz = configuration.GetInt(AnalysisKeys.MaxHz);
            this.DominantFreq = configuration.GetInt(AnalysisKeys.DominantFrequency);

            // duration of DCT in seconds
            this.DctDuration = configuration.GetDouble(AnalysisKeys.DctDuration);

            // minimum acceptable value of a DCT coefficient
            this.DctThreshold = configuration.GetDouble(AnalysisKeys.DctThreshold);

            // Periods and Oscillations
            this.MinPeriod = configuration.GetDouble(AnalysisKeys.MinPeriodicity);
            this.MaxPeriod = configuration.GetDouble(AnalysisKeys.MaxPeriodicity);

            // minimum duration in seconds of an event
            this.MinDuration = configuration.GetDouble(AnalysisKeys.MinDuration);

            // maximum duration in seconds of an event
            this.MaxDuration = configuration.GetDouble(AnalysisKeys.MaxDuration);

            // min and max duration of a single croak event in seconds
            this.MinCroakDuration = configuration.GetDouble("MinCroakDuration");
            this.MaxCroakDuration = configuration.GetDouble("MaxCroakDuration");

            // min score for an acceptable event
            this.EventThreshold = configuration.GetDouble(AnalysisKeys.EventThreshold);
        }
    } // Config class
}