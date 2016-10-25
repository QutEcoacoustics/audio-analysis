// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LitoriaWatjulumensis.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The bloody canetoad
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Recognizers.Base;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using TowseyLibrary;
    using AudioAnalysisTools.Indices;
    using Acoustics.Shared.Csv;
    using System.Drawing;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    /// <summary>
    /// To call this LitoriaWatjulumensis recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    ///
    /// This frog recognizer is based on the "kek-kek" recognizer for the Lewin's Rail
    /// It looks for synchronous oscillations in two frequency bands
    /// This recognizer has also been used for Litoria bicolor
    /// However the Correlation technique used for the Lewins Rail did not work because the ossilations in the upper and lower freq bands are not correlated.
    /// Instead measure the oscillations in the upper and lower bands independently. 
    /// </summary>



    public class LitoriaWatjulumensis : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LitoriaWatjulumensis";

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
        private const string ImageViewer = @"C:\Windows\system32\mspaint.exe";



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

            bool hasProfiles = ConfigFile.HasProfiles(configuration);
            if (!hasProfiles)
            {
                LoggedConsole.WriteFatalLine("The Config file for L.watjulum must contain profiles.", new Exception("Fatal error"));
            }
            string[] profileNames = ConfigFile.GetProfileNames(configuration);
            foreach (var name in profileNames)
            {
                LoggedConsole.WriteLine($"The Config file for L.watjulum contains the profile <{name}>.");
            }

            //            dynamic profile = ConfigFile.GetProfile(configuration, "Trill");
            dynamic trillProfile;
            bool success = ConfigFile.TryGetProfile(configuration, "Trill", out trillProfile);
            if (!success)
            {
                LoggedConsole.WriteFatalLine("The Config file for L.watjulum must contain a \"Trill\" profile.", new Exception("Fatal error"));
            }


            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";
            Log.Info($"Analyzing profile: {profileNames[0]}");
            
            // extract parameters
            LitoriaWatjulumConfig.UpperBandMaxHz = (int)trillProfile[KeyUpperfreqbandTop];
            LitoriaWatjulumConfig.UpperBandMinHz = (int)trillProfile[KeyUpperfreqbandBtm];
            LitoriaWatjulumConfig.LowerBandMaxHz = (int)trillProfile[KeyLowerfreqbandTop];
            LitoriaWatjulumConfig.LowerBandMinHz = (int)trillProfile[KeyLowerfreqbandBtm];

            // Periods and Oscillations
            LitoriaWatjulumConfig.MinPeriod = (double)trillProfile["MinPeriod"]; //: 0.18
            LitoriaWatjulumConfig.MaxPeriod = (double)trillProfile["MaxPeriod"]; //: 0.25
            int maxOscilRate = (int)Math.Ceiling(1 / LitoriaWatjulumConfig.MinPeriod);

            // minimum duration in seconds of an event
            LitoriaWatjulumConfig.MinDurationOfTrill = (double)trillProfile[AnalysisKeys.MinDuration]; //:3
            // maximum duration in seconds of an event
            LitoriaWatjulumConfig.MaxDurationOfTrill = (double)trillProfile[AnalysisKeys.MaxDuration]; //: 15
            LitoriaWatjulumConfig.DecibelThreshold = (double?)trillProfile["DecibelThreshold"] ?? 3.0;
            //// minimum acceptable value of a DCT coefficient
            LitoriaWatjulumConfig.IntensityThreshold = (double?)trillProfile["IntensityThreshold"] ?? 0.4;
            LitoriaWatjulumConfig.EventThreshold = (double?)trillProfile["EventThreshold"] ?? 0.2;

            //double dctDuration = (double)configuration[AnalysisKeys.DctDuration];
            // This is the intensity threshold above
            //double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            if (recording.WavReader.SampleRate != 22050)
            {
                throw new InvalidOperationException("Requires a 22050Hz file");
            }

            // this default framesize seems to work
            const int frameSize = 128;
            // calculate the overlap rather than get it from the user
            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                frameSize,
                maxOscilRate);

            TimeSpan recordingDuration = recording.WavReader.Time;


            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.FileName,
                //set default values - ignore those set by user
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD")
            };

            //#############################################################################################################################################
            //DO THE ANALYSIS
            var results = Analysis(recording, sonoConfig, outputDirectory);
            //######################################################################



            Log.Info($"Analyzing profile: {profileNames[1]}");
            dynamic tinkProfile;
            success = ConfigFile.TryGetProfile(configuration, "Tink", out tinkProfile);
            if (!success)
            {
                LoggedConsole.WriteFatalLine("The Config file for L.watjulum must contain a \"Tink\" profile.", new Exception("Fatal error"));
            }
            LitoriaWatjulumConfig.MinDurationOfTrill = (double)tinkProfile[AnalysisKeys.MinDuration]; //:3
            // maximum duration in seconds of an event
            LitoriaWatjulumConfig.MaxDurationOfTrill = (double)tinkProfile[AnalysisKeys.MaxDuration]; //: 15


            if (results == null) return null; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scoreArray = results.Item3;
            var predictedEvents = results.Item4;
            //var recordingTimeSpan = results.Item5;

            //#############################################################################################################################################

            // Prune events here if erquired i.e. remove those below threshold score if this not already done. See other recognizers.
            foreach (var ae in predictedEvents)
            {
                // add additional info
                ae.Name = abbreviatedSpeciesName;
                ae.SpeciesName = speciesName;
                ae.SegmentStartOffset = segmentStartOffset;
                ae.SegmentDuration = recordingDuration;
            };

            // TODO : Put a recognizer test in here if need one urgently!
            //if (true)
            //{
            //    string subDir = "/Test_LitoriaBicolor/ExpectedOutput";
            //}


            var plot = new Plot(this.DisplayName, scoreArray, LitoriaWatjulumConfig.EventThreshold);
            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = predictedEvents
            };

        }


        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD for TRILLS
        /// 
        /// See Anthony's ExempliGratia.Recognize() method in order to see how to use methods for config profiles.
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="sonoConfig"></param>
        /// <param name="outputDirectory"></param>
        /// <returns></returns>
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Analysis(AudioRecording recording, SonogramConfig sonoConfig, DirectoryInfo outputDirectory)
        {
            int upperBandMinHz = LitoriaWatjulumConfig.UpperBandMinHz;
            int upperBandMaxHz = LitoriaWatjulumConfig.UpperBandMaxHz;
            int lowerBandMinHz = LitoriaWatjulumConfig.LowerBandMinHz;
            int lowerBandMaxHz = LitoriaWatjulumConfig.LowerBandMaxHz;
            double decibelThreshold = LitoriaWatjulumConfig.DecibelThreshold;   //dB
            double intensityThreshold = LitoriaWatjulumConfig.IntensityThreshold;   
            double eventThreshold = LitoriaWatjulumConfig.EventThreshold; //in 0-1
            double minDuration = LitoriaWatjulumConfig.MinDurationOfTrill;  // seconds
            double maxDuration = LitoriaWatjulumConfig.MaxDurationOfTrill;  // seconds
            double minPeriod = LitoriaWatjulumConfig.MinPeriod;  // seconds
            double maxPeriod = LitoriaWatjulumConfig.MaxPeriod;  // seconds

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
            double dctDuration = 4 * maxPeriod;
            // duration of DCT in frames 
            int dctLength = (int)Math.Round(framesPerSecond * dctDuration);
            // set up the cosine coefficients
            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); 
            
            int upperBandMinBin = (int)Math.Round(upperBandMinHz / freqBinWidth) + 1;
            int upperBandMaxBin = (int)Math.Round(upperBandMaxHz / freqBinWidth) + 1;
            int lowerBandMinBin = (int)Math.Round(lowerBandMinHz / freqBinWidth) + 1;
            int lowerBandMaxBin = (int)Math.Round(lowerBandMaxHz / freqBinWidth) + 1;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            //int colCount = sonogram.Data.GetLength(1);

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, lowerBandMinBin, (rowCount - 1), lowerBandMaxBin);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, upperBandMinBin, (rowCount - 1), upperBandMaxBin);
            //lowerArray = DataTools.filterMovingAverage(lowerArray, 3);
            //upperArray = DataTools.filterMovingAverage(upperArray, 3);


            double[] amplitudeScores = DataTools.SumMinusDifference(lowerArray, upperArray);
            double[] differenceScores = DSP_Filters.SubtractBaseline(amplitudeScores, 7);

            // Could smooth here rather than above. Above seemed slightly better?
            //amplitudeScores = DataTools.filterMovingAverage(amplitudeScores, 7);
            //differenceScores = DataTools.filterMovingAverage(differenceScores, 7);


            //iii: CONVERT decibel sum-diff SCORES TO ACOUSTIC TRILL EVENTS
            var predictedTrillEvents = AcousticEvent.ConvertScoreArray2Events(amplitudeScores, lowerBandMinHz, upperBandMaxHz, sonogram.FramesPerSecond, 
                                                                          freqBinWidth, decibelThreshold, minDuration, maxDuration);

            for (int i = 0; i < differenceScores.Length; i++)
            {
                if (differenceScores[i] < 1.0)
                    differenceScores[i] = 0.0;
            }


            // LOOK FOR TRILL EVENTS
            // init the score array
            double[] scores = new double[rowCount];
            // var hits = new double[rowCount, colCount];
            double[,] hits = null;

            // init confirmed events
            var confirmedEvents = new List<AcousticEvent>();
            // add names into the returned events
            foreach (var ae in predictedTrillEvents)
            {
                //rowtop,  rowWidth
                int eventStart = ae.Oblong.RowTop;
                int eventWidth = ae.Oblong.RowWidth;
                int step = 2;
                double maximumIntensity = 0.0;

                // scan the event to get oscillation period and intensity
                for (int i = eventStart - (dctLength/2); i < eventStart + eventWidth - (dctLength/2); i += step)
                {
                    // Look for oscillations in the difference array
                    double[] differenceArray = DataTools.Subarray(differenceScores, i, dctLength);
                    double oscilFreq;
                    double period;
                    double intensity;
                    Oscillations2014.GetOscillation(differenceArray, framesPerSecond, cosines, out oscilFreq, out period, out intensity);

                    bool periodWithinBounds = (period > minPeriod) && (period < maxPeriod);
                    //Console.WriteLine($"step={i}    period={period:f4}");

                    if (!periodWithinBounds) continue;

                    for (int j = 0; j < dctLength; j++) //lay down score for sample length
                    {
                        if (scores[i + j] < intensity)
                            scores[i + j] = intensity;
                    }

                    if (maximumIntensity < intensity) maximumIntensity = intensity;

                }

                // add abbreviatedSpeciesName into event
                if (maximumIntensity >= intensityThreshold)
                {
                    ae.Name = "L.w";
                    ae.Score_MaxInEvent = maximumIntensity;
                    confirmedEvents.Add(ae);
                }

            }

            //######################################################################
            // LOOK FOR TINK EVENTS
            //iii: CONVERT decibel sum-diff SCORES TO ACOUSTIC EVENTS
            double minDurationOfTink = LitoriaWatjulumConfig.MinDurationOfTink;  // seconds
            double maxDurationOfTink = LitoriaWatjulumConfig.MaxDurationOfTink;  // seconds
            var predictedTinkEvents = AcousticEvent.ConvertScoreArray2Events(amplitudeScores, lowerBandMinHz, upperBandMaxHz, sonogram.FramesPerSecond,
                                                                          freqBinWidth, decibelThreshold, minDurationOfTink, maxDurationOfTink);

            // Calculate the cosine similarity of template to potential tinks

            //######################################################################

            var scorePlot = new Plot("L.watjulumensis", scores, intensityThreshold);

            //DEBUG IMAGE this recognizer only. MUST set false for deployment. 
            bool displayDebugImage = MainEntry.InDEBUG;
            if (displayDebugImage)
            {
                // display a variety of debug score arrays
                double[] normalisedScores;
                double normalisedThreshold;
                //DataTools.Normalise(scores, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var debugPlot = new Plot("Score", normalisedScores, normalisedThreshold);
                //DataTools.Normalise(upperArray, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var upperPlot = new Plot("Upper", normalisedScores, normalisedThreshold);
                //DataTools.Normalise(lowerArray, eventDecibelThreshold, out normalisedScores, out normalisedThreshold);
                //var lowerPlot = new Plot("Lower", normalisedScores, normalisedThreshold);
                DataTools.Normalise(amplitudeScores, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var sumDiffPlot = new Plot("Sum Minus Difference", normalisedScores, normalisedThreshold);
                DataTools.Normalise(differenceScores, decibelThreshold, out normalisedScores, out normalisedThreshold);
                var differencePlot = new Plot("Baseline Removed", normalisedScores, normalisedThreshold);

                var debugPlots = new List<Plot> { scorePlot, sumDiffPlot, differencePlot };
                // other debug plots
                //var debugPlots = new List<Plot> { scorePlot, upperPlot, lowerPlot, sumDiffPlot, differencePlot };
                var debugImage = DisplayDebugImage(sonogram, confirmedEvents, debugPlots, hits);
                var debugPath = outputDirectory.Combine(FilenameHelpers.AnalysisResultName(Path.GetFileNameWithoutExtension(recording.FileName), "LitoriaBicolor", "png", "DebugSpectrogram"));
                debugImage.Save(debugPath.FullName);
            }


            // return new sonogram because it makes for more easy interpretation of the image
            var returnSonoConfig = new SonogramConfig
            {
                SourceFName = recording.FileName,
                WindowSize = 512,
                WindowOverlap = 0,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD")
            };
            BaseSonogram returnSonogram = new SpectrogramStandard(returnSonoConfig, recording.WavReader);
            return Tuple.Create(returnSonogram, hits, scores, confirmedEvents);
        } //Analysis()




        //public static void CropEvents(List<AcousticEvent> events, double[] intensity)
        //{
        //    double severity = 0.1;
        //    int length = intensity.Length;

        //    foreach (AcousticEvent ev in events)
        //    {
        //        int start = ev.Oblong.RowTop;
        //        int end   = ev.Oblong.RowBottom;
        //        double[] subArray = DataTools.Subarray(intensity, start, end-start+1);
        //        int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

        //        int newMinRow = start + bounds[0];
        //        int newMaxRow = start + bounds[1];
        //        if (newMaxRow >= length) newMaxRow = length - 1;

        //        Oblong o = new Oblong(newMinRow, ev.Oblong.ColumnLeft, newMaxRow, ev.Oblong.ColumnRight);
        //        ev.Oblong = o;
        //        ev.TimeStart = newMinRow * ev.FrameOffset;
        //        ev.TimeEnd   = newMaxRow * ev.FrameOffset;
        //    }
        //}



        public static Image DisplayDebugImage(BaseSonogram sonogram, List<AcousticEvent> events, List<Plot> scores, double[,] hits)
        {
            const bool doHighlightSubband = false;
            const bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));

            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            if (scores != null)
            {
                foreach (var plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (hits != null) image.OverlayRainbowTransparency(hits);

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


        // KEYS TO PARAMETERS IN CONFIG FILE
        public const string KeyAnalysisName = AnalysisKeys.AnalysisName;
        public const string KeyCallDuration = AnalysisKeys.CallDuration;
        public const string KeyDecibelThreshold = AnalysisKeys.DecibelThreshold;
        public const string KeyEventThreshold = AnalysisKeys.EventThreshold;
        public const string KeyIntensityThreshold = AnalysisKeys.IntensityThreshold;
        public const string KeySegmentDuration = AnalysisKeys.SegmentDuration;
        public const string KeySegmentOverlap = AnalysisKeys.SegmentOverlap;
        public const string KeyResampleRate = AnalysisKeys.ResampleRate;
        public const string KeyFrameLength = AnalysisKeys.FrameLength;
        public const string KeyFrameOverlap = AnalysisKeys.FrameOverlap;
        public const string KeyNoiseReductionType = AnalysisKeys.NoiseReductionType;
        public const string KeyUpperfreqbandTop = "UpperFreqBandTop";
        public const string KeyUpperfreqbandBtm = "UpperFreqBandBottom";
        public const string KeyLowerfreqbandTop = "LowerFreqBandTop";
        public const string KeyLowerfreqbandBtm = "LowerFreqBandBottom";
        public const string KeyMinAmplitude = AnalysisKeys.MinAmplitude;
        public const string KeyMinDuration = AnalysisKeys.MinDuration;
        public const string KeyMaxDuration = AnalysisKeys.MaxDuration;
        public const string KeyMinPeriod = AnalysisKeys.MinPeriodicity;
        public const string KeyMaxPeriod = AnalysisKeys.MaxPeriodicity;
        public const string KeyDrawSonograms = AnalysisKeys.KeyDrawSonograms;

        // KEYS TO OUTPUT EVENTS and INDICES
        public const string KeyCount = "count";
        public const string KeySegmentTimespan = "SegTimeSpan";
        public const string KeyStartAbs = "EvStartAbs";
        public const string KeyStartMin = "EvStartMin";
        public const string KeyStartSec = "EvStartSec";
        public const string KeyCallDensity = "CallDensity";
        public const string KeyCallScore = "CallScore";
        public const string KeyEventTotal = "# events";
    } //end class Lewinia pectoralis - Lewin's Rail.

    public static class LitoriaWatjulumConfig
    {
        public static int UpperBandMinHz { get; set; }
        public static int UpperBandMaxHz { get; set; }
        public static int LowerBandMinHz { get; set; }
        public static int LowerBandMaxHz { get; set; }
        public static double MinPeriod { get; set; }
        public static double MaxPeriod { get; set; }
        public static double IntensityThreshold { get; set; }
        public static double DecibelThreshold { get; set; }
        public static double MinDurationOfTrill { get; set; }
        public static double MaxDurationOfTrill { get; set; }
        public static double EventThreshold { get; set; }
        public static double MinDurationOfTink { get; set; }
        public static double MaxDurationOfTink { get; set; }
    }
}
