// <copyright file="Human1.cs" company="QutEcoacoustics">
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

    [Obsolete("This class has been dismembered keeping only hte core method")]
    public class Human1
    {
        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]

        public const string AnalysisName = "Human2";

        /// <summary>
        /// THIS IS THE CORE DETECTION METHOD
        /// Detects the human voice
        /// </summary>
        public static Tuple<BaseSonogram, double[,], Plot, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            //set default values
            int frameLength = 1024;
            if (configDict.ContainsKey(AnalysisKeys.FrameLength))
            {
                frameLength = int.Parse(configDict[AnalysisKeys.FrameLength]);
            }

            double windowOverlap = 0.0;

            int minHz = int.Parse(configDict["MIN_HZ"]);
            int minFormantgap = int.Parse(configDict["MIN_FORMANT_GAP"]);
            int maxFormantgap = int.Parse(configDict["MAX_FORMANT_GAP"]);
            double intensityThreshold = double.Parse(configDict["INTENSITY_THRESHOLD"]); //in 0-1
            double minDuration = double.Parse(configDict["MIN_DURATION"]);  // seconds
            double maxDuration = double.Parse(configDict["MAX_DURATION"]);  // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig
            {
                //default values config
                SourceFName = recording.BaseName,
                WindowSize = frameLength,
                WindowOverlap = windowOverlap,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD"),
            };
            var tsRecordingtDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;

            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

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

            //ii: DETECT HARMONICS
            int zeroBinCount = 4; //to remove low freq content which dominates the spectrum
            var results = CrossCorrelation.DetectBarsInTheRowsOfaMatrix(subMatrix, intensityThreshold, zeroBinCount);
            double[] intensity = results.Item1;
            double[] periodicity = results.Item2; //an array of periodicity scores

            //intensity = DataTools.filterMovingAverage(intensity, 3);
            //expect humans to have max power >100 and < 1000 Hz. Set these bounds
            int lowerHumanMaxBound = (int)(100 / freqBinWidth); //ignore 0-100 hz - too much noise
            int upperHumanMaxBound = (int)(3000 / freqBinWidth); //ignore above 2500 hz
            double[] scoreArray = new double[intensity.Length];
            for (int r = 0; r < rowCount; r++)
            {
                if (intensity[r] < intensityThreshold)
                {
                    continue;
                }

                //ignore locations with incorrect formant gap
                double herzPeriod = periodicity[r] * freqBinWidth;
                if ((herzPeriod < minFormantgap) || (herzPeriod > maxFormantgap))
                {
                    continue;
                }

                //find freq having max power and use info to adjust score.
                double[] spectrum = MatrixTools.GetRow(sonogram.Data, r);
                for (int j = 0; j < lowerHumanMaxBound; j++)
                {
                    spectrum[j] = 0.0;
                }

                for (int j = upperHumanMaxBound; j < spectrum.Length; j++)
                {
                    spectrum[j] = 0.0;
                }

                double[] peakvalues = DataTools.GetPeakValues(spectrum);
                int maxIndex1 = DataTools.GetMaxIndex(peakvalues);
                peakvalues[maxIndex1] = 0.0;
                int maxIndex2 = DataTools.GetMaxIndex(peakvalues);
                int avMaxBin = (maxIndex1 + maxIndex2) / 2;

                //int freqWithMaxPower = (int)Math.Round(maxIndex * freqBinWidth);
                int freqWithMaxPower = (int)Math.Round(avMaxBin * freqBinWidth);
                double discount = 1.0;
                if (freqWithMaxPower > 1000)
                {
                    discount = 0.0;
                }
                else
                    if (freqWithMaxPower < 500)
                {
                    discount = 0.0;
                }

                //set scoreArray[r]  - ignore locations with low intensity
                if (intensity[r] > intensityThreshold)
                {
                    scoreArray[r] = intensity[r] * discount;
                }
            }

            //transfer info to a hits matrix.
            var hits = new double[rowCount, colCount];
            double threshold = intensityThreshold * 0.75; //reduced threshold for display of hits
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
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(
                scoreArray,
                minHz,
                maxHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                intensityThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);

            //remove isolated speech events - expect humans to talk like politicians
            //predictedEvents = Human2.FilterHumanSpeechEvents(predictedEvents);
            Plot plot = new Plot(AnalysisName, intensity, intensityThreshold);
            return Tuple.Create(sonogram, hits, plot, predictedEvents, tsRecordingtDuration);
        } //Analysis()

        // THis method removes isolated speech events. Expect at least 2 events in 2 seconds
        public static List<AcousticEvent> FilterHumanSpeechEvents(List<AcousticEvent> events)
        {
            int count = events.Count;
            if (count < 2)
            {
                //require three speech events in space of three seconds to be a human speech event.
                return null;
            }

            bool[] partOfDouble = new bool[count];
            for (int i = 1; i < count; i++)
            {
                double gap = events[i].TimeStart - events[i - 1].TimeStart;
                if (gap < 2.0)
                {
                    partOfDouble[i - 1] = true;
                    partOfDouble[i] = true;
                }
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (!partOfDouble[i])
                {
                    events.Remove(events[i]);
                }
            }

            if (events.Count == 0)
            {
                events = null;
            }

            return events;
        }
    } //end class Human
}
