// <copyright file="Crow.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]

    //[Obsolete("This recognizer clsas has been dismembered, keeping only the core funcitonal method")]
    public class Crow
    {
        /// <summary>
        /// The CORE ANALYSIS METHOD
        /// </summary>
        public static Tuple<BaseSonogram, double[,], Plot, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            //set default values -
            int frameLength = 1024;
            if (configDict.ContainsKey(AnalysisKeys.FrameLength))
            {
                frameLength = int.Parse(configDict[AnalysisKeys.FrameLength]);
            }

            double windowOverlap = 0.0;
            int minHz = int.Parse(configDict["MIN_HZ"]);
            int minFormantgap = int.Parse(configDict["MIN_FORMANT_GAP"]);
            int maxFormantgap = int.Parse(configDict["MAX_FORMANT_GAP"]);
            double decibelThreshold = double.Parse(configDict["DECIBEL_THRESHOLD"]);  //dB
            double harmonicIntensityThreshold = double.Parse(configDict["INTENSITY_THRESHOLD"]); //in 0-1
            double callDuration = double.Parse(configDict["CALL_DURATION"]);  // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);

            //i: MAKE SONOGRAM
            var sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameLength,
                WindowOverlap = windowOverlap,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD"),
            }; //default values config

            TimeSpan tsRecordingtDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = freqBinWidth;

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int numberOfBins = 64;
            int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            int maxbin = minBin + numberOfBins - 1;
            int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, rowCount - 1, maxbin);

            int callSpan = (int)Math.Round(callDuration * framesPerSecond);

            //#############################################################################################################################################
            //ii: DETECT HARMONICS
            var results = CrossCorrelation.DetectHarmonicsInSonogramMatrix(subMatrix, decibelThreshold, callSpan);
            double[] dBArray = results.Item1;
            double[] intensity = results.Item2;     //an array of periodicity scores
            double[] periodicity = results.Item3;

            //intensity = DataTools.filterMovingAverage(intensity, 3);
            int noiseBound = (int)(100 / freqBinWidth); //ignore 0-100 hz - too much noise
            double[] scoreArray = new double[intensity.Length];
            for (int r = 0; r < rowCount; r++)
            {
                if (intensity[r] < harmonicIntensityThreshold)
                {
                    continue;
                }

                //ignore locations with incorrect formant gap
                double herzPeriod = periodicity[r] * freqBinWidth;
                if (herzPeriod < minFormantgap || herzPeriod > maxFormantgap)
                {
                    continue;
                }

                //find freq having max power and use info to adjust score.
                //expect humans to have max < 1000 Hz
                double[] spectrum = MatrixTools.GetRow(sonogram.Data, r);
                for (int j = 0; j < noiseBound; j++)
                {
                    spectrum[j] = 0.0;
                }

                int maxIndex = DataTools.GetMaxIndex(spectrum);
                int freqWithMaxPower = (int)Math.Round(maxIndex * freqBinWidth);
                double discount = 1.0;
                if (freqWithMaxPower < 1200)
                {
                    discount = 0.0;
                }

                if (intensity[r] > harmonicIntensityThreshold)
                {
                    scoreArray[r] = intensity[r] * discount;
                }
            }

            //transfer info to a hits matrix.
            var hits = new double[rowCount, colCount];
            double threshold = harmonicIntensityThreshold * 0.75; //reduced threshold for display of hits
            for (int r = 0; r < rowCount; r++)
            {
                if (scoreArray[r] < threshold)
                {
                    continue;
                }

                double herzPeriod = periodicity[r] * freqBinWidth;
                for (int c = minBin; c < maxbin; c++)
                {
                    //hits[r, c] = herzPeriod / (double)380;  //divide by 380 to get a relativePeriod;
                    hits[r, c] = (herzPeriod - minFormantgap) / maxFormantgap;  //to get a relativePeriod;
                }
            }

            //iii: CONVERT TO ACOUSTIC EVENTS
            double maxPossibleScore = 0.5;
            int halfCallSpan = callSpan / 2;
            var predictedEvents = new List<AcousticEvent>();
            for (int i = 0; i < rowCount; i++)
            {
                //assume one score position per crow call
                if (scoreArray[i] < 0.001)
                {
                    continue;
                }

                double startTime = (i - halfCallSpan) / framesPerSecond;
                AcousticEvent ev = new AcousticEvent(segmentStartOffset, startTime, callDuration, minHz, maxHz);
                ev.SetTimeAndFreqScales(framesPerSecond, freqBinWidth);
                ev.Score = scoreArray[i];
                ev.ScoreNormalised = ev.Score / maxPossibleScore; // normalised to the user supplied threshold

                //ev.Score_MaxPossible = maxPossibleScore;
                predictedEvents.Add(ev);
            } //for loop

            Plot plot = new Plot("CROW", intensity, harmonicIntensityThreshold);
            return Tuple.Create(sonogram, hits, plot, predictedEvents, tsRecordingtDuration);
        } //Analysis()
    }
}