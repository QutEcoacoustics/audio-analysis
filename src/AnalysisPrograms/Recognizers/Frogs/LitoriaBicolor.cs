// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaBicolor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   AKA: The bloody canetoad
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
    using Path = System.IO.Path;

    /// <summary>
    /// To call this LitoriaBicolor recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// This frog recognizer is based on the "kek-kek" recognizer for the Lewin's Rail
    /// It looks for synchronous oscillations in two frequency bands
    /// This recognizer was first developed for Jenny ???, a Masters student around 2007.
    /// It has been updated in October 2016 to become one of the new RecognizerBase recognizers.
    /// however the Correlation technique used for the Lewins Rail did not work because the ossilations in the upper and lower freq bands are not correlated.
    /// Instead measure the oscillations in the upper and lower bands independently.
    ///
    /// </summary>

    public class LitoriaBicolor : RecognizerBase
    {
        public override string Description => "Detects acoustic events of Litoria bicolor";

        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaBicolor";

        public override string CommonName => "Northern dwarf tree frog";

        public override Status Status => Status.InDevelopment;

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

        // OTHER CONSTANTS
        //private const string ImageViewer = @"C:\Windows\system32\mspaint.exe";

        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        public override RecognizerResults Recognize(AudioRecording recording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            var recognizerConfig = new LitoriaBicolorConfig();
            recognizerConfig.ReadConfigFile(configuration);

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            TimeSpan recordingDuration = recording.WavReader.Time;

            //// ignore oscillations below this threshold freq
            //int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];
            //// ignore oscillations above this threshold freq
            int maxOscilRate = (int)Math.Ceiling(1 / recognizerConfig.MinPeriod);

            // this default framesize seems to work
            const int frameSize = 128;
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                maxOscilRate);

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,

                //set default values - ignore those set by user
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,

                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD"),
            };

            //#############################################################################################################################################
            //DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var results = Analysis(recording, sonoConfig, recognizerConfig, MainEntry.InDEBUG, segmentStartOffset);

            //######################################################################

            if (results == null)
            {
                return null; //nothing to process
            }

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scoreArray = results.Item3;
            var predictedEvents = results.Item4;
            var debugImage = results.Item5;

            //#############################################################################################################################################

            var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recording.BaseName), this.SpeciesName, "png", "DebugSpectrogram"));
            debugImage.Save(debugPath.FullName);

            // Prune events here if erquired i.e. remove those below threshold score if this not already done. See other recognizers.
            foreach (AcousticEvent ae in predictedEvents)
            {
                // add additional info
                ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                ae.SpeciesName = recognizerConfig.SpeciesName;
                ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
            }

            var plot = new Plot(this.DisplayName, scoreArray, recognizerConfig.EventThreshold);
            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = predictedEvents,
            };
        }

        /// <summary>
        /// THE KEY ANALYSIS METHOD.
        /// </summary>
        public static Tuple<BaseSonogram, double[,], double[], List<AcousticEvent>, Image> Analysis(
            AudioRecording recording,
            SonogramConfig sonoConfig,
            LitoriaBicolorConfig lbConfig,
            bool drawDebugImage,
            TimeSpan segmentStartOffset)
        {
            double decibelThreshold = lbConfig.DecibelThreshold;   //dB
            double intensityThreshold = lbConfig.IntensityThreshold;

            //double eventThreshold = lbConfig.EventThreshold; //in 0-1

            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            //i: MAKE SONOGRAM
            //TimeSpan tsRecordingtDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = freqBinWidth;

            // duration of DCT in seconds - want it to be about 3X or 4X the expected maximum period
            double dctDuration = 3 * lbConfig.MaxPeriod;

            // duration of DCT in frames
            int dctLength = (int)Math.Round(framesPerSecond * dctDuration);

            // set up the cosine coefficients
            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength);

            int upperBandMinBin = (int)Math.Round(lbConfig.UpperBandMinHz / freqBinWidth) + 1;
            int upperBandMaxBin = (int)Math.Round(lbConfig.UpperBandMaxHz / freqBinWidth) + 1;
            int lowerBandMinBin = (int)Math.Round(lbConfig.LowerBandMinHz / freqBinWidth) + 1;
            int lowerBandMaxBin = (int)Math.Round(lbConfig.LowerBandMaxHz / freqBinWidth) + 1;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, lowerBandMinBin, rowCount - 1, lowerBandMaxBin);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, upperBandMinBin, rowCount - 1, upperBandMaxBin);

            //lowerArray = DataTools.filterMovingAverage(lowerArray, 3);
            //upperArray = DataTools.filterMovingAverage(upperArray, 3);

            double[] amplitudeScores = DataTools.SumMinusDifference(lowerArray, upperArray);
            double[] differenceScores = DspFilters.PreEmphasis(amplitudeScores, 1.0);

            // Could smooth here rather than above. Above seemed slightly better?
            amplitudeScores = DataTools.filterMovingAverage(amplitudeScores, 7);
            differenceScores = DataTools.filterMovingAverage(differenceScores, 7);

            //iii: CONVERT decibel sum-diff SCORES TO ACOUSTIC EVENTS
            var predictedEvents = AcousticEvent.ConvertScoreArray2Events(
                amplitudeScores,
                lbConfig.LowerBandMinHz,
                lbConfig.UpperBandMaxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                decibelThreshold,
                lbConfig.MinDuration,
                lbConfig.MaxDuration,
                segmentStartOffset);

            for (int i = 0; i < differenceScores.Length; i++)
            {
                if (differenceScores[i] < 1.0)
                {
                    differenceScores[i] = 0.0;
                }
            }

            // init the score array
            double[] scores = new double[rowCount];

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            // var hits = new double[rowCount, colCount];
            double[,] hits = null;

            // init confirmed events
            var confirmedEvents = new List<AcousticEvent>();

            // add names into the returned events
            foreach (var ae in predictedEvents)
            {
                //rowtop,  rowWidth
                int eventStart = ae.Oblong.RowTop;
                int eventWidth = ae.Oblong.RowWidth;
                int step = 2;
                double maximumIntensity = 0.0;

                // scan the event to get oscillation period and intensity
                for (int i = eventStart - (dctLength / 2); i < eventStart + eventWidth - (dctLength / 2); i += step)
                {
                    // Look for oscillations in the difference array
                    double[] differenceArray = DataTools.Subarray(differenceScores, i, dctLength);
                    Oscillations2014.GetOscillationUsingDct(differenceArray, framesPerSecond, cosines, out var oscilFreq, out var period, out var intensity);

                    bool periodWithinBounds = period > lbConfig.MinPeriod && period < lbConfig.MaxPeriod;

                    //Console.WriteLine($"step={i}    period={period:f4}");

                    if (!periodWithinBounds)
                    {
                        continue;
                    }

                    // lay down score for sample length
                    for (int j = 0; j < dctLength; j++)
                    {
                        if (scores[i + j] < intensity)
                        {
                            scores[i + j] = intensity;
                        }
                    }

                    if (maximumIntensity < intensity)
                    {
                        maximumIntensity = intensity;
                    }
                }

                // add abbreviatedSpeciesName into event
                if (maximumIntensity >= intensityThreshold)
                {
                    ae.Name = "L.b";
                    ae.Score_MaxInEvent = maximumIntensity;
                    confirmedEvents.Add(ae);
                }
            }

            //######################################################################

            // calculate the cosine similarity scores
            var scorePlot = new Plot(lbConfig.SpeciesName, scores, intensityThreshold);

            //DEBUG IMAGE this recognizer only. MUST set false for deployment.
            Image debugImage = null;
            if (drawDebugImage)
            {
                // display a variety of debug score arrays

                //DataTools.Normalise(scores, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var debugPlot = new Plot("Score", normalisedScores, normalisedThreshold);
                //DataTools.Normalise(upperArray, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var upperPlot = new Plot("Upper", normalisedScores, normalisedThreshold);
                //DataTools.Normalise(lowerArray, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var lowerPlot = new Plot("Lower", normalisedScores, normalisedThreshold);
                DataTools.Normalise(amplitudeScores, decibelThreshold, out var normalisedScores, out var normalisedThreshold);
                var sumDiffPlot = new Plot("SumMinusDifference", normalisedScores, normalisedThreshold);
                DataTools.Normalise(differenceScores, 3.0, out normalisedScores, out normalisedThreshold);
                var differencePlot = new Plot("Difference", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scorePlot, sumDiffPlot, differencePlot };

                // other debug plots
                //var debugPlots = new List<Plot> { scorePlot, upperPlot, lowerPlot, sumDiffPlot, differencePlot };
                debugImage = DisplayDebugImage(sonogram, confirmedEvents, debugPlots, hits);
            }

            // return new sonogram because it makes for more easy interpretation of the image
            var returnSonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = 512,
                WindowOverlap = 0,

                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD"),
            };
            BaseSonogram returnSonogram = new SpectrogramStandard(returnSonoConfig, recording.WavReader);
            return Tuple.Create(returnSonogram, hits, scores, confirmedEvents, debugImage);
        } //Analysis()

        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines, doMelScale: false));

            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (var plot in scores)
                {
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            if (hits != null)
            {
                image.OverlayRainbowTransparency(hits);
            }

            if (events.Count > 0)
            {
                foreach (var ev in events) // set colour for the events
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }

                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }
    } //end class LitoriaBicolor.cs

    public class LitoriaBicolorConfig
    {
        public string AnalysisName { get; set; }

        public string SpeciesName { get; set; }

        public string AbbreviatedSpeciesName { get; set; }

        public int UpperBandMinHz { get; set; }

        public int UpperBandMaxHz { get; set; }

        public int LowerBandMinHz { get; set; }

        public int LowerBandMaxHz { get; set; }

        public double MinPeriod { get; set; }

        public double MaxPeriod { get; set; }

        public double MinDuration { get; set; }

        public double MaxDuration { get; set; }

        public double IntensityThreshold { get; set; }

        public double DecibelThreshold { get; set; }

        public double EventThreshold { get; set; }

        internal void ReadConfigFile(Config configuration)
        {
            // common properties
            this.AnalysisName = configuration[AnalysisKeys.AnalysisName] ?? "<no name>";
            this.SpeciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            this.AbbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            this.UpperBandMaxHz = configuration.GetInt("UpperFreqBandTop");
            this.UpperBandMinHz = configuration.GetInt("UpperFreqBandBottom");
            this.LowerBandMaxHz = configuration.GetInt("LowerFreqBandTop");
            this.LowerBandMinHz = configuration.GetInt("LowerFreqBandBottom");

            // Periods and Oscillations
            this.MinPeriod = configuration.GetDouble(AnalysisKeys.MinPeriodicity); //: 0.18
            this.MaxPeriod = configuration.GetDouble(AnalysisKeys.MaxPeriodicity); //: 0.25

            // minimum duration in seconds of an event
            this.MinDuration = configuration.GetDouble(AnalysisKeys.MinDuration); //:3

            // maximum duration in seconds of an event
            this.MaxDuration = configuration.GetDouble(AnalysisKeys.MaxDuration); //: 15

            // minimum acceptable value of a DCT coefficient
            this.IntensityThreshold = configuration.GetDoubleOrNull(AnalysisKeys.IntensityThreshold) ?? 0.4;
            this.DecibelThreshold = configuration.GetDoubleOrNull(AnalysisKeys.DecibelThreshold) ?? 3.0;
            this.EventThreshold = configuration.GetDoubleOrNull(AnalysisKeys.EventThreshold) ?? 0.2;
        } // ReadConfigFile()
    } // class LitoriaBicolorConfig
}