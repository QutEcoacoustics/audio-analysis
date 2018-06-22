// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LewiniaPectoralis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   AKA: The Lewin's Rail
// To call this recognizer, the first command line argument must be "EventRecognizer".
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using Base;
    using log4net;
    using TowseyLibrary;
    using static Acoustics.Shared.FilenameHelpers;

    /// <summary>
    /// AKA: Lewin's Rail
    /// This call recognizer depends on an oscillation recognizer picking up the Kek-kek repeated at a period of 200ms
    ///
    /// This recognizer was first developed around 2007 for Masters student, Jenny Gibson, and her supervisor, Ian Williamson.
    /// It was updated in October 2016 to become one of the new recognizers derived from RecognizerBase.
    ///
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>
    public class LewiniaPectoralis : RecognizerBase
    {
        public override string Description => "[BETA/EXPERIMENTAL] Detects acoustic events of Lewin's Rail";

        public override string Author => "Towsey";

        public override string SpeciesName => "LewiniaPectoralis";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private const bool DoRecognizerTest = true;

        // Return a DEBUG IMAGE this recognizer only. MUST set false for deployment.
        //public bool returnDebugImage = false;
        public bool ReturnDebugImage = MainEntry.InDEBUG;

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
        public override RecognizerResults Recognize(
            AudioRecording recording,
            Config configuration,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            //string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            //string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // this default framesize seems to work for Lewin's Rail
            const int frameSize = 512;

            // DO NOT SET windowOverlap. Calculate it below.

            if (imageWidth == null)
            {
                throw new ArgumentNullException(nameof(imageWidth));
            }

            // check the sample rate. Must be 22050
            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            TimeSpan recordingDuration = recording.WavReader.Time;

            // check for the profiles in the config file
            bool hasProfiles = ConfigFile.HasProfiles(configuration);
            if (!hasProfiles)
            {
                throw new ConfigFileException("The Config file for L.pectoralis must contain a profiles object.");
            }

            // get the profile names
            string[] profileNames = ConfigFile.GetProfileNames(configuration);
            var recognizerConfig = new LewinsRailConfig();
            var prunedEvents = new List<AcousticEvent>();
            var plots = new List<Plot>();
            BaseSonogram sonogram = null;

            // cycle through the profiles and analyse recording using each of them
            foreach (var name in profileNames)
            {
                Log.Debug($"Reading profile <{name}>.");
                recognizerConfig.ReadConfigFile(configuration, name);

                // ignore oscillations above this threshold freq
                int maxOscilRate = (int)Math.Ceiling(1 / recognizerConfig.MinPeriod);

                // calculate frame overlap and ignor any user inut.
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
                var results = Analysis(recording, sonoConfig, recognizerConfig, this.ReturnDebugImage, segmentStartOffset);

                // ######################################################################

                if (results == null)
                {
                    return null; //nothing to process
                }

                sonogram = results.Item1;

                //var hits = results.Item2;
                var scoreArray = results.Item3;
                var predictedEvents = results.Item4;
                var debugImage = results.Item5;

                //#############################################################################################################################################

                if (debugImage == null)
                {
                    Log.Debug("DebugImage is null, not writing file");
                }
                else if (MainEntry.InDEBUG)
                {
                    var imageName = AnalysisResultName(recording.BaseName, this.SpeciesName, "png", "DebugSpectrogram");
                    var debugPath = outputDirectory.Combine(imageName);
                    debugImage.Save(debugPath.FullName);
                }

                foreach (var ae in predictedEvents)
                {
                    // add additional info
                    if (!(ae.Score > recognizerConfig.EventThreshold))
                    {
                        continue;
                    }

                    ae.Name = recognizerConfig.AbbreviatedSpeciesName;
                    ae.SpeciesName = recognizerConfig.SpeciesName;
                    ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                    ae.SegmentDurationSeconds = recordingDuration.TotalSeconds;
                    prunedEvents.Add(ae);
                }

                // do a recognizer TEST.
                if (false)
                {
                    var testDir = new DirectoryInfo(outputDirectory.Parent.Parent.FullName);
                    TestTools.RecognizerScoresTest(recording.BaseName, testDir, recognizerConfig.AnalysisName, scoreArray);
                    AcousticEvent.TestToCompareEvents(recording.BaseName, testDir, recognizerConfig.AnalysisName, predictedEvents);
                }

                // increase very low scores
                for (int j = 0; j < scoreArray.Length; j++)
                {
                    scoreArray[j] *= 4;
                    if (scoreArray[j] > 1.0)
                    {
                        scoreArray[j] = 1.0;
                    }
                }

                var plot = new Plot(this.DisplayName, scoreArray, recognizerConfig.EventThreshold);
                plots.Add(plot);
            }

            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = null,
                Plots = plots,
                Events = prunedEvents,
            };
        }

        /// <summary>
        /// THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="sonoConfig"></param>
        /// <param name="lrConfig"></param>
        /// <param name="returnDebugImage"></param>
        /// <param name="segmentStartOffset"></param>
        /// <returns></returns>
        private static Tuple<BaseSonogram, double[,], double[], List<AcousticEvent>, Image> Analysis(
            AudioRecording recording,
            SonogramConfig sonoConfig,
            LewinsRailConfig lrConfig,
            bool returnDebugImage,
            TimeSpan segmentStartOffset)
        {
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            int sr = recording.SampleRate;

            int upperBandMinHz = lrConfig.UpperBandMinHz;
            int upperBandMaxHz = lrConfig.UpperBandMaxHz;
            int lowerBandMinHz = lrConfig.LowerBandMinHz;
            int lowerBandMaxHz = lrConfig.LowerBandMaxHz;

            //double decibelThreshold = lrConfig.DecibelThreshold;   //dB
            //int windowSize = lrConfig.WindowSize;
            double eventThreshold = lrConfig.EventThreshold; //in 0-1
            double minDuration = lrConfig.MinDuration;  // seconds
            double maxDuration = lrConfig.MaxDuration;  // seconds
            double minPeriod = lrConfig.MinPeriod;  // seconds
            double maxPeriod = lrConfig.MaxPeriod;  // seconds

            //double freqBinWidth = sr / (double)windowSize;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            //i: MAKE SONOGRAM
            double framesPerSecond = freqBinWidth;

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700

            int upperBandMinBin = (int)Math.Round(upperBandMinHz / freqBinWidth) + 1;
            int upperBandMaxBin = (int)Math.Round(upperBandMaxHz / freqBinWidth) + 1;
            int lowerBandMinBin = (int)Math.Round(lowerBandMinHz / freqBinWidth) + 1;
            int lowerBandMaxBin = (int)Math.Round(lowerBandMaxHz / freqBinWidth) + 1;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, lowerBandMinBin, rowCount - 1, lowerBandMaxBin);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, upperBandMinBin, rowCount - 1, upperBandMaxBin);

            int step = (int)Math.Round(framesPerSecond); //take one second steps
            int stepCount = rowCount / step;
            int sampleLength = 64; //64 frames = 3.7 seconds. Suitable for Lewins Rail.
            double[] intensity = new double[rowCount];
            double[] periodicity = new double[rowCount];

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES
            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(lowerArray, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(upperArray, start, sampleLength);
                if (lowerSubarray.Length != sampleLength || upperSubarray.Length != sampleLength)
                {
                    break;
                }

                var spectrum = AutoAndCrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 3;
                for (int s = 0; s < zeroCount; s++)
                {
                    spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                }

                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId / framesPerSecond; //convert maxID to period in seconds
                if (period < minPeriod || period > maxPeriod)
                {
                    continue;
                }

                // lay down score for sample length
                for (int j = 0; j < sampleLength; j++)
                {
                    if (intensity[start + j] < spectrum[maxId])
                    {
                        intensity[start + j] = spectrum[maxId];
                    }

                    periodicity[start + j] = period;
                }
            }

            //######################################################################

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 5);

            var predictedEvents = AcousticEvent.ConvertScoreArray2Events(
                intensity,
                lowerBandMinHz,
                upperBandMaxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                eventThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);

            CropEvents(predictedEvents, upperArray, segmentStartOffset);
            var hits = new double[rowCount, colCount];

            //######################################################################

            var scorePlot = new Plot("L.pect", intensity, lrConfig.IntensityThreshold);
            Image debugImage = null;
            if (returnDebugImage)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                DataTools.Normalise(intensity, lrConfig.DecibelThreshold, out normalisedScores, out normalisedThreshold);
                var intensityPlot = new Plot("Intensity", normalisedScores, normalisedThreshold);
                DataTools.Normalise(periodicity, 10, out normalisedScores, out normalisedThreshold);
                var periodicityPlot = new Plot("Periodicity", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scorePlot, intensityPlot, periodicityPlot };
                debugImage = DrawDebugImage(sonogram, predictedEvents, debugPlots, hits);
            }

            return Tuple.Create(sonogram, hits, intensity, predictedEvents, debugImage);
        } //Analysis()

        public static void CropEvents(List<AcousticEvent> events, double[] intensity, TimeSpan segmentStartOffset)
        {
            double severity = 0.1;
            int length = intensity.Length;

            foreach (var ev in events)
            {
                int start = ev.Oblong.RowTop;
                int end = ev.Oblong.RowBottom;
                double[] subArray = DataTools.Subarray(intensity, start, end - start + 1);
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length)
                {
                    newMaxRow = length - 1;
                }

                var o = new Oblong(newMinRow, ev.Oblong.ColumnLeft, newMaxRow, ev.Oblong.ColumnRight);
                ev.Oblong = o;
                ev.SetEventPositionRelative(segmentStartOffset, newMinRow * ev.FrameOffset, newMaxRow * ev.FrameOffset);
            }
        }

        private static Image DrawDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1KHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1KHzLines, doMelScale: false));

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
                // set colour for the events
                foreach (var ev in events)
                {
                    ev.BorderColour = AcousticEvent.DefaultBorderColor;
                    ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                }

                image.AddEvents(events, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
            }

            return image.GetImage();
        }
    } //end class Lewinia pectoralis - Lewin's Rail.

    public class LewinsRailConfig
    {
        public string AnalysisName { get; set; }

        public string SpeciesName { get; set; }

        public string AbbreviatedSpeciesName { get; set; }

        public int WindowSize { get; set; }

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

        public void ReadConfigFile(Config configuration, string profileName)
        {
            // common properties
            this.AnalysisName = configuration[AnalysisKeys.AnalysisName] ?? "<no name>";
            this.SpeciesName = configuration[AnalysisKeys.SpeciesName] ?? "<no name>";
            this.AbbreviatedSpeciesName = configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            // KEYS TO PARAMETERS IN CONFIG FILE
            const string keyUpperfreqbandTop = "UpperFreqBandTop";
            const string keyUpperfreqbandBtm = "UpperFreqBandBottom";
            const string keyLowerfreqbandTop = "LowerFreqBandTop";
            const string keyLowerfreqbandBtm = "LowerFreqBandBottom";

            // Config profile = ConfigFile.GetProfile(configuration, profileName);
            bool success = ConfigFile.TryGetProfile(configuration, profileName, out var profile);
            if (!success)
            {
                throw new InvalidOperationException($"The Config file for {this.SpeciesName} must contain a valid {profileName} profile.");
            }

            // extract parameters
            this.UpperBandMaxHz = profile.GetInt(keyUpperfreqbandTop);
            this.UpperBandMinHz = profile.GetInt(keyUpperfreqbandBtm);
            this.LowerBandMaxHz = profile.GetInt(keyLowerfreqbandTop);
            this.LowerBandMinHz = profile.GetInt(keyLowerfreqbandBtm);

            //double dctDuration = (double)profile[AnalysisKeys.DctDuration];
            // This is the intensity threshold above
            //double dctThreshold = (double)profile[AnalysisKeys.DctThreshold];

            // Periods and Oscillations
            this.MinPeriod = profile.GetDouble(AnalysisKeys.MinPeriodicity); //: 0.18
            this.MaxPeriod = profile.GetDouble(AnalysisKeys.MaxPeriodicity); //: 0.25

            // minimum duration in seconds of a trill event
            this.MinDuration = profile.GetDouble(AnalysisKeys.MinDuration); //:3

            // maximum duration in seconds of a trill event
            this.MaxDuration = profile.GetDouble(AnalysisKeys.MaxDuration); //: 15
            this.DecibelThreshold = profile.GetDoubleOrNull(AnalysisKeys.DecibelThreshold) ?? 3.0;

            //// minimum acceptable value of a DCT coefficient
            this.IntensityThreshold = profile.GetDoubleOrNull(AnalysisKeys.IntensityThreshold) ?? 0.4;
            this.EventThreshold = profile.GetDoubleOrNull(AnalysisKeys.EventThreshold) ?? 0.2;
        } // ReadConfigFile()
    } // class
}
