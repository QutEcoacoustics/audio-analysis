// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RhinellaMarina.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   AKA: The bloody canetoad
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using System.Diagnostics.Contracts;

    using AnalysisPrograms.Production;
    using AudioAnalysisTools.Indices;
    using System.Drawing;
    using Acoustics.Shared.Csv;

    /// <summary>
    /// AKA: Lewin's Rail
    /// This is call recognizer depends on an oscillation recognizer picking up the Kek-kek repeated at a period of 200ms
    /// 
    /// This recognizer was first developed for Jenny ???, a Masters student around 2007.
    /// It has been updated in October 2016 to become one of the new RecognizerBase recognizers. 
    /// To call this recognizer, the first command line argument must be "EventRecognizer".
    /// Alternatively, this recognizer can be called via the MultiRecognizer.
    /// </summary>



    public class LewiniaPectoralis : RecognizerBase
    {
        public override string Author => "Towsey";

        public override string SpeciesName => "LewiniaPectoralis";

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
        public const string ImageViewer = @"C:\Windows\system32\mspaint.exe";



        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="configuration"></param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="getSpectralIndexes"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="imageWidth"></param>
        /// <param name="audioRecording"></param>
        /// <returns></returns>
        public override RecognizerResults Recognize(AudioRecording recording, dynamic configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {

            // common properties
            string speciesName = (string)configuration[AnalysisKeys.SpeciesName] ?? "<no species>";
            string abbreviatedSpeciesName = (string)configuration[AnalysisKeys.AbbreviatedSpeciesName] ?? "<no.sp>";

            LewinsRailConfig.UpperBandMaxHz = (int)configuration[KeyUpperfreqbandTop];
            LewinsRailConfig.UpperBandMinHz = (int)configuration[KeyUpperfreqbandBtm];
            LewinsRailConfig.LowerBandMaxHz = (int)configuration[KeyLowerfreqbandTop];
            LewinsRailConfig.LowerBandMinHz = (int)configuration[KeyLowerfreqbandBtm];

            LewinsRailConfig.MinPeriod = (double)configuration["MinPeriod"]; //: 0.18
            LewinsRailConfig.MaxPeriod = (double)configuration["MaxPeriod"]; //: 0.25
            // minimum duration in seconds of an event
            LewinsRailConfig.MinDuration = (double)configuration[AnalysisKeys.MinDuration]; //:3
            // maximum duration in seconds of an event
            LewinsRailConfig.MaxDuration = (double)configuration[AnalysisKeys.MaxDuration]; //: 15
            // Use this threshold if averaging over a period - averaging seems to work better
            LewinsRailConfig.EventThreshold = (double)configuration["EventThreshold"]; //: 0.01

            // BETTER TO CALCULATE THIS. IGNORE USER!
            // double frameOverlap = Double.Parse(configDict[Keys.FRAME_OVERLAP]);
            // duration of DCT in seconds 
            //double dctDuration = (double)configuration[AnalysisKeys.DctDuration];

            //// minimum acceptable value of a DCT coefficient
            //double dctThreshold = (double)configuration[AnalysisKeys.DctThreshold];

            //// ignore oscillations below this threshold freq
            //int minOscilFreq = (int)configuration[AnalysisKeys.MinOscilFreq];

            //// ignore oscillations above this threshold freq
            int maxOscilRate = (int)Math.Ceiling(1 / LewinsRailConfig.MinPeriod);

            //// min score for an acceptable event
            //double eventThreshold = (double)configuration[AnalysisKeys.EventThreshold];

            // this default framesize seems to work for Lewins Rail
            const int FrameSize = 512;
            //const int FrameSize = 1024;

            double windowOverlap = Oscillations2012.CalculateRequiredFrameOverlap(
                recording.SampleRate,
                FrameSize,
                maxOscilRate);

            // i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.FileName,
                //set default values - ignor those set by user
                WindowSize = FrameSize,
                WindowOverlap = windowOverlap,
                // the default window is HAMMING
                //WindowFunction = WindowFunctions.HANNING.ToString(),
                //WindowFunction = WindowFunctions.NONE.ToString(),
                // if do not use noise reduction can get a more sensitive recogniser.
                //NoiseReductionType = NoiseReductionType.NONE,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD")
        };


            //BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            //int rowCount = sonogram.Data.GetLength(0);
            //int colCount = sonogram.Data.GetLength(1);

            //#############################################################################################################################################
            //DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var results = Analysis(recording, sonoConfig);
            //######################################################################

            if (results == null) return null; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scoreArray = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;

            //#############################################################################################################################################


            // sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            TimeSpan recordingDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            var prunedEvents = new List<AcousticEvent>();

            for (int i = 0; i < predictedEvents.Count; i++)
            {
                AcousticEvent ae = predictedEvents[i];

                // add additional info
                if (ae.Score > (LewinsRailConfig.EventThreshold))
                {
                    ae.Name = abbreviatedSpeciesName;
                    ae.SpeciesName = speciesName;
                    ae.SegmentStartOffset = segmentStartOffset;
                    ae.SegmentDuration = recordingDuration;
                    prunedEvents.Add(ae);
                }
            };

            // do a recognizer test.
            if (true)
            {
                string subDir = "/Test_LewinsRail/ExpectedOutput";
                var file = new FileInfo(recording.FilePath);
                var testDir = new DirectoryInfo(file.Directory.Parent.FullName + subDir);
                if (!testDir.Exists) testDir.Create();
                var fileName = file.Name.Substring(0, file.Name.Length - 9);

                Log.Info("# TEST1: Starting benchmark score array test for the Lewin's Rail recognizer:");
                string testName1 = "Check score array.";
                var scoreFilePath = Path.Combine(testDir.FullName, fileName + ".TestScores.csv");
                var scoreFile = new FileInfo(scoreFilePath);
                if (!scoreFile.Exists)
                {
                    Log.Warn("   Score Test file does not exist.    Writing output as future scores-test file");
                    FileTools.WriteArray2File(scoreArray, scoreFilePath);
                }
                else
                {
                    TestTools.RecognizerTest(testName1, scoreArray, new FileInfo(scoreFilePath));
                }

                Log.Info("# TEST2: Starting benchmark acoustic events test for the Lewin's Rail recognizer:");
                string testName2 = "Check events.";
                var benchmarkFilePath = Path.Combine(testDir.FullName, fileName + ".TestEvents.csv");
                var benchmarkFile = new FileInfo(benchmarkFilePath);
                if (!benchmarkFile.Exists)
                {
                    Log.Warn("   A file of test events does not exist.    Writing output as future events-test file");
                    Csv.WriteToCsv<EventBase>(benchmarkFile, prunedEvents);
                }
                else // compare the test events with benchmark
                {
                    var opDir = file.Directory.FullName + @"\" + Author +"."+ SpeciesName;
                    var eventsFilePath = Path.Combine(opDir, fileName + "__Events.csv");
                    var eventsFile = new FileInfo(eventsFilePath);
                    TestTools.FileEqualityTest(testName2, eventsFile, benchmarkFile);
                }
            }


            // increase very low scores
            for (int j = 0; j < scoreArray.Length; j++)
            {
                scoreArray[j] *= 4;
                if (scoreArray[j] > 1.0) scoreArray[j] = 1.0;
            }
            var plot = new Plot(this.DisplayName, scoreArray, LewinsRailConfig.EventThreshold);
            return new RecognizerResults()
            {
                Sonogram = sonogram,
                Hits = hits,
                Plots = plot.AsList(),
                Events = prunedEvents
                //Events = events
            };

        }



        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan> Analysis(AudioRecording recording, SonogramConfig sonoConfig)
        {
            int upperBandMinHz = LewinsRailConfig.UpperBandMinHz;
            int upperBandMaxHz = LewinsRailConfig.UpperBandMaxHz;
            int lowerBandMinHz = LewinsRailConfig.LowerBandMinHz;
            int lowerBandMaxHz = LewinsRailConfig.LowerBandMaxHz;
            double decibelThreshold = LewinsRailConfig.DecibelThreshold;   //dB
            double eventThreshold = LewinsRailConfig.EventThreshold; //in 0-1
            double minDuration = LewinsRailConfig.MinDuration;  // seconds
            double maxDuration = LewinsRailConfig.MaxDuration;  // seconds
            double minPeriod = LewinsRailConfig.MinPeriod;  // seconds
            double maxPeriod = LewinsRailConfig.MaxPeriod;  // seconds

            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            //i: MAKE SONOGRAM
            TimeSpan tsRecordingtDuration = recording.Duration();
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
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

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, lowerBandMinBin, (rowCount - 1), lowerBandMaxBin);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, upperBandMinBin, (rowCount - 1), upperBandMaxBin);

            int step = (int)Math.Round(framesPerSecond); //take one second steps
            int stepCount = rowCount / step;
            int sampleLength = 64; //64 frames = 3.7 seconds. Suitable for Lewins Rail.
            double[] intensity   = new double[rowCount];
            double[] periodicity = new double[rowCount]; 

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES
            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(lowerArray, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(upperArray, start, sampleLength);
                if ((lowerSubarray.Length != sampleLength) || (upperSubarray.Length != sampleLength)) break;
                var spectrum = AutoAndCrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 3;
                for (int s = 0; s < zeroCount; s++) spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                spectrum = DataTools.NormaliseArea(spectrum);
                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId / framesPerSecond; //convert maxID to period in seconds
                if ((period < minPeriod) || (period > maxPeriod)) continue;
                for (int j = 0; j < sampleLength; j++) //lay down score for sample length
                {
                    if (intensity[start + j] < spectrum[maxId]) intensity[start + j] = spectrum[maxId];
                    periodicity[start + j] = period;
                }
            }
            //######################################################################

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 5);
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(intensity, lowerBandMinHz, upperBandMaxHz, sonogram.FramesPerSecond, freqBinWidth,
                                                                                         eventThreshold, minDuration, maxDuration);
            CropEvents(predictedEvents, upperArray);
            var hits = new double[rowCount, colCount];

            return System.Tuple.Create(sonogram, hits, intensity, predictedEvents, tsRecordingtDuration);
        } //Analysis()




        public static void CropEvents(List<AcousticEvent> events, double[] intensity)
        {
            double severity = 0.1;
            int length = intensity.Length;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.Oblong.RowTop;
                int end   = ev.Oblong.RowBottom;
                double[] subArray = DataTools.Subarray(intensity, start, end-start+1);
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length) newMaxRow = length - 1;

                Oblong o = new Oblong(newMinRow, ev.Oblong.ColumnLeft, newMaxRow, ev.Oblong.ColumnRight);
                ev.Oblong = o;
                ev.TimeStart = newMinRow * ev.FrameOffset;
                ev.TimeEnd   = newMaxRow * ev.FrameOffset;
            }
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

    public static class LewinsRailConfig
    {
        public static int UpperBandMinHz { get; set; }
        public static int UpperBandMaxHz { get; set; }
        public static int LowerBandMinHz { get; set; }
        public static int LowerBandMaxHz { get; set; }
        public static double DecibelThreshold { get; set; }
        public static double MinDuration { get; set; }
        public static double MaxDuration { get; set; }
        public static double MinPeriod { get; set; }
        public static double MaxPeriod { get; set; }
        public static double EventThreshold { get; set; }
    }
}
