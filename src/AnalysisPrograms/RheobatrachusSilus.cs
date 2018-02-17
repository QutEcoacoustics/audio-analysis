// <copyright file="RheobatrachusSilus.cs" company="QutEcoacoustics">
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

    //"This recognizer is non functional. It's core should be ported to the new recognizer base immediately")
    //"Gastric Broooding Frog - Rheobatrachus silus";
    public class RheobatrachusSilus
    {
        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// </summary>
        public static Tuple<BaseSonogram, double[,], List<Plot>, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, TimeSpan segmentStartOffset)
        {
            //set default values - ignore those set by user
            int frameSize = 128;
            double windowOverlap = 0.5;

            double intensityThreshold = double.Parse(configDict["INTENSITY_THRESHOLD"]); //in 0-1
            double minDuration = double.Parse(configDict["MIN_DURATION"]);  // seconds
            double maxDuration = double.Parse(configDict["MAX_DURATION"]);  // seconds
            double minPeriod = double.Parse(configDict["MIN_PERIOD"]);      // seconds
            double maxPeriod = double.Parse(configDict["MAX_PERIOD"]);      // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,
                WindowSize = frameSize,
                WindowOverlap = windowOverlap,
                NoiseReductionType = SNR.KeyToNoiseReductionType("STANDARD"),
            }; //default values config

            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            TimeSpan tsRecordingtDuration = recording.Duration;
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double frameOffset = sonoConfig.GetFrameOffset(sr);
            double framesPerSecond = 1 / frameOffset;

            BaseSonogram sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);

            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 256      17640       14.5ms          68.9        68.9    ms          hz          hz
            // 512      17640       29.0ms          34.4        34.4    ms          hz          hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //The Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            // Assuming sr=17640 and window=256, then binWidth = 68.9Hz and 1500Hz = bin 21.7..
            // Therefore do a Xcorrelation between bins 21 and 22.
            // Number of frames to span must power of 2. Try 16 frames which covers 232ms - almost 1/4 second.

            int midHz = 1500;
            int lowerBin = (int)(midHz / freqBinWidth) + 1;  //because bin[0] = DC
            int upperBin = lowerBin + 4;
            int lowerHz = (int)Math.Floor((lowerBin - 1) * freqBinWidth);
            int upperHz = (int)Math.Ceiling((upperBin - 1) * freqBinWidth);

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;

            double[] lowerArray = MatrixTools.GetColumn(sonogram.Data, lowerBin);
            double[] upperArray = MatrixTools.GetColumn(sonogram.Data, upperBin);
            lowerArray = DataTools.NormaliseInZeroOne(lowerArray, 0, 60); //## ABSOLUTE NORMALISATION 0-60 dB #######################################################################
            upperArray = DataTools.NormaliseInZeroOne(upperArray, 0, 60); //## ABSOLUTE NORMALISATION 0-60 dB #######################################################################

            int step = (int)(framesPerSecond / 40); //take one/tenth second steps
            int stepCount = rowCount / step;
            int sampleLength = 32; //16 frames = 232ms - almost 1/4 second.
            double[] intensity = new double[rowCount];
            double[] periodicity = new double[rowCount];

            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES

            for (int i = 0; i < stepCount; i++)
            {
                int start = step * i;
                double[] lowerSubarray = DataTools.Subarray(lowerArray, start, sampleLength);
                double[] upperSubarray = DataTools.Subarray(upperArray, start, sampleLength);
                if (lowerSubarray == null || upperSubarray == null)
                {
                    break;
                }

                if (lowerSubarray.Length != sampleLength || upperSubarray.Length != sampleLength)
                {
                    break;
                }

                var spectrum = AutoAndCrossCorrelation.CrossCorr(lowerSubarray, upperSubarray);
                int zeroCount = 2;
                for (int s = 0; s < zeroCount; s++)
                {
                    spectrum[s] = 0.0;  //in real data these bins are dominant and hide other frequency content
                }

                int maxId = DataTools.GetMaxIndex(spectrum);
                double period = 2 * sampleLength / (double)maxId / framesPerSecond; //convert maxID to period in seconds
                if (period < minPeriod || period > maxPeriod)
                {
                    continue;
                }

                for (int j = 0; j < sampleLength; j++) //lay down score for sample length
                {
                    if (intensity[start + j] < spectrum[maxId])
                    {
                        intensity[start + j] = spectrum[maxId];
                    }

                    periodicity[start + j] = period;
                }
            }

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 3);
            intensity = DataTools.NormaliseInZeroOne(intensity, 0, 0.5); //## ABSOLUTE NORMALISATION 0-0.5 #######################################################################

            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(
                intensity,
                lowerHz,
                upperHz,
                sonogram.FramesPerSecond,
                freqBinWidth,
                intensityThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);
            CropEvents(predictedEvents, upperArray);
            var hits = new double[rowCount, colCount];

            var plots = new List<Plot>();

            //plots.Add(new Plot("lowerArray", DataTools.Normalise(lowerArray, 0, 100), 10.0));
            //plots.Add(new Plot("lowerArray", DataTools.Normalise(lowerArray, 0, 100), 10.0));
            //plots.Add(new Plot("lowerArray", DataTools.NormaliseMatrixValues(lowerArray), 0.25));
            //plots.Add(new Plot("upperArray", DataTools.NormaliseMatrixValues(upperArray), 0.25));
            //plots.Add(new Plot("intensity",  DataTools.NormaliseMatrixValues(intensity), intensityThreshold));
            plots.Add(new Plot("intensity", intensity, intensityThreshold));

            return Tuple.Create(sonogram, hits, plots, predictedEvents, tsRecordingtDuration);
        } //Analysis()

        public static void CropEvents(List<AcousticEvent> events, double[] intensity)
        {
            double severity = 0.1;
            int length = intensity.Length;

            foreach (AcousticEvent ev in events)
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

                Oblong o = new Oblong(newMinRow, ev.Oblong.ColumnLeft, newMaxRow, ev.Oblong.ColumnRight);
                ev.Oblong = o;
                ev.TimeStart = newMinRow * ev.FrameOffset;
                ev.TimeEnd = newMaxRow * ev.FrameOffset;
            }
        }
    }
}
