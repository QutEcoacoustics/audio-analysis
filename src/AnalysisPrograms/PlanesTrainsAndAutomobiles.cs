// <copyright file="PlanesTrainsAndAutomobiles.cs" company="QutEcoacoustics">
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

    [Obsolete("This recognizer code was dismembered 23 Jan 2018. Only the core analysis methods were retained.")]
    public class PlanesTrainsAndAutomobiles
    {
        // CONSTANTS
        public const string AnalysisName = "Machine";

        //Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]

        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable.
        /// </summary>
        public static Tuple<BaseSonogram, double[,], Plot, List<AcousticEvent>, TimeSpan> Analysis(
            FileInfo fiSegmentOfSourceFile,
            Dictionary<string, string> configDict,
            TimeSpan segmentStartOffset)
        {
            string analysisName = configDict[AnalysisKeys.AnalysisName];
            int minFormantgap = int.Parse(configDict[AnalysisKeys.MinFormantGap]);
            int maxFormantgap = int.Parse(configDict[AnalysisKeys.MaxFormantGap]);
            int minHz = int.Parse(configDict[AnalysisKeys.MinHz]);
            double intensityThreshold = double.Parse(configDict[AnalysisKeys.IntensityThreshold]); //in 0-1
            double minDuration = double.Parse(configDict[AnalysisKeys.MinDuration]); // seconds
            int frameLength = 2048;
            if (configDict.ContainsKey(AnalysisKeys.FrameLength))
            {
                frameLength = int.Parse(configDict[AnalysisKeys.FrameLength]);
            }

            double windowOverlap = 0.0;
            if (frameLength == 1024)
            {
                //this is to make adjustment with other harmonic methods that use frame length = 1024
                frameLength = 2048;
                windowOverlap = 0.5;
            }

            var recording = new AudioRecording(fiSegmentOfSourceFile.FullName);

            //#############################################################################################################################################
            var results = DetectHarmonics(
                recording,
                intensityThreshold,
                minHz,
                minFormantgap,
                maxFormantgap,
                minDuration,
                frameLength,
                windowOverlap,
                segmentStartOffset); //uses XCORR and FFT

            //#############################################################################################################################################

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            foreach (AcousticEvent ev in predictedEvents)
            {
                ev.FileName = recording.BaseName;
                ev.Name = analysisName;
            }

            TimeSpan tsRecordingtDuration = recording.Duration;

            var plot = new Plot(AnalysisName, scores, intensityThreshold);
            return Tuple.Create(sonogram, hits, plot, predictedEvents, tsRecordingtDuration);
        } //Analysis()

        public static Tuple<BaseSonogram, double[,], double[], List<AcousticEvent>> DetectHarmonics(
            AudioRecording recording,
            double intensityThreshold,
            int minHz,
            int minFormantgap,
            int maxFormantgap,
            double minDuration,
            int windowSize,
            double windowOverlap,
            TimeSpan segmentStartOffset)
        {
            //i: MAKE SONOGRAM
            int numberOfBins = 32;
            double binWidth = recording.SampleRate / (double)windowSize;
            int sr = recording.SampleRate;
            double frameDuration = windowSize / (double)sr; // Duration of full frame or window in seconds
            double frameOffset = frameDuration * (1 - windowOverlap); //seconds between starts of consecutive frames
            double framesPerSecond = 1 / frameOffset;

            //double framesPerSecond = sr / (double)windowSize;
            //int frameOffset = (int)(windowSize * (1 - overlap));
            //int frameCount = (length - windowSize + frameOffset) / frameOffset;

            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            var results2 = DSP_Frames.ExtractEnvelopeAndAmplSpectrogram(
                recording.WavReader.Samples,
                sr,
                epsilon,
                windowSize,
                windowOverlap);
            double[] avAbsolute = results2.Average; //average absolute value over the minute recording

            //double[] envelope = results2.Item2;
            double[,]
                matrix = results2
                    .AmplitudeSpectrogram; //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            double windowPower = results2.WindowPower;

            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxHz = (int)Math.Round(minHz + (numberOfBins * binWidth));

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int maxbin = minBin + numberOfBins;
            double[,] subMatrix = MatrixTools.Submatrix(matrix, 0, minBin + 1, rowCount - 1, maxbin);

            //ii: DETECT HARMONICS
            int zeroBinCount = 5; //to remove low freq content which dominates the spectrum
            var results = CrossCorrelation.DetectBarsInTheRowsOfaMatrix(subMatrix, intensityThreshold, zeroBinCount);
            double[] intensity = results.Item1; //an array of periodicity scores
            double[] periodicity = results.Item2;

            //transfer periodicity info to a hits matrix.
            //intensity = DataTools.filterMovingAverage(intensity, 3);
            double[] scoreArray = new double[intensity.Length];
            var hits = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                double relativePeriod = periodicity[r] / numberOfBins / 2;
                if (intensity[r] > intensityThreshold)
                {
                    for (int c = minBin; c < maxbin; c++)
                    {
                        hits[r, c] = relativePeriod;
                    }
                }

                double herzPeriod = periodicity[r] * binWidth;
                if (herzPeriod > minFormantgap && herzPeriod < maxFormantgap)
                {
                    scoreArray[r] = 2 * intensity[r] * intensity[r]; //enhance high score wrt low score.
                }
            }

            scoreArray = DataTools.filterMovingAverage(scoreArray, 11);

            //iii: CONVERT TO ACOUSTIC EVENTS
            double maxDuration = 100000.0; //abitrary long number - do not want to restrict duration of machine noise
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(
                scoreArray,
                minHz,
                maxHz,
                framesPerSecond,
                binWidth,
                intensityThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);
            hits = null;

            //set up the songogram to return. Use the existing amplitude sonogram
            int bitsPerSample = recording.WavReader.BitsPerSample;
            TimeSpan duration = recording.Duration;
            NoiseReductionType nrt = SNR.KeyToNoiseReductionType("STANDARD");

            //Set the default values config
            SonogramConfig sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = windowSize,
                WindowOverlap = windowOverlap,
                NoiseReductionType = nrt,
                epsilon = Math.Pow(0.5, bitsPerSample - 1),
                WindowPower = windowPower,
                SampleRate = sr,
                Duration = duration,
            };

            var sonogram = new SpectrogramStandard(sonoConfig, matrix)
            {
                DecibelsNormalised = new double[rowCount],
            };

            //foreach frame or time step
            for (int i = 0; i < rowCount; i++)
            {
                sonogram.DecibelsNormalised[i] = 2 * Math.Log10(avAbsolute[i]);
            }

            sonogram.DecibelsNormalised = DataTools.normalise(sonogram.DecibelsNormalised);
            return Tuple.Create((BaseSonogram)sonogram, hits, scoreArray, predictedEvents);
        }
    }
}